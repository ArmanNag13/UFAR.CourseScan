using System;
using System.ComponentModel.DataAnnotations;

namespace UFAR.PDFSync.Entities
{
    public class PdfDocumentEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public string ExtractedText { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public bool IsDefault { get; set; }
    }
}
