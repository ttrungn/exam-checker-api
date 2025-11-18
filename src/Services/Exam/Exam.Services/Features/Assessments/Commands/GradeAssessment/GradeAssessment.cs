using System.Text.Json;
using Exam.Domain.Entities;
using Exam.Domain.Enums;
using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Exceptions;
using Exam.Services.Models.Responses;
using Exam.Services.Models.ScoreStructureJson.Assessments;
using Exam.Services.Models.ScoreStructureJson.ExamSubjects;
using Exam.Services.Utils;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = FluentValidation.ValidationException;

namespace Exam.Services.Features.Assessments.Commands.GradeAssessment;

public record GradeAssessmentCommand : IRequest<BaseServiceResponse>
{
    public Guid AssessmentId { get; init; }
    public ScoreDetail ScoreDetail { get; init; } = null!;
    public string? Comment { get; init; }
}

public class GradeAssessmentCommandValidator : AbstractValidator<GradeAssessmentCommand>
{
    public GradeAssessmentCommandValidator()
    {
        RuleFor(x => x.AssessmentId)
            .NotEmpty().WithMessage("Vui lòng nhập mã bài đánh giá!");

        RuleFor(x => x.ScoreDetail)
            .NotNull().WithMessage("Vui lòng nhập chi tiết điểm số!");
    }
}

public class GradeAssessmentCommandHandler 
    : IRequestHandler<GradeAssessmentCommand, BaseServiceResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GradeAssessmentCommandHandler> _logger;

    public GradeAssessmentCommandHandler(IUnitOfWork unitOfWork, ILogger<GradeAssessmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger     = logger;
    }

    public async Task<BaseServiceResponse> Handle(
        GradeAssessmentCommand request,
        CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.GetRepository<Assessment>();

        var assessment = await repo.Query()
            .Include(a => a.Submission!)
                .ThenInclude(s => s.ExamSubject)
            .FirstOrDefaultAsync(a => a.Id == request.AssessmentId, cancellationToken);

        if (assessment == null)
        {
            throw new NotFoundException("Không tìm thấy Assessment.");
        }

        var examSubject = assessment.Submission!.ExamSubject;
        if (examSubject == null || string.IsNullOrWhiteSpace(examSubject.ScoreStructure))
        {
            throw new ServiceUnavailableException("Môn thi chưa cấu hình ScoreStructure.");
        }

        // Deserialize rubric
        var structure = JsonSerializer.Deserialize<ScoreStructure>(
            examSubject.ScoreStructure,
            JsonDefaults.CamelCase);

        if (structure == null)
            throw new ServiceUnavailableException("ScoreStructure không hợp lệ.");


        // Validate & tính lại tổng điểm
        var recalculatedTotal = ValidateAndCalculateTotal(structure, request.ScoreDetail);

        // Gán lại total 
        request.ScoreDetail.TotalScore = recalculatedTotal;

        assessment.Score        = recalculatedTotal;
        assessment.ScoreDetails = JsonSerializer.Serialize(
            request.ScoreDetail,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        assessment.Comment      = request.Comment;
        assessment.GradedAt     = DateTimeOffset.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Assessment {Id} graded with score {Score}", 
            assessment.Id, assessment.Score);

        return new BaseServiceResponse
        {
            Success = true,
            Message = "Lưu kết quả chấm điểm thành công."
        };
    }

    private static decimal ValidateAndCalculateTotal(ScoreStructure rubric, ScoreDetail detail)
    {
        // Build dictionary để match key
        var sectionDict = rubric.Sections.ToDictionary(x => x.Key, x => x, StringComparer.OrdinalIgnoreCase);

        decimal total = 0;

        foreach (var sectionResult in detail.Sections)
        {
            if (!sectionDict.TryGetValue(sectionResult.Key, out var sectionDef))
            {
                throw new ValidationException($"Section '{sectionResult.Key}' không tồn tại trong ScoreStructure.");
            }

            var criterionDict = sectionDef.Criteria
                .ToDictionary(x => x.Key, x => x, StringComparer.OrdinalIgnoreCase);

            decimal sectionScore = 0;

            foreach (var criterionResult in sectionResult.Criteria)
            {
                if (!criterionDict.TryGetValue(criterionResult.Key, out var criterionDef))
                {
                    throw new ValidationException(
                        $"Criterion '{criterionResult.Key}' không tồn tại trong section '{sectionResult.Key}'.");
                }

                // Validate MaxScore trong result khớp với rubric
                if (criterionResult.MaxScore != criterionDef.MaxScore)
                {
                    throw new ValidationException(
                        $"MaxScore của '{criterionResult.Key}' không khớp với rubric. Expected {criterionDef.MaxScore}, got {criterionResult.MaxScore}");
                }

                if (criterionResult.Score < 0 || criterionResult.Score > criterionDef.MaxScore)
                {
                    throw new ValidationException(
                        $"Điểm của '{criterionResult.Key}' phải trong khoảng [0, {criterionDef.MaxScore}].");
                }

                sectionScore += criterionResult.Score;
            }

            sectionResult.Score = sectionScore;
            total += sectionScore;
        }

        if (total > rubric.MaxScore)
        {
            throw new ValidationException(
                $"Tổng điểm {total} vượt quá MaxScore của bài là {rubric.MaxScore}.");
        }

        return total;
    }
}
