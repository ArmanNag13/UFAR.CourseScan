public class Reference
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Type { get; set; } = string.Empty; // "Core" or "Additional"
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int? Year { get; set; }
    public string? ISBN { get; set; }
    public string? URL { get; set; }

    public Course Course { get; set; } = null!;
}
