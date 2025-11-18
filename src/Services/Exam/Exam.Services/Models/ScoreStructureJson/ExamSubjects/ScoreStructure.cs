namespace Exam.Services.Models.ScoreStructureJson.ExamSubjects;


public class ScoreStructure
{
    public decimal MaxScore { get; set; }
    public List<ScoreSection> Sections { get; set; } = new();
}
public class ScoreSection
{
    public string Key { get; set; } = null!;    // "login", "list", "create"...
    public string Name { get; set; } = null!;   // "Login", "List All", ...
    public int Order { get; set; }
    public List<ScoreCriterion> Criteria { get; set; } = new();
}

public class ScoreCriterion
{
    public string Key { get; set; } = null!;    // "login_ok", "list_1", ...
    public string Name { get; set; } = null!;
    public decimal MaxScore { get; set; }
    public int Order { get; set; }
}
