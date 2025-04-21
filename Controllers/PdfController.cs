using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using UFAR.PDFSync.Services;
using UFAR.PDFSync.DAO;
using UFAR.PDFSync.Entities;

namespace UFAR.PDFSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfController : ControllerBase
    {
        private readonly IPdfService _pdfService;
        private readonly ICourseParserService _courseParserService;
        private readonly ApplicationDbContext _context;

        public PdfController(IPdfService pdfService, ICourseParserService courseParserService, ApplicationDbContext context)
        {
            _pdfService = pdfService;
            _courseParserService = courseParserService;
            _context = context;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadPdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                var uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                if (!Directory.Exists(uploadDirectory))
                {
                    Directory.CreateDirectory(uploadDirectory);
                }

                var filePath = Path.Combine(uploadDirectory, file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var extractedText = _pdfService.ExtractTextFromPdf(filePath);

                if (string.IsNullOrEmpty(extractedText))
                {
                    return BadRequest("Failed to extract text from the PDF.");
                }

                var course = _courseParserService.ParseCourse(extractedText);

                if (course == null)
                {
                    return BadRequest("Failed to parse course data from the extracted text.");
                }

                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                return Ok("PDF data uploaded, parsed, and saved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
