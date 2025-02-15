using System.Threading.Tasks;

namespace UFAR.TimeManagmentTracker.Backend.Services
{
    public interface IAIService
    {
        Task<string> GetAIResponseAsync(string userMessage);
        Task<string> CompareTextsAsync(string text1, string text2);
    }
}
