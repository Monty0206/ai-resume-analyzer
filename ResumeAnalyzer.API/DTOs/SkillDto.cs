namespace ResumeAnalyzer.API.DTOs
{
    /// <summary>
    /// Simplified skill data for API responses
    /// Contains only the essential information clients need
    /// Reduces payload size and improves performance
    /// </summary>
    public class SkillDto
    {
        /// <summary>
        /// Name of the detected skill
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Category grouping for the skill
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// AI confidence in detecting this skill (0-100)
        /// </summary>
        public int ConfidenceLevel { get; set; }

        /// <summary>
        /// Number of times the skill appears
        /// </summary>
        public int Frequency { get; set; }

        /// <summary>
        /// Whether this skill is currently in high demand
        /// </summary>
        public bool IsInDemand { get; set; }
    }
}
