namespace Domain.Enums;

public enum ViolationPolicy
{
    WrongProjectStructure = 0,   // Cấu trúc file sai
    InvalidFileFormat = 1,      // File không đúng định dạng yêu cầu
    MissingRequiredFiles = 2,   // Thiếu file bắt buộc
    IncorrectNamingConvention = 3,  // Sai format tên file
    KeyMismatch = 4,  
    CompilationError = 5,        // Lỗi biên dịch   
}
