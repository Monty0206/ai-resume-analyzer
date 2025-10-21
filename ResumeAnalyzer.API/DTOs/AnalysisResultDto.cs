namespace ResumeAnalyzer.API.DTOs
{
    /// <summary>
    /// Complete analysis response sent to the client
    /// Aggregates all analysis data in a clean, structured format
    /// This is the main response object for the analyze endpoint
    /// </summary>
    public class AnalysisResultDto
    {
        /// <summary>
        /// Unique identifier for tracking this analysis
        /// </summary>
        public Guid AnalysisId { get; set; }

        /// <summary>
        /// ID of the analyzed resume
        /// </summary>
        public Guid ResumeId { get; set; }

        /// <summary>
        /// Original filename of the analyzed resume
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// When the analysis was completed
        /// </summary>
        public DateTime AnalyzedDate { get; set; }

        // --- SCORES ---
        /// <summary>
        /// Overall resume quality score (0-100)
        /// Weighted average of all subscores
        /// </summary>
        public decimal OverallScore { get; set; }

        /// <summary>
        /// ATS compatibility score (0-100)
        /// </summary>
        public decimal AtsScore { get; set; }

        /// <summary>
        /// Completeness score (0-100)
        /// </summary>
        public decimal CompletenessScore { get; set; }

        /// <summary>
        /// Keyword optimization score (0-100)
        /// </summary>
        public decimal KeywordScore { get; set; }

        /// <summary>
        /// Formatting quality score (0-100)
        /// </summary>
        public decimal FormattingScore { get; set; }

        // --- SUMMARIES ---
        /// <summary>
        /// AI-generated summary of resume strengths
        /// </summary>
        public string? StrengthsSummary { get; set; }

        /// <summary>
        /// AI-generated summary of weaknesses
        /// </summary>
        public string? WeaknessesSummary { get; set; }

        // --- DETAILS ---
        /// <summary>
        /// List of skills detected by AI
        /// </summary>
        public List<SkillDto> DetectedSkills { get; set; } = new();

        /// <summary>
        /// List of improvement recommendations
        /// Sorted by priority and impact
        /// </summary>
        public List<RecommendationDto> Recommendations { get; set; } = new();

        /// <summary>
        /// Raw extracted text from the resume
        /// Useful for debugging and detailed review
        /// </summary>
        public string? ExtractedText { get; set; }
    }
}
