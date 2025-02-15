using System.IO;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UFAR.PDFSync.DAO;
using UFAR.PDFSync.Entities;

namespace UFAR.PDFSync.Services
{
    public class PdfService : IPdfService
    {
        private readonly ApplicationDbContext _context;

        public PdfService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> ExtractTextAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found.");

            string extractedText = "";
            using (UglyToad.PdfPig.PdfDocument document = UglyToad.PdfPig.PdfDocument.Open(filePath)) // Explicitly reference PdfPig
            {
                foreach (var page in document.GetPages())
                {
                    extractedText += page.Text + "\n";
                }
            }

            return extractedText;
        }

        public async Task<string> ExtractTextAndSaveAsync(string filePath, string fileName)
        {
            string extractedText = await ExtractTextAsync(filePath);

            var pdfDoc = new PdfDocumentEntity // Explicitly reference your entity
            {
                FileName = fileName,
                ExtractedText = extractedText,
                IsDefault = false // Make this default or set it based on your condition
            };

            _context.PdfDocuments.Add(pdfDoc);
            await _context.SaveChangesAsync();

            return extractedText;
        }
    }
}
