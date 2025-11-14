using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exam.Domain.Entities;
using Exam.Services.Models.Validations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace Exam.Services.Interfaces.Services;
public interface IViolationService
{
    Task<List<Violation>> ValidateSubmissionAsync(
    Guid submissionId,
    ZipArchive studentZip,
    ValidationRules ruleSet,
    CancellationToken ct);
}
