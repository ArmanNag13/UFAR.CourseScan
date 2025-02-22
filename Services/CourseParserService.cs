using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CourseParserService : ICourseParserService
{
    private readonly IPdfService _textExtractorService;

    public CourseParserService(IPdfService textExtractorService)
    {
        _textExtractorService = textExtractorService;
    }

    public Course ParseCourse(string courseText)
    {
        var course = new Course();

        // Extract the course title from the first non-empty line (assumed to be title)
        course.Title = ExtractCourseTitle(courseText);

        // Extract relevant course info
        course.AcademicYear = ExtractAcademicYear(courseText);
        course.Degree = ExtractDegree(courseText);
        course.Qualification = ExtractQualification(courseText);
        course.Professor = ExtractProfessor(courseText);
        course.ECTS = ExtractECTS(courseText);
        course.CreditHours = 6; // Default CreditHours; adjust if needed
        course.HoursCM = (int)ExtractHoursCM(courseText);
        course.HoursTD = (int)ExtractHoursTD(courseText);
        course.HoursTP = (int)ExtractHoursTP(courseText);

        // Parse Learning Outcomes
        course.LearningOutcomes = ExtractLearningOutcomes(courseText);

        // Parse Assessments
        course.Assessments = ExtractAssessments(courseText);

        // Parse Teaching Methods
        course.TeachingMethods = ExtractTeachingMethods(courseText);

        // Parse Prerequisites
        course.Prerequisites = ExtractPrerequisites(courseText);

        // Parse Syllabus
        course.Syllabus = ExtractSyllabus(courseText);

        // Parse References
        course.References = ExtractReferences(courseText);

        return course;
    }

    private string ExtractCourseTitle(string text)
    {
        // Assume that the course title is on the first non-empty line 
        // that doesn't start with a known header such as "ACADEMIC YEAR" or "Degree".
        var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            string trimmed = line.Trim();
            if (!string.IsNullOrEmpty(trimmed) &&
                !trimmed.StartsWith("ACADEMIC YEAR", StringComparison.OrdinalIgnoreCase) &&
                !trimmed.StartsWith("Degree", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed;
            }
        }
        return string.Empty;
    }

    private string ExtractAcademicYear(string text)
    {
        var regex = new Regex(@"ACADEMIC YEAR\s+(\d{4}-\d{4})", RegexOptions.IgnoreCase);
        var match = regex.Match(text);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private string ExtractDegree(string text)
    {
        var regex = new Regex(@"Degree\s+(.+)", RegexOptions.IgnoreCase);
        var match = regex.Match(text);
        return match.Success ? match.Groups[1].Value.Trim() : "Unknown Degree";
    }

    private string ExtractQualification(string text)
    {
        var regex = new Regex(@"Qualification\s+(.+)", RegexOptions.IgnoreCase);
        var match = regex.Match(text);
        return match.Success ? match.Groups[1].Value.Trim() : "Unknown Qualification";
    }

    private string ExtractProfessor(string text)
    {
        var regex = new Regex(@"Professor\s+(.+)", RegexOptions.IgnoreCase);
        var match = regex.Match(text);
        return match.Success ? match.Groups[1].Value.Trim() : "Unknown Professor";
    }

    private int ExtractECTS(string text)
    {
        var regex = new Regex(@"ECTS\s+(\d+)", RegexOptions.IgnoreCase);
        var match = regex.Match(text);
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    private double ExtractHoursCM(string text)
    {
        // Handles patterns like "CM 18 h." with optional period after 'h'
        var regex = new Regex(@"CM\s+(\d+)\s*h\.?", RegexOptions.IgnoreCase);
        var match = regex.Match(text);
        return match.Success ? double.Parse(match.Groups[1].Value) : 0;
    }

    private double ExtractHoursTD(string text)
    {
        // Adjusted to look for "TD" (as seen in your sample: "TD 12 h.")
        var regex = new Regex(@"TD\s+(\d+)\s*h\.?", RegexOptions.IgnoreCase);
        var match = regex.Match(text);
        return match.Success ? double.Parse(match.Groups[1].Value) : 0;
    }

    private double ExtractHoursTP(string text)
    {
        // If a TPS field exists, extract it; otherwise return 0.
        var regex = new Regex(@"TPS\s+(\d+)\s*h\.?", RegexOptions.IgnoreCase);
        var match = regex.Match(text);
        return match.Success ? double.Parse(match.Groups[1].Value) : 0;
    }

    private List<LearningOutcome> ExtractLearningOutcomes(string text)
    {
        var outcomes = new List<LearningOutcome>();

        // Example regex to capture outcomes starting with 'A' or 'B'
        // This is basic and may need further refinement based on input variability.
        var regex = new Regex(@"([AB][\s\-]*\d+(\.\d+)?)[\s\-]+(.+?)(?=\n[A-Z]|\n\n)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        var matches = regex.Matches(text);
        foreach (Match match in matches)
        {
            string code = match.Groups[1].Value.Trim();
            string description = match.Groups[3].Value.Trim();
            if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(description))
            {
                outcomes.Add(new LearningOutcome
                {
                    Code = code,
                    Description = description
                });
            }
        }
        return outcomes;
    }

    private List<Assessment> ExtractAssessments(string text)
    {
        var assessments = new List<Assessment>();
        // Example: look for a "Duration" field with a floating-point number followed by "h"
        var regex = new Regex(@"Duration\s*:\s*(\d+\.\d+)\s*h", RegexOptions.IgnoreCase);
        var match = regex.Match(text);
        if (match.Success)
        {
            double hours = double.Parse(match.Groups[1].Value);
            assessments.Add(new Assessment
            {
                Type = "Exam", // Placeholder; adjust according to actual text
                Duration = (float?)TimeSpan.FromHours(hours).TotalHours,
                Method = "Oral", // Placeholder
                AssessmentMethod = "Summative" // Placeholder
            });
        }
        return assessments;
    }

    private List<TeachingMethod> ExtractTeachingMethods(string text)
    {
        // Placeholder: implement extraction logic if needed
        return new List<TeachingMethod>();
    }

    private List<Prerequisite> ExtractPrerequisites(string text)
    {
        // Example: extract the prerequisites section if it exists
        var prerequisites = new List<Prerequisite>();
        var regex = new Regex(@"KNOWLEDGE & SKILLS PREREQUISITS\s*(.*?)\n\n", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        var match = regex.Match(text);
        if (match.Success)
        {
            prerequisites.Add(new Prerequisite
            {
                Description = match.Groups[1].Value.Trim()
            });
        }
        return prerequisites;
    }

    private List<Syllabus> ExtractSyllabus(string text)
    {
        // Placeholder: implement extraction logic for the syllabus
        return new List<Syllabus>();
    }

    private List<Reference> ExtractReferences(string text)
    {
        // Placeholder: implement extraction logic for references
        return new List<Reference>();
    }
}
