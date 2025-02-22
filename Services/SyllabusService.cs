using Microsoft.EntityFrameworkCore;
using UFAR.PDFSync.DAO;
using UFAR.PDFSync.Services;

public class SyllabusService : ISyllabusService
{
    private readonly ApplicationDbContext _context;

    public SyllabusService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SaveSyllabiAsync(List<SubjectSyllabus> syllabi)
    {
        foreach (var syllabus in syllabi)
        {
            var existingSyllabus = await _context.SubjectSyllabi
                .FirstOrDefaultAsync(s => s.Subject == syllabus.Subject && s.AcademicYear == syllabus.AcademicYear);

            if (existingSyllabus == null)
            {
                _context.SubjectSyllabi.Add(syllabus);
            }
            else
            {
                existingSyllabus.AcademicYear = syllabus.AcademicYear;
                existingSyllabus.CmHours = syllabus.CmHours;
                existingSyllabus.Degree = syllabus.Degree;
                existingSyllabus.EctsCredits = syllabus.EctsCredits;
                existingSyllabus.Professor = syllabus.Professor;
                existingSyllabus.Qualification = syllabus.Qualification;
                existingSyllabus.TdHours = syllabus.TdHours;
                _context.SubjectSyllabi.Update(existingSyllabus);
            }
        }

        await _context.SaveChangesAsync();
    }
}
