namespace ResumeAnalyzer.API.DTOs
{
    /// <summary>
    /// Simplified recommendation data for API responses
    /// Provides clear, actionable improvement suggestions
    /// Optimized for frontend display
    /// </summary>
    public class RecommendationDto
    {
        /// <summary>
        /// Short title summarizing the recommendation
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Detailed explanation of the recommendation
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Category: Content, Formatting, Keywords, ATS Optimization
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Priority: High, Medium, Low
        /// </summary>
        public string Priority { get; set; } = string.Empty;

        /// <summary>
        /// Potential impact on score (0-100)
        /// </summary>
        public int ImpactScore { get; set; }

        /// <summary>
        /// Step-by-step action plan
        /// </summary>
        public string? ActionSteps { get; set; }

        /// <summary>
        /// Example or template for reference
        /// </summary>
        public string? Example { get; set; }
    }
}
