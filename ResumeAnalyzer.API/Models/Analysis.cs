using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResumeAnalyzer.API.Models
{
    /// <summary>
    /// Stores the AI-powered analysis results for a resume
    /// This model contains the core intelligence output from Azure AI
    /// Designed to provide actionable feedback to job seekers
    /// </summary>
    public class Analysis
    {
        /// <summary>
        /// Unique identifier for this analysis
        /// Each analysis is tied to exactly one resume
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Foreign key linking to the analyzed resume
        /// Maintains referential integrity in the database
        /// </summary>
        [Required]
        public Guid ResumeId { get; set; }

        /// <summary>
        /// Navigation property to the parent resume
        /// Enables easy access to resume details from analysis
        /// </summary>
        [ForeignKey(nameof(ResumeId))]
        public Resume? Resume { get; set; }

        /// <summary>
        /// Overall score out of 100
        /// Calculated based on completeness, keywords, formatting, and ATS compatibility
        /// Higher scores indicate better optimized resumes
        /// </summary>
        [Range(0, 100)]
        public decimal OverallScore { get; set; }

        /// <summary>
        /// Score for ATS (Applicant Tracking System) compatibility
        /// Measures how well the resume will perform in automated screening
        /// Critical for modern job applications
        /// </summary>
        [Range(0, 100)]
        public decimal AtsScore { get; set; }

        /// <summary>
        /// Completeness score - checks if all important sections are present
        /// Evaluates: contact info, summary, experience, education, skills
        /// </summary>
        [Range(0, 100)]
        public decimal CompletenessScore { get; set; }

        /// <summary>
        /// Keyword optimization score
        /// Measures alignment with industry-standard terms and job requirements
        /// </summary>
        [Range(0, 100)]
        public decimal KeywordScore { get; set; }

        /// <summary>
        /// Formatting and readability score
        /// Evaluates structure, consistency, and visual appeal
        /// </summary>
        [Range(0, 100)]
        public decimal FormattingScore { get; set; }

        /// <summary>
        /// AI-generated summary of resume strengths
        /// Highlights what the candidate does well
        /// </summary>
        public string? StrengthsSummary { get; set; }

        /// <summary>
        /// AI-generated summary of areas needing improvement
        /// Provides constructive feedback for resume enhancement
        /// </summary>
        public string? WeaknessesSummary { get; set; }

        /// <summary>
        /// When the analysis was completed
        /// Useful for tracking processing time and version history
        /// </summary>
        public DateTime AnalyzedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Collection of detected skills from the resume
        /// Helps candidates see what skills were recognized by AI
        /// </summary>
        public ICollection<Skill> DetectedSkills { get; set; } = new List<Skill>();

        /// <summary>
        /// Collection of improvement recommendations
        /// Actionable suggestions to enhance the resume
        /// </summary>
        public ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
    }
}
