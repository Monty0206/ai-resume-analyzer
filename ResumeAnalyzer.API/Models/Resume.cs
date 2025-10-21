using System.ComponentModel.DataAnnotations;

namespace ResumeAnalyzer.API.Models
{
    /// <summary>
    /// Represents a resume document uploaded for analysis
    /// This model stores the original resume file data and metadata
    /// Created to track resume submissions and their analysis results
    /// </summary>
    public class Resume
    {
        /// <summary>
        /// Unique identifier for the resume
        /// Using GUID ensures uniqueness across distributed systems
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Original filename of the uploaded resume
        /// Preserving this helps users identify their submissions
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// File type/extension (pdf, docx, etc.)
        /// Important for determining which parser to use
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string FileType { get; set; } = string.Empty;

        /// <summary>
        /// Size of the uploaded file in bytes
        /// Useful for monitoring storage usage and validating uploads
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Binary data of the resume file
        /// Storing file content allows re-analysis without re-upload
        /// </summary>
        public byte[]? FileContent { get; set; }

        /// <summary>
        /// When the resume was uploaded
        /// Essential for tracking and sorting submissions
        /// </summary>
        public DateTime UploadedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Full text content extracted from the resume
        /// Used for keyword matching and analysis
        /// </summary>
        public string? ExtractedText { get; set; }

        /// <summary>
        /// Current status of the resume analysis
        /// Helps track processing state in the workflow
        /// </summary>
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Processing, Completed, Failed

        /// <summary>
        /// Navigation property - the analysis result for this resume
        /// One-to-one relationship: each resume has one analysis
        /// </summary>
        public Analysis? Analysis { get; set; }
    }
}
