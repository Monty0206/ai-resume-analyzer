using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResumeAnalyzer.API.Models
{
    /// <summary>
    /// Represents a skill detected in the resume by AI analysis
    /// Skills are extracted and categorized to help candidates understand their profile
    /// Important for matching with job requirements
    /// </summary>
    public class Skill
    {
        /// <summary>
        /// Unique identifier for the skill entry
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Foreign key to the parent analysis
        /// Each skill belongs to one analysis result
        /// </summary>
        [Required]
        public Guid AnalysisId { get; set; }

        /// <summary>
        /// Navigation property to parent analysis
        /// Maintains the relationship hierarchy
        /// </summary>
        [ForeignKey(nameof(AnalysisId))]
        public Analysis? Analysis { get; set; }

        /// <summary>
        /// Name of the detected skill
        /// Examples: "C#", "Project Management", "Azure", "React"
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Category of the skill for better organization
        /// Examples: "Programming", "Cloud", "Soft Skills", "Tools"
        /// Helps group related skills together
        /// </summary>
        [MaxLength(50)]
        public string? Category { get; set; }

        /// <summary>
        /// Confidence level of the AI detection (0-100)
        /// Higher values mean the AI is more certain this skill is present
        /// Useful for filtering out false positives
        /// </summary>
        [Range(0, 100)]
        public int ConfidenceLevel { get; set; }

        /// <summary>
        /// How many times this skill appears in the resume
        /// Higher frequency may indicate stronger proficiency
        /// </summary>
        public int Frequency { get; set; } = 1;

        /// <summary>
        /// Whether this is considered an in-demand skill
        /// Based on current job market trends and requirements
        /// Helps prioritize skill development
        /// </summary>
        public bool IsInDemand { get; set; }
    }
}
