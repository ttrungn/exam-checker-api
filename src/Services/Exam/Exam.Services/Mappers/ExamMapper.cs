using Exam.Services.Features.Exams.Commands.CreateExam;
using Exam.Services.Features.Exams.Commands.UpdateExam;
using Exam.Services.Features.Exams.Queries.GetExams;
using Exam.Services.Models.Requests.Exams;
using Exam.Services.Models.Responses.Exams;

namespace Exam.Services.Mappers;

public static class ExamMapper
{
    public static Domain.Entities.Exam ToExam(this CreateExamCommand request)
    {
        return new Domain.Entities.Exam()
        {
            SemesterId = request.SemesterId,
            Code = request.Code,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };
    }

    public static void UpdateExam(this Domain.Entities.Exam exam, UpdateExamCommand request)
    {
        exam.SemesterId = request.SemesterId;
        exam.Code = request.Code;
        exam.StartDate = request.StartDate;
        exam.EndDate = request.EndDate;
    }

    public static ExamResponse ToExamResponse(this Domain.Entities.Exam exam)
    {
        return new ExamResponse()
        {
            Id = exam.Id,
            Code = exam.Code,
            StartDate = exam.StartDate,
            EndDate = exam.EndDate,
            CreatedAt = exam.CreatedAt,
            UpdatedAt = exam.UpdatedAt,
            IsActive = exam.IsActive,
            Semester = exam.Semester?.ToSemesterResponse()
        };
    }

    public static GetExamsQuery ToGetExamsQuery(this ExamGetRequest request)
    {
        return new GetExamsQuery()
        {
            Code = request.Code,
            IsActive = request.IsActive,
            PageIndex = request.PageIndex ?? 1,
            PageSize = request.PageSize ?? 8
        };
    }
}
