public class TeachingMethod
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Method { get; set; } = string.Empty; // "Lecture", "Practical Work", etc.

    public Course Course { get; set; } = null!;
}
