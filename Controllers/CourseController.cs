using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using UFAR.PDFSync.DAO;
using Microsoft.Extensions.Logging;
using System.Linq;

public class CourseController : Controller
{
    private readonly IPdfService _pdfService;
    private readonly ICourseParserService _courseParserService;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CourseController> _logger;

    public CourseController(IPdfService pdfService, ICourseParserService courseParserService, ApplicationDbContext dbContext, ILogger<CourseController> logger)
    {
        _pdfService = pdfService;
        _courseParserService = courseParserService;
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpPost("upload-course")]
    public async Task<IActionResult> UploadCourse(IFormFile pdfFile)
    {
        if (pdfFile == null || pdfFile.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        var filePath = Path.Combine(Path.GetTempPath(), pdfFile.FileName);

        try
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await pdfFile.CopyToAsync(stream);
            }

            var extractedText = _pdfService.ExtractTextFromPdf(filePath);

            if (string.IsNullOrEmpty(extractedText))
            {
                return BadRequest("Failed to extract text from the PDF.");
            }

            var course = _courseParserService.ParseCourse(extractedText);
            Console.WriteLine(extractedText);

            if (course == null)
            {
                return BadRequest("Failed to parse course from the extracted text.");
            }

            _dbContext.Courses.Add(course);
            await _dbContext.SaveChangesAsync();

            _logger?.LogInformation($"Course {course.Title} uploaded and parsed successfully.");

            // ✅ Return the Course object directly so frontend can deserialize it properly
            return Ok(course);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "An error occurred while uploading and parsing the course.");

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            return StatusCode(500, "An error occurred while processing the course.");
        }
        finally
        {
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }

    [HttpDelete("delete-course/{id}")]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        try
        {
            var course = await _dbContext.Courses.FindAsync(id);

            if (course == null)
            {
                return NotFound($"Course with ID {id} not found.");
            }

            _dbContext.Courses.Remove(course);
            await _dbContext.SaveChangesAsync();

            _logger?.LogInformation($"Course with ID {id} deleted successfully.");

            return Ok(new { Message = $"Course with ID {id} deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"An error occurred while deleting the course with ID {id}.");
            return StatusCode(500, "An error occurred while deleting the course.");
        }
    }

    [HttpDelete("delete-all-courses")]
    public async Task<IActionResult> DeleteAllCourses()
    {
        try
        {
            var allCourses = _dbContext.Courses.ToList();

            if (!allCourses.Any())
            {
                return NotFound("No courses found to delete.");
            }

            _dbContext.Courses.RemoveRange(allCourses);
            await _dbContext.SaveChangesAsync();

            _logger?.LogInformation("All courses deleted successfully.");

            return Ok(new { Message = "All courses deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "An error occurred while deleting all courses.");
            return StatusCode(500, "An error occurred while deleting all courses.");
        }
    }
}
