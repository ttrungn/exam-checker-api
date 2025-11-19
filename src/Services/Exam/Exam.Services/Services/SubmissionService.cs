using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;
using Exam.Domain.Entities;
using Exam.Domain.Enums;
using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Exceptions;
using Exam.Services.Features.Submissions.Commands.CreateSubmissionsFromZipCommand;
using Exam.Services.Features.Submissions.Commands.UploadSubmissionFromZipCommand;
using Exam.Services.Interfaces.Services;
using Exam.Services.Models.Configurations;
using Exam.Services.Models.Responses;
using Exam.Services.Models.Validations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharpCompress.Archives;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace Exam.Services.Services;

public class SubmissionService : ISubmissionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAzureBlobService _blobService;
    private readonly BlobSettings _blobSettings;
    private readonly ILogger<SubmissionService> _logger;
    private readonly IViolationService _violationService;

    public SubmissionService(
        IUnitOfWork unitOfWork,
        IAzureBlobService blobService,
        IOptions<BlobSettings> blobSettings,
        ILogger<SubmissionService> logger,
        IViolationService validationService
        )
    {
        _unitOfWork   = unitOfWork;
        _blobService  = blobService;
        _blobSettings = blobSettings.Value;
        _logger       = logger;
        _violationService = validationService;
    }
    
    public async Task<DataServiceResponse<List<Guid>>> CreateSubmissionsFromZipAsync(
        CreateSubmissionsFromZipCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("CreateSubmissionsFromZipAsync invoked: ExamSubjectId={ExamSubjectId}, FileName={FileName}", 
            command.ExamSubjectId, command.ArchiveFile?.FileName);
        

        try
        {
            await using var mem = new MemoryStream();
            await command.ArchiveFile!.CopyToAsync(mem, cancellationToken);
            mem.Position = 0;

            var result = await ProcessZipCoreAsync(
                mem,
                command.ArchiveFile.FileName,
                command.ExamSubjectId,
                command.ExaminerId,
                command.ModeratorId,
                cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation("CreateSubmissionsFromZipAsync success: Created {Count} submissions", result.Data?.Count ?? 0);
            }
            else
            {
                _logger.LogWarning("CreateSubmissionsFromZipAsync failed: {Message}", result.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing zip file for ExamSubjectId={ExamSubjectId}", command.ExamSubjectId);
            throw new ServiceUnavailableException("Không thể xử lý file zip lúc này, vui lòng liên hệ admin để được hỗ trợ.");
        }
    }
    
    // Service upload zip to azure blob storage
    public async Task<DataServiceResponse<Guid>> UploadZipForProcessingAsync(UploadSubmissionFromZipCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("UploadZipForProcessingAsync invoked: ExamSubjectId={ExamSubjectId}, FileName={FileName}", 
            command.ExamSubjectId, command.ArchiveFile?.FileName);
        
        ExamSubject? exist;
        try
        {
            var examSubjectRepo = _unitOfWork.GetRepository<ExamSubject>();
            exist = await examSubjectRepo.Query()
                .Where(es=> es.Id == command.ExamSubjectId)
                .FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching ExamSubject {ExamSubjectId}", command.ExamSubjectId);
            throw new ServiceUnavailableException("Không thể kiểm tra ExamSubject lúc này, vui lòng liên hệ admin để được hỗ trợ.");
        }
        
        if (exist == null)
        {
            _logger.LogWarning("ExamSubject {ExamSubjectId} not found", command.ExamSubjectId);
            throw new NotFoundException($"ExamSubjectId {command.ExamSubjectId} không tồn tại.");
        }

        Domain.Entities.Exam? exam;
        try
        {
            var examRepo = _unitOfWork.GetRepository<Domain.Entities.Exam>();
            exam = await examRepo.Query()
                .Where(e => e.Id == exist.ExamId).FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Exam for ExamSubjectId {ExamSubjectId}", command.ExamSubjectId);
            throw new ServiceUnavailableException("Không thể kiểm tra Exam lúc này, vui lòng liên hệ admin để được hỗ trợ.");
        }

        if (exam == null)
        {
            _logger.LogWarning("Exam for ExamSubjectId {ExamSubjectId} not found", command.ExamSubjectId);
            throw new NotFoundException($"Exam for ExamSubjectId {command.ExamSubjectId} không tồn tại.");
        }
        
        try
        {
            var uploadsContainer = _blobSettings.UploadsContainer ?? "uploads";

            // path/metadata cho trigger
            // vd: uploads/{examSubjectId}/{originalName}
            var blobName = $"{command.ExamSubjectId}/{command.ExaminerId}/{command.ModeratorId}/{command.ArchiveFile!.FileName}";

            await using var stream = command.ArchiveFile.OpenReadStream();
            await _blobService.UploadAsync(stream, blobName, uploadsContainer);
            _logger.LogInformation("UploadZipForProcessingAsync success: Uploaded to {BlobName}", blobName);

            return new()
            {
                Success = true,
                Message = "Upload zip thành công, Function sẽ xử lý nền."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading zip to Azure Blob Storage for ExamSubjectId={ExamSubjectId}", command.ExamSubjectId);
            throw new ServiceUnavailableException("Không thể upload file lúc này, vui lòng liên hệ admin để được hỗ trợ.");
        }
    }


    #region Helpers
    
    // Core logic dùng chung cho cả API & Function
    private async Task<DataServiceResponse<List<Guid>>> ProcessZipCoreAsync(
        Stream stream,
        string zipFileName,
        Guid examSubjectId,
        Guid? examinerId,
        Guid moderatorId,
        CancellationToken ct)
    {
        _logger.LogInformation("ProcessZipCoreAsync invoked: ExamSubjectId={ExamSubjectId}, FileName={FileName}", 
            examSubjectId, zipFileName);

        string subjectCode, examCode;
        try
        {
            (subjectCode, examCode) = await ResolveCodesAsync(examSubjectId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving codes for ExamSubjectId={ExamSubjectId}", examSubjectId);
            throw new ServiceUnavailableException("Không thể lấy thông tin ExamSubject lúc này, vui lòng liên hệ admin để được hỗ trợ.");
        }

        var rootFolder = $"{subjectCode}_{examCode}";
        var container  = _blobSettings.DefaultContainer;

        var createdIds    = new List<Guid>();

        var submissionRepo = _unitOfWork.GetRepository<Submission>();
        var violationRepo = _unitOfWork.GetRepository<Violation>();
        var examSubjectRepo = _unitOfWork.GetRepository<ExamSubject>();
        var assessmentRepo   = _unitOfWork.GetRepository<Assessment>();

        var examSubject = await examSubjectRepo.Query()
            .FirstOrDefaultAsync(es => es.Id == examSubjectId, ct); 
        var rules = new ValidationRules();
        try
        {
            rules = examSubject != null
            ? JsonSerializer.Deserialize<ValidationRules>(examSubject.ViolationStructure ?? "[]")
            : new ValidationRules();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Violation structure mismatch with ValidationRules {ExamSubjectId}", examSubjectId);
            throw;
        }


        var submissionList = new List<Submission>();

        try
        {
            using var archive = ArchiveFactory.Open(stream);

            // group theo thư mục sinh viên
            var groups = archive.Entries
                .Where(e => !e.IsDirectory && !string.IsNullOrEmpty(e.Key))
                .Select(e => new { Entry = e, Path = NormalizeArchivePath(e.Key!) })
                .Where(x => !string.IsNullOrEmpty(x.Path))
                .GroupBy(x => x.Path.Split('/')[0], StringComparer.OrdinalIgnoreCase);

            foreach (var g in groups)
            {
                var topLevel = g.Key;

                try
                {

                    // Create zip for each student
                    await using var zms = new MemoryStream();
                    using (var studentZipWriter = new ZipArchive(zms, ZipArchiveMode.Create, leaveOpen: true))
                    {
                        foreach (var x in g)
                        {
                            var parts      = x.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                            var insidePath = string.Join('/', parts.Skip(1));
                            var entryName = string.IsNullOrWhiteSpace(insidePath)
                                ? System.IO.Path.GetFileName(x.Entry.Key ?? "")
                                : insidePath;

                            var newEntry = studentZipWriter.CreateEntry(entryName, CompressionLevel.SmallestSize);
                            await using var src = x.Entry.OpenEntryStream();
                            await using var dst = newEntry.Open();
                            await src.CopyToAsync(dst, ct);
                        }
                    }

                    zms.Position = 0;
                    // upload each zip file to blob storage
                    var blobPath = $"{rootFolder}/{topLevel}.zip";
                    await _blobService.UploadAsync(zms, blobPath, container);
                    var sasUrl = _blobService.GetReadSasUrl(container, blobPath, TimeSpan.FromDays(7));

                    var submission = new Submission
                    {
                        Id            = Guid.NewGuid(),
                        ExaminerId    = examinerId,
                        ModeratorId   = moderatorId,
                        ExamSubjectId = examSubjectId,
                        AssignAt      = DateTimeOffset.UtcNow,
                        Status        = SubmissionStatus.Processing,
                        GradeStatus   = GradeStatus.NotGraded,
                        FileUrl       = sasUrl
                    };

                    await submissionRepo.InsertAsync(submission, ct);
                    createdIds.Add(submission.Id);
                    submissionList.Add(submission);
                    
                    //create assessment record
                    var assessment = new Assessment
                    {
                        SubmissionId   = submission.Id,
                        ExaminerId     = examinerId.Value,
                        StudentCode    = topLevel,
                        SubmissionName = topLevel, 
                        Status         = AssessmentStatus.Pending,
                    };
                        await assessmentRepo.InsertAsync(assessment, ct);
                        _logger.LogDebug("Created assessment {AssessmentId} for submission {SubmissionId} (student: {StudentCode})",
                            assessment.Id, submission.Id, topLevel);
                    
                    
                    // Reset stream position for validation 
                    zms.Position = 0;

                    using var readableZip = new ZipArchive(zms, ZipArchiveMode.Read, leaveOpen: true);

                    var violationResult = await _violationService.ValidateSubmissionAsync(
                        submission.Id,
                        readableZip,
                        rules!,
                        sasUrl,
                        ct);

                    if (violationResult.Any())
                    {
                        await violationRepo.InsertAsync(violationResult, ct);
                        submission.Status = SubmissionStatus.Violated;
                    } 
                    else
                    {
                        submission.Status = SubmissionStatus.Validated;
                    }
                        _logger.LogDebug("Created submission {SubmissionId} for {TopLevel}", submission.Id, topLevel);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing group {TopLevel} for ExamSubjectId={ExamSubjectId}", topLevel, examSubjectId);
                    continue;
                }
            }

            await _unitOfWork.SaveChangesAsync();

            if (createdIds.Count <= 0)
            {
                _logger.LogWarning("ProcessZipCoreAsync: No files found in zip");
                return new()
                {
                    Success = false,
                    Message = "Zip has no files."
                };
            }

            _logger.LogInformation("ProcessZipCoreAsync success: Created {Count} submissions", createdIds.Count);


            return new()
            {
                Success = true,
                Message = "Created submissions successfully.",
                Data    = createdIds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing zip archive for ExamSubjectId={ExamSubjectId}", examSubjectId);
            throw new ServiceUnavailableException("Không thể xử lý file zip lúc này, vui lòng liên hệ admin để được hỗ trợ.");
        }
    }
    // Lấy mã môn & mã kỳ thi để đặt thư mục
    private async Task<(string subjectCode, string examCode)> ResolveCodesAsync(Guid examSubjectId, CancellationToken ct)
    {
        var es = await _unitOfWork.GetRepository<ExamSubject>().Query()
            .Include(x => x.Subject)
            .Include(x => x.Exam)
            .Where(x => x.Id == examSubjectId)
            .Select(x => new
            {
                SubjectCode = x.Subject.Code,
                ExamCode    = x.Exam.Code
            })
            .FirstOrDefaultAsync(ct);

        if (es == null)
        {
            _logger.LogError("ExamSubject {ExamSubjectId} not found", examSubjectId);
            throw new NotFoundException($"ExamSubject {examSubjectId} không tồn tại");
        }

        string Norm(string s) => Regex.Replace(s.Trim(), @"[^A-Za-z0-9_\-\.]+", "_");
        return (Norm(es.SubjectCode), Norm(es.ExamCode));
    }

    // Chuẩn hóa đường dẫn trong zip
    private static string NormalizeZipPath(string raw)
    {
        var p = raw.Replace('\\', '/').Trim();
        if (p.StartsWith("/")) p = p.TrimStart('/');
        if (p.EndsWith("/"))  return string.Empty;
        if (p.Contains("../") || p.Contains("..\\"))
            throw new InvalidOperationException("Invalid path traversal in zip.");
        return p;
    }
    private static string NormalizeArchivePath(string raw)
    {
        var p = raw.Replace('\\', '/').Trim();
        if (p.StartsWith("/")) p = p.TrimStart('/');
        if (p.EndsWith("/")) return string.Empty;
        if (p.Contains("../") || p.Contains("..\\"))
            throw new InvalidOperationException("Invalid path traversal in archive.");
        return p;
    }
    #endregion
  
}
