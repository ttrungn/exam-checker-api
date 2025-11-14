using Exam.Domain.Entities;
using Exam.Services.Features.Semesters.Commands.CreateSemester;
using Exam.Services.Features.Semesters.Commands.UpdateSemester;
using Exam.Services.Features.Semesters.Queries.GetSemesters;
using Exam.Services.Models.Requests.Semesters;
using Exam.Services.Models.Responses.Semesters;

namespace Exam.Services.Mappers;

public static class SemesterMapper
{
    public static Semester ToSemester(this CreateSemesterCommand request)
    {
        return new Semester
        {
            Name = request.Name
        };
    }

    public static void UpdateSemester(this Semester semester, UpdateSemesterCommand request)
    {
        semester.Name = request.Name;
    }

    public static SemesterResponse ToSemesterResponse(this Semester semester)
    {
        return new SemesterResponse
        {
            Id = semester.Id,
            Name = semester.Name,
            CreatedAt = semester.CreatedAt,
            UpdatedAt = semester.UpdatedAt,
            IsActive = semester.IsActive
        };
    }

    public static GetSemestersQuery ToGetSemestersQuery(this SemesterGetRequest request)
    {
        return new GetSemestersQuery
        {
            Name = request.Name,
            IsActive = request.IsActive,
            PageIndex = request.PageIndex ?? 1,
            PageSize = request.PageSize ?? 8
        };
    }
}
