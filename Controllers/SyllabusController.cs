using Microsoft.AspNetCore.Mvc;
using UFAR.PDFSync.Services;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UFAR.PDFSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SyllabusController : ControllerBase
    {
        private readonly IPdfService _pdfService;
        private readonly IPdfParser _pdfParser;
        private readonly ISyllabusService _syllabusService;

        // Constructor injection for dependencies
        public SyllabusController(IPdfService pdfService, IPdfParser pdfParser, ISyllabusService syllabusService)
        {
            _pdfService = pdfService;
            _pdfParser = pdfParser;
            _syllabusService = syllabusService;
        }

        // API endpoint for uploading and processing the PDF
        [HttpPost("upload-syllabus")]
        public async Task<IActionResult> UploadSyllabus(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                // Define the upload directory path and check if it exists
                var uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                if (!Directory.Exists(uploadDirectory))
                {
                    Directory.CreateDirectory(uploadDirectory); // Create if not exists
                }

                var filePath = Path.Combine(uploadDirectory, file.FileName);

                // Save the uploaded file locally
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Extract text from the uploaded PDF file
                var extractedText = _pdfService.ExtractTextFromPdf(filePath);

                // Log extracted text for debugging purposes
                if (!string.IsNullOrEmpty(extractedText))
                {
                    Console.WriteLine("Extracted Text: ");
                    Console.WriteLine(extractedText); // Log or output extracted text for inspection
                }
                else
                {
                    Console.WriteLine("No text extracted from the PDF.");
                }

                if (string.IsNullOrEmpty(extractedText))
                {
                    return BadRequest("Failed to extract text from the PDF.");
                }

                // Parse the extracted text into syllabus data
                var syllabi = _pdfParser.ParseSyllabusText(extractedText);

                // Check if syllabi were found after parsing
                if (syllabi == null || syllabi.Count == 0)
                {
                    return BadRequest("No syllabus data found in the extracted text.");
                }

                // Log parsed syllabi for debugging purposes
                Console.WriteLine("Parsed Syllabi:");
                foreach (var syllabus in syllabi)
                {
                    Console.WriteLine($"Subject: {syllabus.Subject}, AcademicYear: {syllabus.AcademicYear}, Qualification: {syllabus.Qualification}");
                }

                // Save the parsed syllabi to the database
                await _syllabusService.SaveSyllabiAsync(syllabi);

                return Ok("Syllabus data uploaded and saved successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception details (You can use a logging framework like Serilog, NLog, etc.)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
