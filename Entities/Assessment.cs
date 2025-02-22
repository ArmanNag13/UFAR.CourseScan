public class Assessment
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Type { get; set; } = string.Empty; // "Ongoing", "Midterm", "Final Exam"
    public string AssessmentMethod { get; set; } = string.Empty; // "Oral", "Written"
    public float? Duration { get; set; } // In hours, nullable
    public bool GroupBased { get; set; }
    public bool ProjectRequired { get; set; }
    public bool PresentationRequired { get; set; }
    public string Method { get; set; }

    public Course Course { get; set; } = null!;
}
