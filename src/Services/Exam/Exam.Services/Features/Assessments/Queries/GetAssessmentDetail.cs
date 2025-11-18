using System.Text.Json;
using Exam.Domain.Entities;
using Exam.Domain.Enums;
using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Exceptions;
using Exam.Services.Models.Responses;
using Exam.Services.Models.Responses.Submissions;
using Exam.Services.Models.ScoreStructureJson.Assessments;
using Exam.Services.Models.ScoreStructureJson.ExamSubjects;
using Exam.Services.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.Assessments.Queries;

public record GetAssessmentDetailQuery : IRequest<DataServiceResponse<AssessmentDetailResponse>>
{
    public Guid Id { get; init; }
}



public class GetAssessmentDetailQueryHandler 
    : IRequestHandler<GetAssessmentDetailQuery, DataServiceResponse<AssessmentDetailResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAssessmentDetailQueryHandler> _logger;

    public GetAssessmentDetailQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAssessmentDetailQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger     = logger;
    }
    
    public async Task<DataServiceResponse<AssessmentDetailResponse>> Handle(
        GetAssessmentDetailQuery request,
        CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.GetRepository<Assessment>();

        var assessment = await repo.Query()
            .Include(a => a.Submission!)
                .ThenInclude(s => s.ExamSubject)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (assessment == null)
        {
            throw new NotFoundException("Không tìm thấy Assessment.");
        }

        var examSubject = assessment.Submission!.ExamSubject;
        if (examSubject == null || string.IsNullOrWhiteSpace(examSubject.ScoreStructure))
        {
            throw new ServiceUnavailableException("Môn thi chưa cấu hình ScoreStructure.");
        }

        // Rubric
        var structure = JsonSerializer.Deserialize<ScoreStructure>(
            examSubject.ScoreStructure,
            JsonDefaults.CamelCase);
        if (structure == null)
        {
            throw new ServiceUnavailableException("ScoreStructure không hợp lệ.");
        }

        // ScoreDetail (nếu đã chấm trước đó)
        ScoreDetail? scoreDetail = null;
        if (!string.IsNullOrWhiteSpace(assessment.ScoreDetails))
        {
            scoreDetail = JsonSerializer.Deserialize<ScoreDetail>(
                assessment.ScoreDetails,
                JsonDefaults.CamelCase);
        }
        if (scoreDetail == null)
        {
            scoreDetail = new ScoreDetail
            {
                TotalScore = 0,
                Sections = structure.Sections.Select(s => new ScoreSectionResult
                {
                    Key   = s.Key,
                    Name  = s.Name,
                    Score = 0,
                    Criteria = s.Criteria.Select(c => new ScoreCriterionResult
                    {
                        Key      = c.Key,
                        Name     = c.Name,
                        MaxScore = c.MaxScore,
                        Score    = 0
                    }).ToList()
                }).ToList()
            };
        }
        // LẦN ĐẦU VÀO: Pending -> InReview
        if (assessment.Status == AssessmentStatus.Pending)
        {
            assessment.Status = AssessmentStatus.InReview;
            await _unitOfWork.SaveChangesAsync();
        }

        var response = new AssessmentDetailResponse
        {
            Id            = assessment.Id,
            SubmissionId  = assessment.SubmissionId,
            SubmissionName = assessment.SubmissionName!,
            Status        = assessment.Status,
            Score         = assessment.Score,
            Comment       = assessment.Comment,
            ScoreStructure = structure,
            ScoreDetail    = scoreDetail
        };

        return new DataServiceResponse<AssessmentDetailResponse>
        {
            Success = true,
            Message = "Lấy thông tin chấm điểm thành công.",
            Data    = response
        };
    }
}
