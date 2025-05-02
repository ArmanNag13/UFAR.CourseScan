using Azure.AI.OpenAI;
using OpenAI.Chat;
using UFAR.PDFSync.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using UFAR.PDFSync.DAO;
using UFAR.PDFSync.Services;
using System.ClientModel;

namespace UFAR.TimeManagmentTracker.Backend.Services
{
    public class AIService : IAIService
    {
        private readonly AzureOpenAIClient _azureClient;
        private readonly ChatClient _chatClient;
        private readonly ApplicationDbContext _context;

        public AIService(ApplicationDbContext context)
        {
            string endpoint = "https://i22m-m3in499z-westeurope.openai.azure.com/";
            string key = "BvxxwtmW5qmCcJpyKNNT0NkJjoCofoaEXpFSLwevwJ6d5XKPq1tJJQQJ99AKAC5RqLJXJ3w3AAAAACOGTiEi";
            ApiKeyCredential credential = new ApiKeyCredential(key);
            _azureClient = new AzureOpenAIClient(new Uri(endpoint), credential);
            _chatClient = _azureClient.GetChatClient("gpt-35-turbo");

            _context = context;
        }

        // Existing function to get a response from AI
        public async Task<string> GetAIResponseAsync(string userMessage)
        {
            try
            {
                var completion = await _chatClient.CompleteChatAsync(
                    new ChatMessage[]
                    {
                        new SystemChatMessage("You are an intelligent assistant designed to help users organize and manage their daily tasks, deadlines, and exams..."),
                        new UserChatMessage(userMessage)
                    }
                );

                var aiResponse = completion.Value.Content[0].Text;

                return aiResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while getting AI response: {ex.Message}");
                throw;
            }
        }

        // New function to get feedback based on parsed course data
        public async Task<string> GetCourseFeedbackAsync(Course course)
        {
            try
            {
                // Constructing prompt with parsed course data
                var prompt = $@"
You are an academic assistant. Analyze the following university course plan and provide:

- A short summary (2-3 sentences)
- Feedback on structure (e.g., missing sections, unbalanced assessments)
- Suggestions to improve clarity or completeness

Course data:
Title: {course.Title}
Academic Year: {course.AcademicYear}
Language: {course.Language}
ECTS: {course.ECTS}
Professor: {course.Professor}
Learning Outcomes: {string.Join("\n", course.LearningOutcomes.Select(lo => $"- {lo.Description}"))}
Assessments: {string.Join("\n", course.Assessments.Select(a => $"- {a.Type}: {a.Method}"))}
Teaching Methods: {string.Join("\n", course.TeachingMethods.Select(tm => $"- {tm.Method}"))}
Syllabus: {string.Join("\n", course.Syllabus.Select(s => $"- {s.Topic} ({s.Hours}h)"))}
References: {string.Join("\n", course.References.Select(r => $"- {r.Title} by {r.Author}"))}

Respond in markdown format.
";

                var completion = await _chatClient.CompleteChatAsync(
                    new ChatMessage[]
                    {
                        new SystemChatMessage("You are an assistant specialized in academic course analysis. Provide a concise and informative summary and feedback on the course structure."),
                        new UserChatMessage(prompt)
                    }
                );

                var aiResponse = completion.Value.Content[0].Text;

                return aiResponse.Trim(); // Return the concise feedback result
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while analyzing course: {ex.Message}");
                throw;
            }
        }

        // Updated function to compare two texts concisely
        public async Task<string> CompareTextsAsync(string text1, string text2)
        {
            try
            {
                var prompt = $@"
I have two texts. Please compare them and list only the differences, changes, or missing words between the two, in a concise format. 

Text 1:
{text1}

Text 2:
{text2}

Only provide a list of the differences without any additional explanation or details.
";

                var completion = await _chatClient.CompleteChatAsync(
                    new ChatMessage[]
                    {
                        new SystemChatMessage("You are an assistant specialized in comparing texts. Provide only the differences in a short and clear format, with no extra details."),
                        new UserChatMessage(prompt)
                    }
                );

                var comparisonResult = completion.Value.Content[0].Text;

                return comparisonResult.Trim(); // Return the concise comparison result
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while comparing texts: {ex.Message}");
                throw;
            }
        }
    }
}
