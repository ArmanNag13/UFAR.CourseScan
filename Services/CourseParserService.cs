using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

public class CourseParserService : ICourseParserService
{
    public Course ParseCourse(string text)
    {
        var isFrench = DetectLanguage(text);
        var isArmenian = IsArmenian(text);
        var course = new Course();

        Console.WriteLine("Parsing Course Information...");
        ParseCourseInfo(text, course, isFrench);

        if (isArmenian)
        {
            course.LearningOutcomes = ParseArmenianLearningOutcomes(text);
            course.Assessments = ParseArmenianAssessments(text);
            course.TeachingMethods = ParseArmenianTeachingMethods(text);
            course.Prerequisites = ParseArmenianPrerequisites(text);
            course.Syllabus = ParseArmenianSyllabus(text);
            course.References = ParseArmenianReferences(text);
        }
        else
        {
            course.LearningOutcomes = ParseLearningOutcomes(text, isFrench);
            course.Assessments = ParseAssessments(text, isFrench);
            course.TeachingMethods = ParseTeachingMethods(text, isFrench);
            course.Prerequisites = ParsePrerequisites(text, isFrench);
            course.Syllabus = ParseSyllabus(text, isFrench);
            course.References = ParseReferences(text, isFrench);
        }

        Console.WriteLine("Course parsed successfully.");
        return course;
    }

