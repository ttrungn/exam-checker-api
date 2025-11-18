namespace Exam.Services.Models.ScoreStructureJson.Assessments;

public class ScoreDetail
{
    public decimal TotalScore { get; set; }
    public List<ScoreSectionResult> Sections { get; set; } = new();
}
public class ScoreSectionResult
{
    public string Key { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal Score { get; set; }
    public List<ScoreCriterionResult> Criteria { get; set; } = new();
}

public class ScoreCriterionResult
{
    public string Key { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal MaxScore { get; set; }
    public decimal Score { get; set; }
}
