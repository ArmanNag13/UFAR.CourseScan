using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using UFAR.PDFSync.Entities;

namespace UFAR.PDFSync.DAO
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<PdfDocumentEntity> PdfDocuments { get; set; }
    }
}