    private bool DetectLanguage(string text)
    {
        Console.WriteLine("Detecting language...");

        if (text.Contains("ANNEE ACADEMIQUE", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("RESULTATS ATTENDUS", StringComparison.OrdinalIgnoreCase))
        {
            return true; // French
        }

        return false; // Default to English
    }

    private bool IsArmenian(string text)
    {
        return text.Contains("ուսումնական տարի") ||
               text.Contains("Դասախոս՝") ||
               text.Contains("Կրթական ծրագիր՝");
    }

    private void ParseCourseInfo(string text, Course course, bool isFrench)
    {
        Console.WriteLine("Parsing Course Info...");

        course.Title = ExtractTitle(text);

        if (isFrench)
        {
            course.Language = "French";
            ParseFrenchCourseDetails(text, course);
        }
        else if (IsArmenian(text))
        {
            course.Language = "Armenian";
            ParseArmenianCourseDetails(text, course);
        }
        else
        {
            course.Language = "English";
            ParseEnglishCourseDetails(text, course);
        }

        ExtractCourseHoursAndECTS(text, course);
    }

    private string ExtractTitle(string text)
    {
        // Try Armenian title first
        var armenianTitleMatch = Regex.Match(text, @"^(.*?)\s*[\r\n]+2024-2025", RegexOptions.Multiline);
        if (armenianTitleMatch.Success)
            return armenianTitleMatch.Groups[1].Value.Trim();

        // Fall back to English/French title pattern
        var titleMatch = Regex.Match(text, @"^(.*?)\s*(?:ACADEMIC YEAR|ANNEE ACADEMIQUE)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        return titleMatch.Success ? titleMatch.Groups[1].Value.Trim() : string.Empty;
    }

    private void ParseFrenchCourseDetails(string text, Course course)
    {
        course.AcademicYear = GetMatch(text, @"ANNEE ACADEMIQUE\s*[:]?\s*(\d{4}-\d{4})", RegexOptions.IgnoreCase);
        course.Degree = GetMatch(text, @"Dipl[oô]me\s*[:]?\s*(.+)", RegexOptions.IgnoreCase);
        course.Qualification = GetMatch(text, @"(?:Qualification|Mention)\s*[:]?\s*(.+)", RegexOptions.IgnoreCase);
        course.Professor = GetMatch(text, @"Enseignant\s*[:]?\s*(.+)", RegexOptions.IgnoreCase);
    }

    private void ParseEnglishCourseDetails(string text, Course course)
    {
        course.AcademicYear = GetMatch(text, @"ACADEMIC YEAR\s*[:]?\s*(\d{4}-\d{4})", RegexOptions.IgnoreCase);
        course.Degree = GetMatch(text, @"Degree\s*[:]?\s*(.+)", RegexOptions.IgnoreCase);
        course.Qualification = GetMatch(text, @"Qualification\s*[:]?\s*(.+)", RegexOptions.IgnoreCase);
        course.Professor = GetMatch(text, @"Professor\s*(?:PhD)?[,\-:\s]*\s*(.+)", RegexOptions.IgnoreCase);
    }

    private void ParseArmenianCourseDetails(string text, Course course)
    {
        course.AcademicYear = GetMatch(text, @"(\d{4}-\d{4})\s*ուսումնական տարի");
        course.Degree = GetMatch(text, @"Կրթական ծրագիր՝\s*(.+)");
        course.Qualification = GetMatch(text, @"Մասնագիտություն՝\s*(.+)");

        var professors = Regex.Matches(text, @"Դասախոս՝\s*([^\n]+)", RegexOptions.Multiline)
                            .Cast<Match>()
                            .Select(m => m.Groups[1].Value.Trim())
                            .ToList();

        if (professors.Count > 0)
        {
            course.Professor = string.Join(", ", professors);
        }
    }

    private void ExtractCourseHoursAndECTS(string text, Course course)
    {
        // Handle Armenian hours format (Դ=lecture, Գ=practical)
        if (course.Language == "Armenian")
        {
            var lectureMatch = Regex.Match(text, @"դաս\.\s*(\d+)ժ\.");
            if (lectureMatch.Success)
                course.HoursCM = int.Parse(lectureMatch.Groups[1].Value);

            var practicalMatch = Regex.Match(text, @"դաս\.\s*\|\s*գործ\.\s*(\d+)ժ\.");
            if (practicalMatch.Success)
                course.HoursTD = int.Parse(practicalMatch.Groups[1].Value);

            var labMatch = Regex.Match(text, @"դաս\.\s*\|\s*լաբ\.\s*(\d+)ժ\.");
            if (labMatch.Success)
                course.HoursTP = int.Parse(labMatch.Groups[1].Value);
        }
        else
        {
            // Original French/English format
            var hoursPattern = @"(?i)\b(CM|TD|TP|TPS?)\s*[:]?\s*(\d+(?:\.\d+)?)\s*h\.?";
            foreach (Match match in Regex.Matches(text, hoursPattern, RegexOptions.IgnoreCase))
            {
                var type = match.Groups[1].Value.ToUpperInvariant();
                if (int.TryParse(match.Groups[2].Value, out var hrs))
                {
                    switch (type)
                    {
                        case "CM": course.HoursCM = hrs; break;
                        case "TD": course.HoursTD = hrs; break;
                        case "TP": course.HoursTP = hrs; break;
                        case "TPS": course.HoursTP = hrs; break;
                    }
                }
            }
        }

        // ECTS parsing (common for all languages)
        var ectsPattern = @"(?i)\bECTS\s*[:\-]?\s*(\d+(?:\.\d+)?)\b";
        var matches = Regex.Matches(text, ectsPattern, RegexOptions.IgnoreCase);
        foreach (Match match in matches)
        {
            if (double.TryParse(match.Groups[1].Value, out var ects))
            {
                course.CreditHours = (int)Math.Round(ects);
                Console.WriteLine($"✔️ Parsed ECTS: {course.CreditHours}");
                break;
            }
        }

        if (course.CreditHours == 0)
        {
            Console.WriteLine("❌ ECTS not found! Please review the format in the document.");
        }
    }

    // Armenian-specific parsers
    private List<LearningOutcome> ParseArmenianLearningOutcomes(string text)
    {
        var outcomes = new List<LearningOutcome>();
        var section = GetSection(text, "ԱԿԸՆԿԱԼՎՈՂ ԱՐԴՅՈՒՆՔՆԵՐ", new[] { "ԳՆԱՀԱՏՄԱՆ ՁԵՎԸ", "ՄԱՆԿԱՎԱՐԺԱԿԱՆ ՄԵԹՈԴՆԵՐ" });

        section = Regex.Replace(section, @"\r?\n", "\n").Trim();
        section = Regex.Replace(section, @"(?<=\S)\n(?![Ա-Գ]\s*\d)", " ");

        var pattern = @"(?<code>[Ա-Գ]\s*\d(?:\.\d+)?)[\s:.-]+(?<desc>.*?)(?=\n[Ա-Գ]\s*\d(?:\.\d+)?|$)";
        var matches = Regex.Matches(section, pattern, RegexOptions.Singleline);

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

    private List<Assessment> ParseArmenianAssessments(string text)
    {
        var assessments = new List<Assessment>();
        var section = GetSection(text, "ԳՆԱՀԱՏՄԱՆ ՁԵՎԸ", new[] { "ՄԱՆԿԱՎԱՐԺԱԿԱՆ ՄԵԹՈԴՆԵՐ", "ԴԱՍԸՆԹԱՑԻ ՊԼԱՆԸ" });

        var types = new Dictionary<string, string>
        {
            {"Ընթացիկ աշխատանքներ", "Ongoing evaluation"},
            {"Միջանկյալ քննություն", "Midterm exam"},
            {"Կիսամյակային քննություն", "Final exam"}
        };

        foreach (var type in types)
        {
            if (section.Contains(type.Key))
            {
                var method = section.Contains("Բանավոր") ? "Oral" :
                            section.Contains("Գրավոր") ? "Written" : "Unknown";

                assessments.Add(new Assessment
                {
                    Type = type.Value,
                    Method = method
                });
            }
        }

        Console.WriteLine($"Parsed Assessments: {assessments.Count}");
        return assessments;
    }

    private List<TeachingMethod> ParseArmenianTeachingMethods(string text)
    {
        var methods = new List<TeachingMethod>();
        var section = GetSection(text, "ՄԱՆԿԱՎԱՐԺԱԿԱՆ ՄԵԹՈԴՆԵՐ", new[] { "ԳՐԱԿԱՆՈՒԹՅՈՒՆ", "ԴԱՍԸՆԹԱՑԻ ՊԼԱՆԸ" });

        var armenianMethods = new[]
        {
            "դասախոսություն", "գործնական պարապմունք", "ինքնուրույն աշխատանք",
            "խնդիրների լուծում", "ներկայացում", "քննարկում"
        };

        foreach (var method in armenianMethods)
        {
            if (section.Contains(method))
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

    private List<Prerequisite> ParseArmenianPrerequisites(string text)
    {
        var prerequisites = new List<Prerequisite>();
        var section = GetSection(text, "ԱՆՀՐԱԺԵՇՏ ՆԱԽՆԱԿԱՆ ԳԻՏԵԼԻՔՆԵՐ", new[] { "ԴԱՍԸՆԹԱՑԻ ՊԼԱՆԸ", "ԹԵՄԱ" });

        var lines = section.Split(new[] { '\n', '•', '–', '-' }, StringSplitOptions.RemoveEmptyEntries);

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

    private List<Syllabus> ParseArmenianSyllabus(string text)
    {
        var syllabi = new List<Syllabus>();
        var section = GetSection(text, "ԴԱՍԸՆԹԱՑԻ ՊԼԱՆԸ", new[] { "ՀԻՄՆԱԿԱՆ ԳՐԱԿԱՆՈՒԹՅԱՆ ՑԱՆԿ", "ԼՐԱՑՈՒՑԻՉ ԳՐԱԿԱՆՈՒԹՅՈՒՆ" });

        var pattern = @"(?<topic>.+?)\nԴ–(?<lecture>\d+(?:\.\d+)?)\s*Գ–(?<practical>\d+(?:\.\d+)?)";

        //foreach (Match match in Regex.Matches(section, pattern))
        //{
        //    syllabi.Add(new Syllabus
        //    {
        //        Topic = match.Groups["topic"].Value.Trim(),
        //        HoursLecture = double.Parse(match.Groups["lecture"].Value),
        //        HoursPractical = double.Parse(match.Groups["practical"].Value)
        //    });
        //}

        Console.WriteLine($"Parsed Syllabus: {syllabi.Count}");
        return syllabi;
    }

    private List<Reference> ParseArmenianReferences(string text)
    {
        var references = new List<Reference>();
        var section = GetSection(text, "ՀԻՄՆԱԿԱՆ ԳՐԱԿԱՆՈՒԹՅԱՆ ՑԱՆԿ", new[] { "ԼՐԱՑՈՒՑԻՉ ԳՐԱԿԱՆՈՒԹՅՈՒՆ", "ԻՆՏԵՐՆԵՏԱՅԻՆ ԿԱՅՔԵՐ" });

        var pattern = @"(?<author>.+?)<<(.+?)>>,\s*(?<year>\d{4})";

        foreach (Match match in Regex.Matches(section, pattern))
        {
            references.Add(new Reference
            {
                Author = match.Groups["author"].Value.Trim(),
                Title = match.Groups[2].Value.Trim(),
                Year = int.Parse(match.Groups["year"].Value)
            });
        }

        Console.WriteLine($"Parsed References: {references.Count}");
        return references;
    }

    // Original parsers for French/English
    private List<LearningOutcome> ParseLearningOutcomes(string text, bool isFrench)
    {
        Console.WriteLine("Parsing Learning Outcomes...");
        var outcomes = new List<LearningOutcome>();

        var sectionTitle = isFrench
            ? "RESULTAT[S]? ATTENDUS? DE L[’']ENSEIGNEMENT"
            : "EXPECTED LEARNING OUTCOMES OF THE COURSE";

        var section = GetSection(text, sectionTitle, new[] {
            "KNOWLEDGE / SKILLS ASSESSMENT", "MODALITES D'EVALUATION",
            "ASSESSMENT", "EVALUATION", "TEACHING METHODS"
        }, isRegex: true);

        section = Regex.Replace(section, @"\r?\n", "\n").Trim();
        section = Regex.Replace(section, @"(?<=\S)\n(?![A-C]\s*\d)", " ");

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

        section = Regex.Replace(section, @"\r?\n", "\n").Trim();

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

        section = Regex.Replace(section, @"\r?\n", "\n");

        var knownMethods = new[]
        {
            "Lecture", "Practical Work", "Self-study", "Modeling", "Slideshow",
            "Presentation", "Exercises", "Demonstration", "Problem solving",
            "Video-presentation", "Instruction with demonstration", "Individual work",
            "Explanation", "Study of textbooks", "Sources"
        };

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

        section = Regex.Replace(section, @"\r?\n", "\n").Trim();

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

        section = Regex.Replace(section, @"\r?\n", "\n").Trim();

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

    private List<Reference> ParseReferences(string text, bool isFrench)
    {
        Console.WriteLine("Parsing References...");
        var refs = new List<Reference>();

        var sectionTitle = isFrench ? "BIBLIOGRAPHIE" : "CORE REFERENCES";
        var section = GetSection(text, sectionTitle, new[] { "WEB RESOURCES", "STRUCTURE", "ADDITIONAL" });

        section = Regex.Replace(section, @"\r?\n", "\n").Trim();

        var pattern = @"(?<author>.+?)?\s*(?<title>.+?)[,|]\s*(?<year>\d{4})";

        foreach (Match match in Regex.Matches(section, pattern, RegexOptions.Singleline))
        {
            var title = match.Groups["title"].Value.Trim();
            var yearString = match.Groups["year"].Value;

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

    // Utility methods
    private string GetMatch(string text, string pattern, RegexOptions options = RegexOptions.IgnoreCase)
    {
        var match = Regex.Match(text, pattern, options);
        return match.Success ? match.Groups[1].Value.Trim().TrimEnd('.', ',') : string.Empty;
    }

    private string GetSection(string text, string startPattern, string[] endMarkers, bool isRegex = false)
    {
        Console.WriteLine($"Getting Section: {startPattern}");

        int start = -1;

        if (isRegex)
        {
            var match = Regex.Match(text, startPattern, RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                Console.WriteLine("Start marker (regex) not found: " + startPattern);
                return "";
            }
            start = match.Index + match.Length;
        }
        else
        {
            start = text.IndexOf(startPattern, StringComparison.OrdinalIgnoreCase);
            if (start == -1)
            {
                Console.WriteLine("Start marker not found: " + startPattern);
                return "";
            }
            start += startPattern.Length;
        }

        int end = text.Length;
        foreach (var marker in endMarkers)
        {
            int markerIndex = text.IndexOf(marker, start, StringComparison.OrdinalIgnoreCase);
            if (markerIndex != -1 && markerIndex < end)
                end = markerIndex;
        }

        Console.WriteLine("Section extracted from: " + start + " to " + end);
        return text.Substring(start, end - start);
    }

    private int ExtractHours(string input)
    {
        var match = Regex.Match(input, @"(\d+(?:\.\d+)?)\s*h", RegexOptions.IgnoreCase);
        if (match.Success && double.TryParse(match.Groups[1].Value, out var hours))
        {
            return (int)Math.Round(hours);
        }
        return 0;
    }
}