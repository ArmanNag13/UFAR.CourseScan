using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

public class CourseParserService : ICourseParserService
{
    public Course ParseCourse(string text)
    {
        var isFrench = DetectLanguage(text);
        var course = new Course();

        Console.WriteLine("Parsing Course Information...");
        ParseCourseInfo(text, course, isFrench);
        course.LearningOutcomes = ParseLearningOutcomes(text, isFrench);
        course.Assessments = ParseAssessments(text, isFrench);
        course.TeachingMethods = ParseTeachingMethods(text, isFrench);
        course.Prerequisites = ParsePrerequisites(text, isFrench);
        course.Syllabus = ParseSyllabus(text, isFrench);
        course.References = ParseReferences(text, isFrench);

        Console.WriteLine("Course parsed successfully.");
        return course;
    }

    private bool DetectLanguage(string text)
    {
        Console.WriteLine("Detecting language...");
        return text.Contains("ANNEE ACADEMIQUE", StringComparison.OrdinalIgnoreCase) ||
               text.Contains("RESULTATS ATTENDUS", StringComparison.OrdinalIgnoreCase);
    }

    private void ParseCourseInfo(string text, Course course, bool isFrench)
    {
        Console.WriteLine("Parsing Course Info...");

        // Extract Title
        course.Title = ExtractTitle(text);

        // Extract Academic Year, Degree, Qualification, and Professor based on language
        if (isFrench)
        {
            ParseFrenchCourseDetails(text, course);
        }
        else
        {
            ParseEnglishCourseDetails(text, course);
        }

        // Set default language if not extracted
        course.Language = "English";

        // Extract Hours and ECTS
        ExtractCourseHoursAndECTS(text, course);
    }

