using System.Collections.Generic;
using System.Threading.Tasks;

namespace UFAR.PDFSync.Services
{
    public interface ISyllabusService
    {
        Task SaveSyllabiAsync(List<SubjectSyllabus> syllabi);
    }
}
