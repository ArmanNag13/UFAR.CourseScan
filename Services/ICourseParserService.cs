using UFAR.PDFSync.Entities;

public interface ICourseParserService
{
    // Parses the entire course text and returns a Course entity
    Course ParseCourse(string courseText);
}
