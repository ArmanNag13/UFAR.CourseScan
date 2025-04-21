using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UFAR.PDFSync.Entities;
using UFAR.PDFSync.Services;

public class PdfParser : IPdfParser
{
    public List<SubjectSyllabus> ParseSyllabusText(string extractedText)
    {
        var syllabi = new List<SubjectSyllabus>();

        // Split the extracted text into lines for easier parsing
        var lines = extractedText.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var syllabus = new SubjectSyllabus
        {
            Subject = ExtractValue(lines, "LOGIC") ?? "Unknown",
            AcademicYear = ExtractValue(lines, "ACADEMIC YEAR") ?? "Unknown",
            Qualification = ExtractValue(lines, "Qualification") ?? "Unknown",
            Degree = ExtractValue(lines, "Degree") ?? "Unknown",
            Professor = ExtractValue(lines, "Professor") ?? "Unknown",
            CmHours = ExtractNumericValue(lines, "CM") ?? "0",
            TdHours = ExtractNumericValue(lines, "TD") ?? "" +
                        ExtractNumericValue(lines, "TP") ?? "0",
            EctsCredits = ExtractNumericValue(lines, "ECTS") ?? "0"
        };

        syllabi.Add(syllabus);

        // Log extracted values for debugging
        Console.WriteLine("Parsed Syllabus:");
        Console.WriteLine($"Subject: {syllabus.Subject}");
        Console.WriteLine($"Academic Year: {syllabus.AcademicYear}");
        Console.WriteLine($"Qualification: {syllabus.Qualification}");
        Console.WriteLine($"Degree: {syllabus.Degree}");
        Console.WriteLine($"Professor: {syllabus.Professor}");
        Console.WriteLine($"CM Hours: {syllabus.CmHours}");
        Console.WriteLine($"TD Hours: {syllabus.TdHours}");
        Console.WriteLine($"ECTS Credits: {syllabus.EctsCredits}");

        return syllabi;
    }

    // Extracts a value based on a given keyword
    private string ExtractValue(string[] lines, string fieldName)
    {
        foreach (var line in lines)
        {
            if (line.Contains(fieldName, StringComparison.OrdinalIgnoreCase))
            {
                var parts = line.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    return parts[1].Trim();
                }
                return line.Replace(fieldName, "", StringComparison.OrdinalIgnoreCase).Trim();
            }
        }
        return "Unknown";
    }

    // Extracts a numeric value for hours (CM, TD, ECTS)
    private string ExtractNumericValue(string[] lines, string fieldName)
    {
        foreach (var line in lines)
        {
            if (line.Contains(fieldName, StringComparison.OrdinalIgnoreCase))
            {
                var match = Regex.Match(line, $"{fieldName}\\s*(\\d+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }
        }
        return "0";
    }
}