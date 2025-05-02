using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UFAR.PDFSync.Services;
using System.ComponentModel.DataAnnotations;
using UFAR.PDFSync.DAO;
using UFAR.PDFSync.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace UFAR.PDFSync.Controllers
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

                var comparisonResult = await _aiService.CompareTextsAsync(defaultPdfDocument.ExtractedText, secondPdfDocument.ExtractedText);
                return Ok(new { comparisonResult });
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while comparing PDFs: {ex.Message}");
                return StatusCode(500, $"An error occurred while processing your request: {ex.Message}");
            }
        }

        // POST: api/ai/ask-ai
        [HttpPost("ask-ai")]
        public async Task<IActionResult> AskAI([FromBody] AIMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Message cannot be empty.");
            }

            try
            {
                var response = await _aiService.GetAIResponseAsync(request.Message);
                return Ok(new { response });
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while processing AI request: {ex.Message}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // POST: api/ai/course-summary
        [HttpPost("course-summary")]
        public async Task<IActionResult> GetCourseSummary([FromBody] CourseSummaryRequest request)
        {
            if (request.CourseId <= 0)
                return BadRequest("Invalid course ID.");

            try
            {
                var course = await _context.Courses
                    .Include(c => c.LearningOutcomes)
                    .Include(c => c.Assessments)
                    .Include(c => c.TeachingMethods)
                    .Include(c => c.Syllabus)
                    .Include(c => c.References)
                    .FirstOrDefaultAsync(c => c.Id == request.CourseId);

                if (course == null)
                    return NotFound("Course not found.");

                var prompt = $"Can you summarize this university course in a short paragraph? " +
                             $"Title: {course.Title}, Language: {course.Language}, ECTS: {course.ECTS}, " +
                             $"Learning Outcomes: {string.Join("; ", course.LearningOutcomes.Select(lo => lo.Description))}, " +
                             $"Assessments: {string.Join("; ", course.Assessments.Select(a => a.Method))}";

                var summary = await _aiService.GetAIResponseAsync(prompt);

                return Ok(new { summary });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating course summary: {ex.Message}");
                return StatusCode(500, "An error occurred while processing the course summary.");
            }
        }

        // Not an endpoint - internal usage only
        [NonAction]
        public async Task<Course> GetCourseAsync(int courseId)
        {
            try
            {
                var course = await _context.Courses
                    .Include(c => c.LearningOutcomes)
                    .Include(c => c.Assessments)
                    .Include(c => c.TeachingMethods)
                    .Include(c => c.Syllabus)
                    .Include(c => c.References)
                    .FirstOrDefaultAsync(c => c.Id == courseId);

                if (course == null)
                {
                    throw new Exception("Course not found.");
                }

                return course;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching course: {ex.Message}");
                throw;
            }
        }
    }

    // DTOs
    public class ComparePdfRequest
    {
        [Required]
        public int SecondFileId { get; set; }
    }

    public class AIMessageRequest
    {
        [Required]
        public string Message { get; set; } = string.Empty;
    }

    public class CourseSummaryRequest
    {
        [Required]
        public int CourseId { get; set; }
    }
}
