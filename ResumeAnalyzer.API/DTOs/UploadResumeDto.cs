namespace ResumeAnalyzer.API.DTOs
{
    /// <summary>
    /// Data Transfer Object for resume upload requests
    /// Separates API contract from internal data model
    /// Validates and structures incoming resume data
    /// </summary>
    public class UploadResumeDto
    {
        /// <summary>
        /// The resume file uploaded by the user
        /// Accepted formats: PDF, DOCX, DOC, TXT
        /// Maximum size should be validated in controller (e.g., 5MB)
        /// </summary>
        public IFormFile? File { get; set; }

        /// <summary>
        /// Optional: Target job title or role for tailored analysis
        /// Helps AI provide more relevant recommendations
        /// Example: "Software Developer", "DevOps Engineer"
        /// </summary>
        public string? TargetRole { get; set; }

        /// <summary>
        /// Optional: Industry for context-specific analysis
        /// Examples: "Technology", "Healthcare", "Finance"
        /// Influences keyword recommendations
        /// </summary>
        public string? Industry { get; set; }
    }
}
