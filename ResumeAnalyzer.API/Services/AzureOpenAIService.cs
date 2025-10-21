using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace ResumeAnalyzer.API.Services
{
    // GPT-4 integration for intelligent resume insights
    public class AzureOpenAIService
    {
        private readonly AzureOpenAIClient? _client;
        private readonly ILogger<AzureOpenAIService> _logger;
        private readonly string _deploymentName;

        public AzureOpenAIService(
            IConfiguration configuration,
            ILogger<AzureOpenAIService> logger)
        {
            _logger = logger;

            var endpoint = configuration["AzureOpenAI:Endpoint"];
            var apiKey = configuration["AzureOpenAI:ApiKey"];
            _deploymentName = configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4";

            // If config is missing, the service will just fall back to basic recommendations
            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Azure OpenAI not configured. AI recommendations will use fallback mode.");
                return;
            }

            _client = new AzureOpenAIClient(
                new Uri(endpoint),
                new AzureKeyCredential(apiKey));

            _logger.LogInformation("Azure OpenAI Service initialized with deployment: {Deployment}", _deploymentName);
        }

        // Generate personalized recommendations using GPT-4
        // This is what makes the analyzer way better than basic rule-based systems
        public async Task<string> GeneratePersonalizedRecommendationsAsync(
            string resumeText,
            decimal overallScore,
            string targetRole = "")
        {
            if (_client == null)
            {
                return GenerateFallbackRecommendations(overallScore);
            }

            try
            {
                var chatClient = _client.GetChatClient(_deploymentName);

                // System prompt sets the "personality" of GPT-4
                var systemPrompt = @"You are an expert career coach and resume consultant with 15+ years of experience. 
You specialize in helping candidates optimize their resumes for ATS systems and recruiters.
Provide specific, actionable advice in a friendly but professional tone.
Focus on concrete improvements that will increase interview chances.";

                var userPrompt = $@"Analyze this resume and provide 3-5 high-impact recommendations for improvement.

Resume Content:
{resumeText}

Current Overall Score: {overallScore}/100
{(string.IsNullOrEmpty(targetRole) ? "" : $"Target Role: {targetRole}")}

Provide recommendations in this format:
1. **[Title]**: [Specific actionable advice]
2. **[Title]**: [Specific actionable advice]
...

Focus on:
- ATS optimization
- Keyword gaps
- Achievement quantification
- Professional language
- Structure improvements";

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(systemPrompt),
                    new UserChatMessage(userPrompt)
                };

                var response = await chatClient.CompleteChatAsync(messages, new ChatCompletionOptions
                {
                    MaxOutputTokenCount = 800,
                    Temperature = 0.7f // Some creativity but not too random
                });

                var recommendation = response.Value.Content[0].Text;
                
                _logger.LogInformation("Generated GPT-4 recommendations: {Length} characters", recommendation.Length);
                
                return recommendation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI recommendations");
                return GenerateFallbackRecommendations(overallScore);
            }
        }

        // Rewrite resume sections to sound more professional and impactful
        public async Task<string> RewriteResumeSectionAsync(string originalText, string sectionType = "experience")
        {
            if (_client == null || string.IsNullOrWhiteSpace(originalText))
            {
                return originalText;
            }

            try
            {
                var chatClient = _client.GetChatClient(_deploymentName);

                var systemPrompt = @"You are an expert resume writer. Rewrite resume content to be more impactful, 
using strong action verbs, quantified achievements, and professional language. 
Keep the same factual information but make it shine.";

                var userPrompt = $@"Rewrite this {sectionType} section to be more powerful and ATS-friendly:

{originalText}

Requirements:
- Start with strong action verbs
- Quantify achievements where possible
- Use industry keywords
- Keep it concise (2-3 bullet points max)
- Professional tone";

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(systemPrompt),
                    new UserChatMessage(userPrompt)
                };

                var response = await chatClient.CompleteChatAsync(messages, new ChatCompletionOptions
                {
                    MaxOutputTokenCount = 300,
                    Temperature = 0.6f // Slightly more conservative for rewriting
                });

                return response.Value.Content[0].Text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rewriting section");
                return originalText;
            }
        }

        // Interactive chat for answering specific questions
        public async Task<string> ChatWithExpertAsync(string question, string resumeContext = "")
        {
            if (_client == null)
            {
                return "AI Chat Expert is currently unavailable. Please configure Azure OpenAI to enable this feature.";
            }

            try
            {
                var chatClient = _client.GetChatClient(_deploymentName);

                var systemPrompt = @"You are a helpful resume expert assistant. Answer questions about resume writing, 
job applications, career advice, and professional development. Be specific and actionable.
If resume context is provided, tailor advice to that specific resume.";

                // Include resume context if available, but truncate to avoid token limits
                var contextMessage = string.IsNullOrEmpty(resumeContext) 
                    ? "" 
                    : $"\n\nResume Context:\n{resumeContext.Substring(0, Math.Min(1000, resumeContext.Length))}";

                var userPrompt = $"{question}{contextMessage}";

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(systemPrompt),
                    new UserChatMessage(userPrompt)
                };

                var response = await chatClient.CompleteChatAsync(messages, new ChatCompletionOptions
                {
                    MaxOutputTokenCount = 500,
                    Temperature = 0.7f
                });

                return response.Value.Content[0].Text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in chat with expert");
                return "I apologize, but I'm having trouble processing your question right now. Please try again.";
            }
        }

        // Calculate how well a resume matches a job description
        // Returns score, matching keywords, and what's missing
        public async Task<(decimal matchScore, List<string> matchingKeywords, List<string> missingKeywords)> 
            CalculateJobMatchAsync(string resumeText, string jobDescription)
        {
            if (_client == null)
            {
                return (0, new List<string>(), new List<string>());
            }

            try
            {
                var chatClient = _client.GetChatClient(_deploymentName);

                var systemPrompt = @"You are an ATS system analyzer. Compare a resume against a job description 
and identify matching vs missing keywords. Be precise and technical.";

                // Truncate resume if too long to avoid token limits
                var userPrompt = $@"Compare this resume against the job description:

JOB DESCRIPTION:
{jobDescription}

RESUME:
{resumeText.Substring(0, Math.Min(2000, resumeText.Length))}

Respond in JSON format:
{{
    ""matchScore"": 85,
    ""matchingKeywords"": [""Python"", ""Azure"", ""CI/CD""],
    ""missingKeywords"": [""Kubernetes"", ""Terraform"", ""Docker""]
}}";

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(systemPrompt),
                    new UserChatMessage(userPrompt)
                };

                var response = await chatClient.CompleteChatAsync(messages, new ChatCompletionOptions
                {
                    MaxOutputTokenCount = 400,
                    Temperature = 0.3f, // Low temperature for more consistent results
                    ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
                });

                var jsonResponse = response.Value.Content[0].Text;
                var matchData = System.Text.Json.JsonSerializer.Deserialize<JobMatchResponse>(jsonResponse);

                return (matchData?.matchScore ?? 0, 
                        matchData?.matchingKeywords ?? new List<string>(), 
                        matchData?.missingKeywords ?? new List<string>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating job match");
                return (0, new List<string>(), new List<string>());
            }
        }

        // Fallback recommendations when GPT-4 isn't available
        private string GenerateFallbackRecommendations(decimal score)
        {
            if (score >= 80)
            {
                return @"1. **Polish Your Top Skills**: Your resume is strong! Focus on highlighting your most in-demand skills prominently.
2. **Add Quantifiable Achievements**: Include specific metrics and numbers to demonstrate your impact.
3. **Optimize for ATS**: Ensure all keywords from your target job descriptions are present.";
            }
            else if (score >= 60)
            {
                return @"1. **Strengthen Your Experience Section**: Add more detail about your accomplishments and responsibilities.
2. **Improve Keyword Density**: Research common terms in your industry and incorporate them naturally.
3. **Format for ATS Compatibility**: Remove complex tables or graphics that might confuse automated systems.";
            }
            else
            {
                return @"1. **Complete All Required Sections**: Ensure you have contact info, summary, experience, education, and skills.
2. **Use Action Verbs**: Start bullet points with strong verbs like 'Developed', 'Led', 'Implemented'.
3. **Add More Detail**: Expand on your experience with specific examples and achievements.";
            }
        }

        // Response model for deserializing GPT-4's JSON response
        private class JobMatchResponse
        {
            public decimal matchScore { get; set; }
            public List<string> matchingKeywords { get; set; } = new();
            public List<string> missingKeywords { get; set; } = new();
        }
    }
}
