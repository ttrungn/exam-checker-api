namespace Exam.Domain.Enums;

public enum ViolationPolicy
{
    InvalidFileFormat = 1,      // File không đúng định dạng yêu cầu
    MissingRequiredFiles = 2,   // Thiếu file bắt buộc
    IncorrectNamingConvention = 3,  // Sai format tên file
    KeyMismatch = 4,  // Trùng key 
}
