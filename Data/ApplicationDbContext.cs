using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using UFAR.PDFSync.Entities;

namespace UFAR.PDFSync.DAO
{


    using Microsoft.EntityFrameworkCore;

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }
        public DbSet<PdfDocumentEntity> PdfDocuments { get; set; }
        public DbSet<SubjectSyllabus> SubjectSyllabi { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<LearningOutcome> LearningOutcomes { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<TeachingMethod> TeachingMethods { get; set; }
        public DbSet<Prerequisite> Prerequisites { get; set; }
        public DbSet<Syllabus> Syllabi { get; set; }
        public DbSet<Reference> References { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define relationships
            modelBuilder.Entity<LearningOutcome>()
                .HasOne(lo => lo.Course)
                .WithMany(c => c.LearningOutcomes)
                .HasForeignKey(lo => lo.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Assessment>()
                .HasOne(a => a.Course)
                .WithMany(c => c.Assessments)
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TeachingMethod>()
                .HasOne(tm => tm.Course)
                .WithMany(c => c.TeachingMethods)
                .HasForeignKey(tm => tm.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Prerequisite>()
                .HasOne(p => p.Course)
                .WithMany(c => c.Prerequisites)
                .HasForeignKey(p => p.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Syllabus>()
                .HasOne(s => s.Course)
                .WithMany(c => c.Syllabus)
                .HasForeignKey(s => s.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reference>()
                .HasOne(r => r.Course)
                .WithMany(c => c.References)
                .HasForeignKey(r => r.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}