using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text;

public class PdfService : IPdfService
{
    // Method to extract text from the PDF
    public string ExtractTextFromPdf(string filePath)
    {
        var extractedText = new StringBuilder();

        // Open the PDF file
        using (var reader = new PdfReader(filePath))
        using (var pdfDoc = new PdfDocument(reader))
        {
            // Loop through each page in the PDF
            for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
            {
                var strategy = new LocationTextExtractionStrategy();
                var pageContent = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page), strategy);
                extractedText.Append(pageContent);
            }
        }

        // Return the full extracted text as a string
        return extractedText.ToString();
        Console.WriteLine("Extracted Text:");
        Console.WriteLine(extractedText);

    }

    public string ExtractTextFromPdf(Stream stream)
    {
        throw new NotImplementedException();
    }
}
