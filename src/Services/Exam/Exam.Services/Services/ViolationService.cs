using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exam.Domain.Entities;
using Exam.Domain.Enums;
using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Interfaces.Services;
using Exam.Services.Models.Validations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Services;
public class ViolationService : IViolationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ViolationService> _logger;

    public ViolationService(IUnitOfWork unitOfWork, ILogger<ViolationService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task<List<Violation>> ValidateSubmissionAsync(Guid submissionId, ZipArchive studentZip, ValidationRules ruleSet, CancellationToken ct)
    {
        _logger.LogInformation("Starting validation for submission {SubmissionId}", submissionId);

        var violations = new List<Violation>();

 

        var innerZip = TryGetInnerZip(studentZip);
        if (innerZip == null)
        {
            _logger.LogWarning("No solution.zip found inside submission {SubmissionId}", submissionId);
            violations.Add(new Violation
            {
                SubmissionId = submissionId,
                ViolationType = ViolationPolicy.WrongProjectStructure,
                Description = "Nested solution.zip not found — invalid submission structure."
            });
            return Task.FromResult(violations);
        }

        _logger.LogInformation("Validating solution.zip contents for submission {SubmissionId}", submissionId);

        if (ruleSet.KeywordCheck != null)
        {
            _logger.LogInformation("Running keyword check for submission {SubmissionId}", submissionId);
            var keywordViolations = ValidateKeywordViolations(submissionId, innerZip, ruleSet.KeywordCheck);
            if (keywordViolations is not null)
            {
                _logger.LogWarning("Keyword violation found for submission {SubmissionId}: {Description}", submissionId, keywordViolations.Description);
                violations.Add(keywordViolations);
            }
        }

        if (ruleSet.NameFormatMismatch != null)
        {
            _logger.LogInformation("Running name format check for submission {SubmissionId}", submissionId);
            var nameFormatViolations = ValidateNameFormat(submissionId, innerZip, ruleSet.NameFormatMismatch);
            if (nameFormatViolations is not null)
            {
                _logger.LogWarning("Name format violation found for submission {SubmissionId}: {Description}", submissionId, nameFormatViolations.Description);
                violations.Add(nameFormatViolations);
            }
        }

        _logger.LogInformation("Validation completed for submission {SubmissionId}. Total violations: {Count}", submissionId, violations.Count);

        return Task.FromResult(violations);
    }

    #region Helpers
    private Violation? ValidateKeywordViolations(
        Guid submissionId,
        ZipArchive zip,
        KeywordRule rule)
    {
        _logger.LogDebug("Validating keywords for submission {SubmissionId}", submissionId);

        var keywordHits = new List<string>();

        foreach (var entry in zip.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name))
            {
                continue;
            }

            var extension = Path.GetExtension(entry.FullName).ToLower();
            if (rule.FileExtensions == null)
            {
                continue;
            }

            var allowedExts = rule.FileExtensions
                .Select(x => x.ToLower())
                .ToHashSet();

            if (!allowedExts.Contains(extension))
            {
                continue;
            }

            string content;
            using (var stream = entry.Open())
            using (var reader = new StreamReader(stream))
            {
                content = reader.ReadToEnd();
            }

            foreach (var keyword in rule.Keywords ?? Enumerable.Empty<string>())
            {
                int position = content.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
                if (position >= 0)
                {
                    _logger.LogDebug("Keyword '{Keyword}' found in file {FileName} for submission {SubmissionId}", keyword, entry.FullName, submissionId);
                    keywordHits.Add($"'{keyword}' in {entry.FullName}");
                }
            }
        }
        if (keywordHits.Any())
        {
            _logger.LogWarning("Keyword violations detected for submission {SubmissionId}: {Hits}", submissionId, string.Join(", ", keywordHits));
            return new Violation
            {
                SubmissionId = submissionId,
                ViolationType = Domain.Enums.ViolationPolicy.KeyMismatch,
                Description = $"Keywords found: {string.Join(", ", keywordHits)}"
            };
        }
        _logger.LogDebug("No keyword violations found for submission {SubmissionId}", submissionId);
        return null;
    }

    private Violation? ValidateNameFormat(
        Guid submissionId,
        ZipArchive zip,
        NameFormatRule rule)
    {
        _logger.LogDebug("Validating name format for submission {SubmissionId}", submissionId);

        var slnFiles = zip.Entries
             .Where(e => e.FullName.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
             .ToList();

        if (!slnFiles.Any())
        {
            _logger.LogWarning("No .sln file found in the root directory for submission {SubmissionId}", submissionId);
            return new Violation
            {
                SubmissionId = submissionId,
                ViolationType = Domain.Enums.ViolationPolicy.WrongProjectStructure,
                Description = "No .sln file found in the root directory."
            };
        }

        if (slnFiles.Count != 1)
        {
            _logger.LogWarning("Multiple .sln files found in the root directory for submission {SubmissionId}", submissionId);
            return new Violation
            {
                SubmissionId = submissionId,
                ViolationType = Domain.Enums.ViolationPolicy.WrongProjectStructure,
                Description = "There should be exactly one .sln file in the root directory."
            };
        }

        var slnFile = slnFiles.First();

        var fileName = Path.GetFileNameWithoutExtension(slnFile.FullName);
        var expected = rule.NameFormat;

        // Split rule into prefix & suffix around {StudentName}
        var parts = expected.Split("{StudentName}", StringSplitOptions.None);
        var prefix = parts[0];
        var suffix = parts.Length > 1 ? parts[1] : "";

        // Validate file name structure
        if (!fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
            !fileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Name format mismatch for submission {SubmissionId}. Expected: {Expected}, Actual: {Actual}", submissionId, expected, fileName + ".sln");
            return new Violation
            {
                SubmissionId = submissionId,
                ViolationType = Domain.Enums.ViolationPolicy.IncorrectNamingConvention,
                Description =
                    $"Name format mismatch.\nExpected template: '{expected}'\nActual: '{fileName}.sln'"
            };
        }

        var studentName = fileName.Substring(prefix.Length, fileName.Length - prefix.Length - suffix.Length);
        if (string.IsNullOrWhiteSpace(studentName))
        {
            _logger.LogWarning("Student name missing in solution name for submission {SubmissionId}", submissionId);
            return new Violation
            {
                SubmissionId = submissionId,
                ViolationType = Domain.Enums.ViolationPolicy.IncorrectNamingConvention,
                Description = "Student name missing in solution name."
            };  
        }

        _logger.LogDebug("Name format validation passed for submission {SubmissionId}", submissionId);
        return null;
    }

    //private  Task<List<Violation>> ValidateCompilationAsync(
    //    Guid submissionId,
    //    ZipArchive zip,
    //    CancellationToken ct)
    //{
    //    throw new NotImplementedException();
    //}

    private ZipArchive? TryGetInnerZip(ZipArchive outerZip)
    {
        var innerZipEntry = outerZip.Entries
            .FirstOrDefault(e =>
                e.FullName.EndsWith("solution.zip", StringComparison.OrdinalIgnoreCase));

        if (innerZipEntry == null)
            return null;

        var ms = new MemoryStream();
        using (var s = innerZipEntry.Open())
            s.CopyTo(ms);

        ms.Position = 0;
        return new ZipArchive(ms, ZipArchiveMode.Read);
    }


    #endregion
}
