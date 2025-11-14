using System.ComponentModel.DataAnnotations;

namespace Exam.Services.Models.Requests.Exams;

public class ExamRequest
{
    [Required(ErrorMessage = "Vui lòng nhập mã học kỳ!")]
    public Guid SemesterId { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập mã kỳ thi!")]
    public string Code { get; set; } = null!;
    [Required(ErrorMessage = "Vui lòng nhập ngày bắt đầu kỳ thi!")]
    public DateTimeOffset StartDate { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập ngày kết thúc kỳ thi!")]
    public DateTimeOffset EndDate { get; set; }
}
