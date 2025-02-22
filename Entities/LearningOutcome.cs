public class LearningOutcome
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Category { get; set; } = string.Empty; // "Knowledge" / "Skills to Apply" / "General Skills"
    public string Code { get; set; } = string.Empty; // e.g., "A1", "B1.1"
    public string Description { get; set; } = string.Empty;

    public Course Course { get; set; } = null!;
}
