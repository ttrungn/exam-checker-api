using ClosedXML.Excel;
using Exam.Domain.Entities;
using Exam.Domain.Enums;
using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Exceptions;
using Exam.Services.Models.Responses;
using Exam.Services.Models.Responses.Assessments;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.Assessments.Queries;

public record ExportExamResultsQuery : IRequest<DataServiceResponse<FileServiceResponse>>
{
    public Guid ExamId { get; init; }
    public Guid SubjectId { get; init; }
}

public class ExportExamResultsQueryValidator : AbstractValidator<ExportExamResultsQuery>
{
    public ExportExamResultsQueryValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty().WithMessage("Vui lòng chọn kỳ thi (Exam)!");
        RuleFor(x => x.SubjectId).NotEmpty().WithMessage("Vui lòng chọn môn học (Subject)!");
    }
}
public class ExportExamResultsQueryHandler
    : IRequestHandler<ExportExamResultsQuery, DataServiceResponse<FileServiceResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExportExamResultsQueryHandler> _logger;

    public ExportExamResultsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<ExportExamResultsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger     = logger;
    }

    public async Task<DataServiceResponse<FileServiceResponse>> Handle(
        ExportExamResultsQuery request,
        CancellationToken cancellationToken)
    {
        var submissionRepo = _unitOfWork.GetRepository<Submission>();

        // Lấy tất cả submissions của exam và subject này
        var allSubmissions = await submissionRepo.Query()
            .Include(s => s.ExamSubject)
                .ThenInclude(es => es!.Exam)
            .Include(s => s.ExamSubject)
                .ThenInclude(es => es!.Subject)
            .Include(s => s.Assessments)
            .Where(s =>
                s.ExamSubject != null &&
                s.ExamSubject.ExamId == request.ExamId &&
                s.ExamSubject.SubjectId == request.SubjectId)
            .ToListAsync(cancellationToken);

        if (!allSubmissions.Any())
        {
            throw new NotFoundException("Không tìm thấy bài nộp nào cho kỳ thi và môn học này.");
        }

        var totalSubmissions = allSubmissions.Count;

        // Lấy ExamCode và SubjectCode
        var examCode = allSubmissions.First().ExamSubject?.Exam?.Code ?? "Unknown";
        var subjectCode = allSubmissions.First().ExamSubject?.Subject?.Code ?? "Unknown";

        // Lấy submissions đã được duyệt điểm (Approved hoặc Graded)
        var approvedSubmissions = allSubmissions
            .Where(s => s.GradeStatus == GradeStatus.Approved || s.GradeStatus == GradeStatus.Graded)
            .ToList();

        // Tạo file Excel
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Results");

        // Header
        ws.Cell(1, 1).Value = "No";
        ws.Cell(1, 2).Value = "Student Code";
        ws.Cell(1, 3).Value = "Submission Name";
        ws.Cell(1, 4).Value = "Total Score";
        ws.Cell(1, 5).Value = "Comment";

        // Style header
        var headerRange = ws.Range(1, 1, 1, 5);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

        var row = 2;
        var index = 1;

        // Export các submission đã duyệt
        foreach (var submission in approvedSubmissions.OrderBy(s => s.Assessments.FirstOrDefault()?.StudentCode)
                     .ThenBy(s => s.Assessments.FirstOrDefault()?.SubmissionName))
        {
            // Lấy assessment Complete của submission này
            var assessment = submission.Assessments
                .FirstOrDefault(a => a.Status == AssessmentStatus.Complete);

            if (assessment != null)
            {
                ws.Cell(row, 1).Value = index;
                ws.Cell(row, 2).Value = assessment.StudentCode ?? string.Empty;
                ws.Cell(row, 3).Value = assessment.SubmissionName ?? string.Empty;
                ws.Cell(row, 4).Value = assessment.Score ?? 0;
                ws.Cell(row, 5).Value = assessment.Comment ?? string.Empty;
      
                row++;
                index++;
            }
        }

        // Thêm dòng thống kê ở cuối
        row++; // Dòng trống
        ws.Cell(row, 1).Value = "Thống kê:";
        ws.Cell(row, 2).Value = $"Hoàn tất: {approvedSubmissions.Count}/{totalSubmissions} bài";
        ws.Cell(row, 3).Value = $"Chưa Chấm: {totalSubmissions - approvedSubmissions.Count} bài";
        
        // Style dòng thống kê
        var statsRange = ws.Range(row, 1, row, 5);
        statsRange.Style.Font.Bold = true;
        statsRange.Style.Font.FontColor = XLColor.DarkBlue;
        statsRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Format đơn giản
        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var fileBytes = stream.ToArray();

        var fileName = $"TestResult_{examCode}_{subjectCode}_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";

        _logger.LogInformation(
            "Exported exam results for Exam {ExamCode}, Subject {SubjectCode}: {Exported}/{Total} submissions",
            examCode, subjectCode, approvedSubmissions.Count, totalSubmissions);

        var response = new FileServiceResponse
        {
            FileContent = fileBytes,
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileName = fileName
        };

        return new DataServiceResponse<FileServiceResponse>()
        {
            Success = true,
            Message = $"Xuất file thành công. Đã xuất {approvedSubmissions.Count}/{totalSubmissions} bài.",
            Data = response
        };
    }
}
