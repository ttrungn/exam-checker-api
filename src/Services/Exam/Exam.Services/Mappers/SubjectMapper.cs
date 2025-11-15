using Exam.Domain.Entities;
using Exam.Services.Features.Subjects.Commands.CreateSubject;
using Exam.Services.Features.Subjects.Commands.UpdateSubject;
using Exam.Services.Features.Subjects.Queries.GetSubjects;
using Exam.Services.Models.Requests.Subjects;
using Exam.Services.Models.Responses.Subjects;

namespace Exam.Services.Mappers;

public static class SubjectMapper
{
    public static Subject ToSubject(this CreateSubjectCommand request)
    {
        return new Subject()
        {
            SemesterId = request.SemesterId, Name = request.Name, Code = request.Code
        };
    }

    public static void UpdateSubject(this Subject subject, UpdateSubjectCommand request)
    {
        subject.SemesterId = request.SemesterId;
        subject.Name = request.Name;
        subject.Code = request.Code;
    }

    public static SubjectResponse ToSubjectResponse(this Subject subject)
    {
        return new SubjectResponse()
        {
            Id = subject.Id,
            Name = subject.Name,
            Code = subject.Code,
            CreatedAt = subject.CreatedAt,
            UpdatedAt = subject.UpdatedAt,
            IsActive = subject.IsActive,
            Semester = subject.Semester?.ToSemesterResponse()
        };
    }

    public static GetSubjectsQuery ToGetSubjectsQuery(this SubjectGetRequest request)
    {
        return new GetSubjectsQuery()
        {
            Name = request.Name,
            Code = request.Code,
            IsActive = request.IsActive,
            PageIndex = request.PageIndex ?? 1,
            PageSize = request.PageSize ?? 8
        };
    }
}
