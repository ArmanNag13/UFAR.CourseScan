using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using UFAR.PDFSync.DAO;
using Microsoft.Extensions.Logging; // Optional: For logging

public class CourseController : Controller
{
    private readonly IPdfService _pdfService;  // Inject PdfService
    private readonly ICourseParserService _courseParserService;  // Inject CourseParserService
    private readonly ApplicationDbContext _dbContext;  // Your database context
    private readonly ILogger<CourseController> _logger;  // Optional: For logging

    // Constructor to inject the services
    public CourseController(IPdfService pdfService, ICourseParserService courseParserService, ApplicationDbContext dbContext, ILogger<CourseController> logger)
    {
        _pdfService = pdfService;
        _courseParserService = courseParserService;
        _dbContext = dbContext;
        _logger = logger; // Injecting logger for better error tracking
    }

    // Action to upload and parse the PDF file
    [HttpPost("upload-course")]
    public async Task<IActionResult> UploadCourse(IFormFile pdfFile)
    {
        if (pdfFile == null || pdfFile.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        // Create a file path to temporarily save the uploaded PDF
        var filePath = Path.Combine(Path.GetTempPath(), pdfFile.FileName);

        try
        {
            // Save the uploaded PDF to the temporary path
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await pdfFile.CopyToAsync(stream);
            }

            // Extract the text from the uploaded PDF using PdfService
            var extractedText = _pdfService.ExtractTextFromPdf(filePath);

            if (string.IsNullOrEmpty(extractedText))
            {
                return BadRequest("Failed to extract text from the PDF.");
            }

            // Parse the extracted text into a Course entity
            var course = _courseParserService.ParseCourse(extractedText);
            Console.WriteLine(extractedText);

            if (course == null)
            {
                return BadRequest("Failed to parse course from the extracted text.");
            }

            // Add the parsed course to the database
            _dbContext.Courses.Add(course);
            await _dbContext.SaveChangesAsync();

            // Optionally, log the successful upload (useful for debugging and production environments)
            _logger?.LogInformation($"Course {course.Title} uploaded and parsed successfully.");

            // Return a response with the parsed course data
            return Ok(new
            {
               
                Message = "Course successfully uploaded and parsed.",
                Course = course


               
            });
            Console.WriteLine(course);
        }
        catch (Exception ex)
        {
            // Log the exception for better error tracking
            _logger?.LogError(ex, "An error occurred while uploading and parsing the course.");

            // Clean up the file if something goes wrong
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            // Return a general error message
            return StatusCode(500, "An error occurred while processing the course.");
        }
        finally
        {
            // Clean up the file after processing, even if there was an error
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}
