using System.IO.Compression;
using System.Text.RegularExpressions;
using Exam.Domain.Entities;
using Exam.Domain.Enums;
using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Features.Submission.Commands.CreateSubmissionsFromZipCommand;
using Exam.Services.Interfaces.Services;
using Exam.Services.Models.Configurations;
using Exam.Services.Models.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Exam.Services.Services;

public class SubmissionService : ISubmissionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAzureBlobService _blobService;
    private readonly BlobSettings _blobSettings;
    
    public SubmissionService(IUnitOfWork unitOfWork, IAzureBlobService blobService, IOptions<BlobSettings> blobSettings)
    {
        _unitOfWork = unitOfWork;
        _blobService = blobService;
        _blobSettings = blobSettings.Value;
    }

    public async Task<DataServiceResponse<List<Guid>>> CreateSubmissionsFromZipAsync(
    CreateSubmissionsFromZipCommand command,
    CancellationToken cancellationToken = default)
    {
        if (command.ZipFile is null || command.ZipFile.Length == 0)
            return new DataServiceResponse<List<Guid>>
            {
                Success = false,
                Message = "Zip file is required."
            };
        if (!Path.GetExtension(command.ZipFile.FileName).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            return new DataServiceResponse<List<Guid>>
            {
                Success = false, Message = "Only .zip is supported."
            };
        var (subjectCode, examCode) = await ResolveCodesAsync(command.ExamSubjectId, cancellationToken);
        var rootFolder = $"{subjectCode}_{examCode}"; 
        var container  = _blobSettings.DefaultContainer;

        var createdIds = new List<Guid>();

        await using var mem = new MemoryStream();
        await command.ZipFile.CopyToAsync(mem, cancellationToken);
        mem.Position = 0;

        using var zip = new ZipArchive(mem, ZipArchiveMode.Read);

        // Group theo thư mục top-level (phần tử đầu trong đường dẫn)
        var groups = zip.Entries
            .Where(e => !string.IsNullOrEmpty(e.Name))  // bỏ folder
            .Select(e => new { Entry = e, Path = NormalizeZipPath(e.FullName) })
            .GroupBy(x => x.Path.Split('/')[0], StringComparer.OrdinalIgnoreCase);

        foreach (var g in groups)
        {
            var topLevel = g.Key;
            //Tạo gói zip cho riêng thư mục top-level
            await using var zms = new MemoryStream();
            using (var z = new ZipArchive(zms, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var x in g)
                {
                    // Bỏ tên top-level để zip bên trong gọn gàng (0/solution.zip,...)
                    var parts = x.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    var inside = string.Join('/', parts.Skip(1));      // "0/solution.zip"
                    var entryName = string.IsNullOrWhiteSpace(inside) ? x.Entry.Name : inside;

                    var newEntry = z.CreateEntry(entryName, CompressionLevel.SmallestSize);
                    await using var src = x.Entry.Open();
                    await using var dst = newEntry.Open();
                    await src.CopyToAsync(dst, cancellationToken);
                }
            }
            
            zms.Position = 0;
            var topZipBlob = $"{rootFolder}/{topLevel}.zip";      
            await _blobService.UploadAsync(zms, topZipBlob, container);
            var sasUrl = _blobService.GetReadSasUrl(container, topZipBlob, TimeSpan.FromDays(7));
            
            var submission = new Submission
            {
                Id = Guid.NewGuid(),
                ExaminerId = command.ExaminerId,
                ExamSubjectId = command.ExamSubjectId,
                ModeratorId = command.ModeratorId,
                AssignAt = DateTimeOffset.UtcNow,
                Status = SubmissionStatus.Processing,
                FileUrl = sasUrl
            };

            await _unitOfWork.GetRepository<Submission>().InsertAsync(submission, cancellationToken);
            createdIds.Add(submission.Id);
        }

        await _unitOfWork.SaveChangesAsync();

        if(createdIds.Count == 0)
            return new DataServiceResponse<List<Guid>>()
            {
                Success = false,
                Message = "Zip has no files."
            };
        
        return new DataServiceResponse<List<Guid>>()
        {
            Success = true,
            Message = "Created submissions successfully.",
            Data = createdIds
        };
    }


    #region Helpers
    public async Task<(string subjectCode, string examCode)> ResolveCodesAsync(Guid examSubjectId, CancellationToken ct)
    {
        var es = await _unitOfWork.GetRepository<ExamSubject>().Query()
            .Include(x => x.Subject)
            .Include(x => x.Exam)
            .Where(x => x.Id == examSubjectId)
            .Select(x => new {
                SubjectCode = x.Subject.Code,
                ExamCode = x.Exam.Code
            })
            .FirstOrDefaultAsync(ct);

        if (es == null) throw new InvalidOperationException($"ExamSubject {examSubjectId} not found");

        //xóa khoảng trắng, ký tự lạ
        string Norm(string s) => Regex.Replace(s.Trim(), @"[^A-Za-z0-9_\-\.]+", "_");
        return (Norm(es.SubjectCode), Norm(es.ExamCode));
    }
    
    private static string NormalizeZipPath(string raw)
    {
        var p = raw.Replace('\\', '/').Trim();
        if (p.StartsWith("/")) p = p.TrimStart('/');
        if (p.EndsWith("/")) return string.Empty;                  // folder
        if (p.Contains("../") || p.Contains("..\\")) 
            throw new InvalidOperationException("Invalid path traversal in zip.");
        return p;
    }
    #endregion
   
}
