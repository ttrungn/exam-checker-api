using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Services.Models.Validations;
public class ValidationRules
{
    public KeywordRule? KeywordCheck { get; set; }
    public NameFormatRule? NameFormatMismatch { get; set; }
    public bool CompilationError { get; set; }
}
