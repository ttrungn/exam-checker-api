using System.ComponentModel.DataAnnotations;

namespace Exam.Services.Models.Requests.Subjects;

public class SubjectRequest
{
    [Required(ErrorMessage = "Vui lòng nhập mã học kỳ!")]
    public Guid SemesterId { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập tên môn học!")]
    public string Name { get; set; } = null!;
    [Required(ErrorMessage = "Vui lòng nhập mã môn học!")]
    public string Code { get; set; } = null!;
}
