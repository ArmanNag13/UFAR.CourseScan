using System.Collections.Generic;

namespace UFAR.PDFSync.Services
{
    public interface IPdfParser
    {
        // Method to parse the extracted text into syllabus data
        List<SubjectSyllabus> ParseSyllabusText(string extractedText);
    }
}
