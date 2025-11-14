using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Services.Models.Validations;
public class KeywordRule
{
    public List<string>? Keywords { get; set; }
    public List<string>? FileExtensions { get; set; }
}

