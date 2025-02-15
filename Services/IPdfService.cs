using System.Threading.Tasks;

namespace UFAR.PDFSync.Services
{
    public interface IPdfService
    {
        Task<string> ExtractTextAndSaveAsync(string filePath, string fileName);
    }
}
