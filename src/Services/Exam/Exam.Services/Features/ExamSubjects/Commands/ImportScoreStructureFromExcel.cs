using System.Text.Json;
using ClosedXML.Excel;
using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Exceptions;
using Exam.Services.Models.Responses;
using Exam.Services.Models.ScoreStructureJson.ExamSubjects;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.ExamSubjects.Commands;


public record ImportScoreStructureFromExcelCommand : IRequest<BaseServiceResponse>
{
    public Guid ExamSubjectId { get; init; }
    public IFormFile File { get; init; } = null!;
}

public class ImportScoreStructureFromExcelCommandValidator
    : AbstractValidator<ImportScoreStructureFromExcelCommand>
{
    public ImportScoreStructureFromExcelCommandValidator()
    {
        RuleFor(x => x.ExamSubjectId)
            .NotEmpty().WithMessage("Vui lòng chọn môn thi (ExamSubject)!");

        RuleFor(x => x.File)
            .NotNull().WithMessage("Vui lòng chọn file Excel!")
            .Must(f => f.Length > 0).WithMessage("File Excel không được rỗng!")
            .Must(f => f.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase)
                       || f.FileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Vui lòng chọn file Excel (.xlsx hoặc .xls)!");
    }
}


public class ImportScoreStructureFromExcelCommandHandler
    : IRequestHandler<ImportScoreStructureFromExcelCommand, BaseServiceResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ImportScoreStructureFromExcelCommandHandler> _logger;

    public ImportScoreStructureFromExcelCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<ImportScoreStructureFromExcelCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> Handle(
        ImportScoreStructureFromExcelCommand request,
        CancellationToken cancellationToken)
    {
        var examSubjectRepo = _unitOfWork.GetRepository<Domain.Entities.ExamSubject>();

        var examSubject = await examSubjectRepo.Query()
            .FirstOrDefaultAsync(x => x.Id == request.ExamSubjectId, cancellationToken);

        if (examSubject == null)
        {
            _logger.LogError("ExamSubject not found with ID: {Id}", request.ExamSubjectId);
            throw new NotFoundException("Không tìm thấy môn thi (ExamSubject)!");
        }

        ScoreStructure scoreStructureDto;
        using (var stream = request.File.OpenReadStream())
        using (var workbook = new XLWorkbook(stream))
        {
            var ws = workbook.Worksheets.First(); // sheet đầu tiên

            var row1 = ws.Row(1); // group: Login, List, Create ...
            var row2 = ws.Row(2); // criteria names
            var row3 = ws.Row(3); // max scores

            // thì rubric bắt đầu từ cột 7
            const int startColumn = 7;
            var lastColumn = ws.LastColumnUsed().ColumnNumber();

            var sectionDict = new Dictionary<string, ScoreSection>();
            decimal totalMaxScore = 0;
            int sectionOrderSeed = 1;

            for (int col = startColumn; col <= lastColumn; col++)
            {
                var sectionNameRaw = row1.Cell(col).GetString();
                sectionNameRaw = Clean(sectionNameRaw);

                // Nếu là merge cell -> lấy tên section trước đó
                if (string.IsNullOrWhiteSpace(sectionNameRaw))
                {
                    sectionNameRaw = sectionDict.LastOrDefault().Value?.Name ?? string.Empty;
                }

                // ----- CRITERION NAME -----
                var criterionNameRaw = row2.Cell(col).GetString();
                criterionNameRaw = Clean(criterionNameRaw);
                // Nếu cả section + criterion đều rỗng => bỏ qua
                if (string.IsNullOrWhiteSpace(sectionNameRaw)
                    && string.IsNullOrWhiteSpace(criterionNameRaw))
                {
                    continue;
                }

                // VD: "Login (1.0)" -> "Login"
                var sectionName = ExtractSectionName(sectionNameRaw);
                sectionName = Clean(sectionName);

                // Bỏ cột Total & “Trường hợp 0 điểm…”
                if (sectionName.Equals("total", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (sectionName.Contains("0 điểm", StringComparison.OrdinalIgnoreCase))
                    continue;

                var sectionKey = ToKey(sectionName);
                if (!sectionDict.TryGetValue(sectionKey, out var sectionDto))
                {
                    sectionDto = new ScoreSection { Key = sectionKey, Name = sectionName, Order = sectionOrderSeed++ };
                    sectionDict[sectionKey] = sectionDto;
                }

                if (string.IsNullOrWhiteSpace(criterionNameRaw))
                {
                    continue;
                }

                var criterionName = Clean(criterionNameRaw);
                var rawName = criterionName;
                var criterionKey = ToKey($"{sectionKey}_{rawName}");

                // Row 3 là max score
                decimal maxScore = 0;
                var maxScoreCell = row3.Cell(col);
                if (!maxScoreCell.IsEmpty())
                {
                    maxScore = (decimal)maxScoreCell.GetDouble();
                }

                // Nếu tiêu chí không có điểm tối đa thì bỏ qua
                if (maxScore <= 0)
                    continue;

                var criterionOrder = sectionDto.Criteria.Count + 1;

                // Nếu key bị trùng trong cùng section thì thêm suffix index
                if (sectionDto.Criteria.Any(x => x.Key == criterionKey))
                {
                    criterionKey = $"{criterionKey}_{criterionOrder}";
                }

                sectionDto.Criteria.Add(new ScoreCriterion
                {
                    Key = criterionKey, Name = criterionName, MaxScore = maxScore, Order = criterionOrder
                });

                totalMaxScore += maxScore;
            }


            scoreStructureDto = new ScoreStructure
            {
                MaxScore = totalMaxScore,
                Sections = sectionDict.Values
                    .OrderBy(s => s.Order)
                    .ToList()
            };
        }

        var json = JsonSerializer.Serialize(
            scoreStructureDto,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = false });

        examSubject.ScoreStructure = json;

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Imported ScoreStructure for ExamSubject {Id} successfully",
            examSubject.Id);

        return new DataServiceResponse<ScoreStructure>
        {
            Success = true, Message = "Import rubric (ScoreStructure) thành công!", Data = scoreStructureDto
        };
    }

    private static string ExtractSectionName(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return raw;

        // "Login (1.0)" -> "Login"
        var index = raw.IndexOf('(');
        if (index > 0)
        {
            return raw[..index].Trim();
        }

        return raw.Trim();
    }

    private static string ToKey(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        // lower-case + bỏ ()[] + khoảng trắng -> `_`
        var key = input.Trim().ToLowerInvariant();

        key = key.Replace("(", "")
            .Replace(")", "")
            .Replace("[", "")
            .Replace("]", "");

        key = string.Join("_", key
            .Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // phòng trường hợp có dấu , ; :
        key = key.Replace(",", "_")
            .Replace(";", "_")
            .Replace(":", "_");

        return key;
    }

    private static string Clean(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        return input
            .Replace("\n", " ")
            .Replace("\r", " ")
            .Replace("\t", " ")
            .Trim();
    }
}
