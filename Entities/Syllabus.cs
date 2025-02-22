public class Syllabus
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public float Hours { get; set; }
    public string CoreResources { get; set; } = string.Empty;
    public string AdditionalResources { get; set; } = string.Empty;

    public Course Course { get; set; } = null!;
}
