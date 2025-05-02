using System.Threading.Tasks;

namespace UFAR.PDFSync.Services
{
    public interface IAIService
    {
        Task<string> GetAIResponseAsync(string userMessage);
        Task<string> CompareTextsAsync(string text1, string text2);
        Task<string> GetCourseFeedbackAsync(Course course);

    }

}
