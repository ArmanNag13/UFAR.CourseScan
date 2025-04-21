public interface IPdfService
{
    string ExtractTextFromPdf(string filePath);

    string ExtractTextFromPdf(Stream stream);
}
