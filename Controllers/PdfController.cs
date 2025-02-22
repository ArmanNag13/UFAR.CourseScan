/*
 * using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UFAR.PDFSync.Services;
using UFAR.PDFSync.DAO;
using UFAR.PDFSync.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UFAR.PDFSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPdfService _pdfService;

        public PdfController(ApplicationDbContext context, IPdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        // POST: api/Pdf/Upload
        [HttpPost("Upload")]
        public async Task<IActionResult> UploadPdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                // Define a file path to temporarily store the uploaded file
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedPdfs", file.FileName);

                // Ensure the directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                // Save the file to the server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Extract text from the uploaded PDF and save it to the database
                var extractedText = await _pdfService.ExtractTextAndSaveAsync(filePath, file.FileName);

                return Ok(new { message = "File uploaded successfully", extractedText });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Pdf/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPdfDocument(int id)
        {
            var pdfDocument = await _context.PdfDocuments.FindAsync(id);
            if (pdfDocument == null)
            {
                return NotFound("Document not found.");
            }

            return Ok(pdfDocument);
        }

        // GET: api/Pdf
        [HttpGet]
        public async Task<IActionResult> GetPdfDocuments()
        {
            var pdfDocuments = await _context.PdfDocuments.ToListAsync();
            return Ok(pdfDocuments);
        }

        // PUT: api/Pdf/SetAsDefault/{id}
        [HttpPut("SetAsDefault/{id}")]
        public async Task<IActionResult> SetAsDefault(int id)
        {
            var pdfDocument = await _context.PdfDocuments.FindAsync(id);
            if (pdfDocument == null)
            {
                return NotFound("Document not found.");
            }

            // Set the IsDefault property of the selected document to true
            pdfDocument.IsDefault = true;

            // Set IsDefault to false for all other documents to ensure only one is marked as default
            var otherDocuments = await _context.PdfDocuments.Where(d => d.Id != id).ToListAsync();
            foreach (var document in otherDocuments)
            {
                document.IsDefault = false;
            }

            try
            {
                // Save changes to the database
                await _context.SaveChangesAsync();
                return Ok(new { message = "Document set as default successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
*/
