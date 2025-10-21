using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResumeAnalyzer.API.Models
{
    /// <summary>
    /// Represents an AI-generated recommendation for resume improvement
    /// These actionable suggestions help job seekers optimize their resumes
    /// Prioritized by importance and impact on ATS performance
    /// </summary>
    public class Recommendation
    {
        /// <summary>
        /// Unique identifier for this recommendation
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Foreign key to the parent analysis
        /// Links recommendation to specific resume analysis
        /// </summary>
        [Required]
        public Guid AnalysisId { get; set; }

        /// <summary>
        /// Navigation property to parent analysis
        /// Enables querying recommendations with their analysis
        /// </summary>
        [ForeignKey(nameof(AnalysisId))]
        public Analysis? Analysis { get; set; }

        /// <summary>
        /// Brief title of the recommendation
        /// Examples: "Add Missing Skills Section", "Improve Work Experience Formatting"
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Detailed description of what needs improvement
        /// Provides context and explains why this matters
        /// </summary>
        [Required]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Category of the recommendation
        /// Examples: "Content", "Formatting", "Keywords", "ATS Optimization"
        /// Helps organize feedback by type
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Priority level: High, Medium, Low
        /// Indicates urgency and impact of addressing this issue
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Priority { get; set; } = "Medium";

        /// <summary>
        /// Estimated impact on overall score if implemented
        /// Helps users prioritize which changes to make first
        /// </summary>
        [Range(0, 100)]
        public int ImpactScore { get; set; }

        /// <summary>
        /// Specific actionable steps to implement the recommendation
        /// Example: "1. Create a Skills section\n2. List 8-12 relevant skills\n3. Include both technical and soft skills"
        /// </summary>
        public string? ActionSteps { get; set; }

        /// <summary>
        /// Example or template showing the recommended approach
        /// Provides concrete guidance for implementation
        /// </summary>
        public string? Example { get; set; }
    }
}
