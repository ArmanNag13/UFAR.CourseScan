public class Course
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty; // "English" or "French"
    public string Professor { get; set; } = string.Empty;
    public int ECTS { get; set; }
    public int HoursCM { get; set; } // Lecture hours
    public int HoursTD { get; set; } // Practical hours
    public int HoursTP { get; set; } // Lab hours

    public float CreditHours { get; set; }
    public List<LearningOutcome> LearningOutcomes { get; set; } = new();
    public List<Assessment> Assessments { get; set; } = new();
    public List<TeachingMethod> TeachingMethods { get; set; } = new();
    public List<Prerequisite> Prerequisites { get; set; } = new();
    public List<Syllabus> Syllabus { get; set; } = new();
    public List<Reference> References { get; set; } = new();
}
