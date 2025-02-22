using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UFAR.TimeManagmentTracker.Backend.Services;
using System.ComponentModel.DataAnnotations;
using UFAR.PDFSync.DAO;
using UFAR.PDFSync.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.Extensions.Logging;

namespace UFAR.TimeManagmentTracker.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIController : ControllerBase
    {
        private readonly IAIService _aiService;
        private readonly IPdfService _pdfService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AIController> _logger;

        public AIController(IAIService aiService, IPdfService pdfService, ApplicationDbContext context, ILogger<AIController> logger)
        {
            _aiService = aiService;
            _pdfService = pdfService;
            _context = context;
            _logger = logger;
        }

        // POST: api/ai/compare-pdf
        [HttpPost("compare-pdf")]
        public async Task<IActionResult> ComparePdf([FromBody] ComparePdfRequest request)
        {
            try
            {
                // Fetch the default document from the database (it should have IsDefault = true)
                var defaultPdfDocument = await _context.PdfDocuments
                    .FirstOrDefaultAsync(d => d.IsDefault);

                if (defaultPdfDocument == null)
                {
                    _logger.LogError("No default document found in the database.");
                    return NotFound("No default document found in the database.");
                }

                if (string.IsNullOrWhiteSpace(defaultPdfDocument.ExtractedText))
                {
                    _logger.LogError("The default PDF document has no extracted text.");
                    return BadRequest("The default PDF document has no extracted text.");
                }

                // Fetch the second PDF document using the provided file ID
                var secondPdfDocument = await _context.PdfDocuments
                    .FirstOrDefaultAsync(d => d.Id == request.SecondFileId);

                if (secondPdfDocument == null)
                {
                    _logger.LogError($"No document found with the provided file ID: {request.SecondFileId}");
                    return NotFound($"No document found with the provided file ID: {request.SecondFileId}");
                }

                if (string.IsNullOrWhiteSpace(secondPdfDocument.ExtractedText))
                {
                    _logger.LogError("The second PDF document has no extracted text.");
                    return BadRequest("The second PDF document has no extracted text.");
                }

                // Compare the default document text with the second document text using AIService
                var comparisonResult = await _aiService.CompareTextsAsync(defaultPdfDocument.ExtractedText, secondPdfDocument.ExtractedText);

                return Ok(new { comparisonResult });
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while comparing PDFs: {ex.Message}");
                return StatusCode(500, $"An error occurred while processing your request: {ex.Message}");
            }
        }

        // POST api/ai/ask-ai
        [HttpPost("ask-ai")]
        public async Task<IActionResult> AskAI([FromBody, Required, MinLength(1)] string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return BadRequest("Message cannot be empty.");
            }

            try
            {
                var response = await _aiService.GetAIResponseAsync(message);
                return Ok(new { response });
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while processing AI request: {ex.Message}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }

    // Request model for comparing PDF text
    public class ComparePdfRequest
    {
        [Required]
        public int SecondFileId { get; set; }  // Added the field for the second file ID
    }
}
