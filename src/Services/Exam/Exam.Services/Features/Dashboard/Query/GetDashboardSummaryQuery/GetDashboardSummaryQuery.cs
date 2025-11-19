using DocumentFormat.OpenXml.EMMA;
using Exam.Domain.Entities;
using Exam.Domain.Enums;
using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Models.Responses.Dashboard;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exam.Services.Features.Dashboard.Query.GetDashboardSummaryQuery;

public class GetDashboardSummaryQuery : IRequest<IQueryable<DashboardSummaryResponse>>
{
    // public Guid? ExamSubjectId { get; set; }
    //
    // public Guid? ExamId { get; set; }
    //
    // public Guid? SubjectId { get; set; }
    
}

public class GetDashboardSummaryHandler 
    : IRequestHandler<GetDashboardSummaryQuery, IQueryable<DashboardSummaryResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDashboardSummaryHandler(IUnitOfWork context)
    {
        _unitOfWork = context;
    }

    public Task<IQueryable<DashboardSummaryResponse>> Handle(
        GetDashboardSummaryQuery request, 
        CancellationToken cancellationToken)
    {
        var repo =  _unitOfWork.GetRepository<ExamSubject>();

        var query = repo.Query()
            .Include(x => x.Exam)
            .Include(x => x.Subject)
            .Include(x => x.Submissions).AsNoTracking();
        
        // if (request.ExamSubjectId.HasValue)
        //     query = query.Where(x => x.Id == request.ExamSubjectId.Value);
        //
        // if (request.ExamId.HasValue)
        //     query = query.Where(x => x.ExamId == request.ExamId.Value);
        //
        // if (request.SubjectId.HasValue)
        //     query = query.Where(x => x.SubjectId == request.SubjectId.Value);
        
        var result  = query
            .GroupBy(x => new 
            {
                x.Id,
                x.ExamId,
                x.SubjectId,
                ExamCode = x.Exam.Code,
                SubjectCode = x.Subject.Code,
            })
            .Select(x => new DashboardSummaryResponse
            {
                ExamSubjectId = x.Key.Id,
                ExamId = x.Key.ExamId,
                SubjectId = x.Key.SubjectId,
                ExamCode = x.Key.ExamCode,
                SubjectCode = x.Key.SubjectCode,
                TotalSubmissions = x.SelectMany(s => s.Submissions).Count(),
                Graded = x.SelectMany(s => s.Submissions).Count(s => s.GradeStatus == GradeStatus.Graded),
                Reassigned = x.SelectMany(s => s.Submissions).Count(s => s.GradeStatus == GradeStatus.ReAssigned),
                Approved = x.SelectMany(s => s.Submissions).Count(s => s.GradeStatus == GradeStatus.Approved),
                NotGraded = x.SelectMany(s => s.Submissions).Count(s => s.GradeStatus == GradeStatus.NotGraded),
                Violated = x.SelectMany(s => s.Submissions).Count(s =>
                    s.Status == SubmissionStatus.Violated || s.Status == SubmissionStatus.ModeratorViolated),
                ProgressPercent =
                    x.SelectMany(s => s.Submissions)
                        .Count(s => s.GradeStatus == GradeStatus.Graded) * 100m /
                    Math.Max(x.SelectMany(s => s.Submissions).Count(), 1)
            });
            
            

        return Task.FromResult(result);
    }
}