    // Extract Title
    private string ExtractTitle(string text)
    {
        var titleMatch = Regex.Match(text, @"^(.*?)\s*(?:ACADEMIC YEAR|ANNEE ACADEMIQUE)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        return titleMatch.Success ? titleMatch.Groups[1].Value.Trim() : "";
    }

    // Parse French course details
    private void ParseFrenchCourseDetails(string text, Course course)
    {
        course.AcademicYear = GetMatch(text, @"ANNEE ACADEMIQUE\s+(\d{4}-\d{4})", RegexOptions.IgnoreCase);
        course.Degree = GetMatch(text, @"Dipl[oô]me\s*:\s*(.+)", RegexOptions.IgnoreCase);
        course.Qualification = GetMatch(text, @"(Qualification|Mention)\s*:\s*(.+)", RegexOptions.IgnoreCase)?.Split(new char[] { ':' }, 2).LastOrDefault()?.Trim() ?? "";
        course.Professor = GetMatch(text, @"Enseignant\s*:\s*(.+)", RegexOptions.IgnoreCase);
    }

    // Parse English course details
    private void ParseEnglishCourseDetails(string text, Course course)
    {
        course.AcademicYear = GetMatch(text, @"ACADEMIC YEAR\s+(\d{4}-\d{4})", RegexOptions.IgnoreCase);
        course.Degree = GetMatch(text, @"Degree\s*:\s*(.+)", RegexOptions.IgnoreCase);
        course.Qualification = GetMatch(text, @"Qualification\s*:\s*(.+)", RegexOptions.IgnoreCase);
        course.Professor = GetMatch(text, @"Professor\s*:\s*(.+)", RegexOptions.IgnoreCase);
    }

    // Extract course hours (CM, TP, TD, ECTS)
    private void ExtractCourseHoursAndECTS(string text, Course course)
    {
        var distPattern = @"(?i)(CM\s*(\d+)\s*h)|" +
                          @"(TP\s*(\d+)\s*h)|" +
                          @"(TD\s*(\d+)\s*h)|" +
                          @"(TPS\s*(\d+)\s*h)|" +
                          @"(ECTS\s*(\d+))";

        var distMatches = Regex.Matches(text, distPattern);

        foreach (Match match in distMatches)
        {
            if (match.Groups[2].Success && match.Value.Contains("CM", StringComparison.OrdinalIgnoreCase))
            {
                course.HoursCM = int.TryParse(match.Groups[2].Value, out int cm) ? cm : 0;
            }
            else if (match.Groups[4].Success && match.Value.Contains("TP", StringComparison.OrdinalIgnoreCase))
            {
                course.HoursTP = int.TryParse(match.Groups[4].Value, out int tp) ? tp : 0;
            }
            else if (match.Groups[6].Success && match.Value.Contains("TD", StringComparison.OrdinalIgnoreCase))
            {
                course.HoursTD = int.TryParse(match.Groups[6].Value, out int td) ? td : 0;
            }
            else if (match.Groups[9].Success && match.Value.Contains("ECTS", StringComparison.OrdinalIgnoreCase))
            {
                course.CreditHours = int.TryParse(match.Groups[9].Value, out int ects) ? ects : 0;
            }
        }
    }

    // Helper method for extracting a match with optional regex options
    private string GetMatch(string text, string pattern, RegexOptions options = RegexOptions.IgnoreCase)
    {
        var match = Regex.Match(text, pattern, options);
        return match.Success ? match.Groups[1].Value.Trim() : "";
    }


    private List<LearningOutcome> ParseLearningOutcomes(string text, bool isFrench)
    {
        Console.WriteLine("Parsing Learning Outcomes...");
        var outcomes = new List<LearningOutcome>();

        var sectionTitle = isFrench
            ? "RESULTATS ATTENDUS DE L'ENSEIGNEMENT"
            : "EXPECTED LEARNING OUTCOMES OF THE COURSE";

        // Stop at likely next section
        var section = GetSection(text, sectionTitle, new[]
        {
        "KNOWLEDGE / SKILLS ASSESSMENT", "MODALITES D’EVALUATION", "ASSESSMENT", "EVALUATION", "TEACHING METHODS"
    });

        // Normalize newlines
        section = Regex.Replace(section, @"\r?\n", "\n").Trim();

        // Combine lines where an outcome description is broken across lines (indent or no number)
        section = Regex.Replace(section, @"(?<=\S)\n(?![A-C]\s*\d)", " ");

        // Match learning outcomes
        var outcomePattern = @"(?<code>[A-C]\s*\d(?:\.\d+)?)[\s:.-]+(?<desc>.*?)(?=\n[A-C]\s*\d(?:\.\d+)?|$)";
        var matches = Regex.Matches(section, outcomePattern, RegexOptions.Singleline);

        foreach (Match match in matches)
        {
            outcomes.Add(new LearningOutcome
            {
                Code = match.Groups["code"].Value.Trim(),
                Description = match.Groups["desc"].Value.Trim()
            });
        }

        Console.WriteLine($"Parsed {outcomes.Count} learning outcomes.");
        return outcomes;
    }


    private List<Assessment> ParseAssessments(string text, bool isFrench)
    {
        Console.WriteLine("Parsing Assessments...");
        var assessments = new List<Assessment>();

        var sectionTitle = isFrench
            ? "MODALITES D'EVALUATION"
            : "KNOWLEDGE / SKILLS ASSESSMENT & EVALUATION";

        var section = GetSection(text, sectionTitle, new[]
        {
        "TEACHING METHODS", "MODALITES PEDAGOGIQUES", "SYLLABUS", "RESOURCES", "PREREQUISITES"
    });

        // Normalize line endings and whitespace
        section = Regex.Replace(section, @"\r?\n", "\n").Trim();

        // Capture each assessment item (type + optional method)
        var pattern = @"(?i)(Ongoing evaluation tasks?|Midterm exam|Final exam)[^\n]*?\n?(?:Assessment\s*:)?\s*(Oral|Written)?";

        var matches = Regex.Matches(section, pattern, RegexOptions.Singleline);
        foreach (Match match in matches)
        {
            var type = match.Groups[1].Value.Trim();
            var method = match.Groups[2].Success ? match.Groups[2].Value.Trim() : "Unknown";

            assessments.Add(new Assessment
            {
                Type = type,
                Method = method
            });
        }

        Console.WriteLine($"Parsed Assessments: {assessments.Count}");
        return assessments;
    }

    private List<TeachingMethod> ParseTeachingMethods(string text, bool isFrench)
    {
        Console.WriteLine("Parsing Teaching Methods...");
        var methods = new List<TeachingMethod>();

        var sectionTitle = isFrench ? "MODALITES PEDAGOGIQUES" : "TEACHING METHODS & TOOLS";
        var section = GetSection(text, sectionTitle, new[]
        {
        "PREREQUIS", "KNOWLEDGE", "SYLLABUS", "PREREQUISITES", "COURSE DESCRIPTION"
    });

        // Normalize the section text
        section = Regex.Replace(section, @"\r?\n", "\n");

        // Define common teaching methods — you can extend this list
        var knownMethods = new[]
        {
        "Lecture", "Practical Work", "Self-study", "Modeling", "Slideshow",
        "Presentation", "Exercises", "Demonstration", "Problem solving",
        "Video-presentation", "Instruction with demonstration", "Individual work",
        "Explanation", "Study of textbooks", "Sources"
    };

        // Look for each known method in the section (case-insensitive match)
        foreach (var method in knownMethods)
        {
            var pattern = $@"(?i)\b{Regex.Escape(method)}\b";
            if (Regex.IsMatch(section, pattern))
            {
                methods.Add(new TeachingMethod
                {
                    Method = method
                });
            }
        }

        Console.WriteLine($"Parsed Teaching Methods: {methods.Count}");
        return methods;
    }


    private List<Prerequisite> ParsePrerequisites(string text, bool isFrench)
    {
        Console.WriteLine("Parsing Prerequisites...");
        var prerequisites = new List<Prerequisite>();

        var sectionTitle = isFrench
            ? "PRE-REQUIS EN TERMES DE CONNAISSANCES"
            : "KNOWLEDGE & SKILLS PREREQUISITS";

        var section = GetSection(text, sectionTitle, new[]
        {
        "SYLLABUS", "COURSE DESCRIPTION", "STRUCTURE OF THE COURSE"
    });

        // Normalize newlines and trim
        section = Regex.Replace(section, @"\r?\n", "\n").Trim();

        // Split by line or bullet or comma if it contains multiple prerequisites
        var lines = section.Split(new[] { '\n', ',', '•', '–', '-' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var prereq = line.Trim();
            if (!string.IsNullOrWhiteSpace(prereq))
            {
                prerequisites.Add(new Prerequisite
                {
                    Description = prereq
                });
            }
        }

        Console.WriteLine($"Parsed Prerequisites: {prerequisites.Count}");
        return prerequisites;
    }


    private List<Syllabus> ParseSyllabus(string text, bool isFrench)
    {
        Console.WriteLine("Parsing Syllabus...");
        var syllabi = new List<Syllabus>();

        var sectionTitle = isFrench
            ? "PLAN DE COURS"
            : "COURSE DESCRIPTION /SYLLABUS / RESOURCES";

        var section = GetSection(text, sectionTitle, new[]
        {
        "CORE REFERENCES", "STRUCTURE", "ADDITIONAL", "BIBLIOGRAPHIE"
    });

        // Normalize
        section = Regex.Replace(section, @"\r?\n", "\n").Trim();

        // Pattern matches lines with optional hours (e.g., 1.5hCM or 14hTD) and a topic
        var pattern = @"(?<hours>\d+(?:\.\d+)?h(?:CM|TD|TP|TDS|TPS)?)\s+(?<topic>.+?)(?:\n|$)";
        foreach (Match match in Regex.Matches(section, pattern, RegexOptions.Multiline))
        {
            var hours = ExtractHours(match.Groups["hours"].Value);
            var topic = match.Groups["topic"].Value.Trim();

            if (!string.IsNullOrWhiteSpace(topic))
            {
                syllabi.Add(new Syllabus
                {
                    Topic = topic,
                    Hours = hours
                });
            }
        }

        Console.WriteLine($"Parsed Syllabus: {syllabi.Count}");
        return syllabi;
    }

    private int ExtractHours(string input)
    {
        // Matches like "1.5hCM", "14hTD", "13.5hTP", etc.
        var match = Regex.Match(input, @"(\d+(?:\.\d+)?)\s*h", RegexOptions.IgnoreCase);
        if (match.Success && double.TryParse(match.Groups[1].Value, out var hours))
        {
            return (int)Math.Round(hours); // round if needed
        }
        return 0;
    }


    private List<Reference> ParseReferences(string text, bool isFrench)
    {
        Console.WriteLine("Parsing References...");
        var refs = new List<Reference>();

        var sectionTitle = isFrench ? "BIBLIOGRAPHIE" : "CORE REFERENCES";
        var section = GetSection(text, sectionTitle, new[] { "WEB RESOURCES", "STRUCTURE", "ADDITIONAL" });

        // Normalize the section
        section = Regex.Replace(section, @"\r?\n", "\n").Trim();

        // Pattern to handle common reference formats:
        // 1. Title, Year
        // 2. Title | Year
        // 3. Additional complex formats: Authors, Title, Year
        var pattern = @"(?<author>.+?)?\s*(?<title>.+?)[,|]\s*(?<year>\d{4})";

        foreach (Match match in Regex.Matches(section, pattern, RegexOptions.Singleline))
        {
            var title = match.Groups["title"].Value.Trim();
            var yearString = match.Groups["year"].Value;

            // Parse year safely
            if (int.TryParse(yearString, out var year))
            {
                refs.Add(new Reference
                {
                    Title = title,
                    Year = year
                });
            }
        }

        Console.WriteLine($"Parsed References: {refs.Count}");
        return refs;
    }


    // --- UTILITY METHODS ---

    private string GetMatch(string text, string pattern, bool ignoreCase = false)
    {
        var options = ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
        var match = Regex.Match(text, pattern, options);
        return match.Success ? match.Groups[1].Value.Trim() : "";
    }

    private string GetSection(string text, string startMarker, string[] endMarkers)
    {
        Console.WriteLine($"Getting Section: {startMarker}");
        int start = text.IndexOf(startMarker, StringComparison.OrdinalIgnoreCase);
        if (start == -1)
        {
            Console.WriteLine("Start marker not found: " + startMarker);
            return "";
        }

        int end = text.Length;
        foreach (var marker in endMarkers)
        {
            int markerIndex = text.IndexOf(marker, start + startMarker.Length, StringComparison.OrdinalIgnoreCase);
            if (markerIndex != -1 && markerIndex < end)
                end = markerIndex;
        }

        Console.WriteLine("Section extracted from: " + start + " to " + end);
        return text.Substring(start, end - start);
    }
}
