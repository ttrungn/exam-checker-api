using System.ComponentModel.DataAnnotations;

namespace Exam.Services.Models.Requests.Semesters;

public class SemesterRequest
{
    [Required(ErrorMessage =  "Vui lòng nhập tên!")]
    public string Name { get; set; } = null!;
}
