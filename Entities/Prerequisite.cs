public class Prerequisite
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Requirement { get; set; } = string.Empty; // e.g., "Sets and their operations"
    public string Description { get; set; }

    public Course Course { get; set; } = null!;
}
