using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;

namespace ResumeAnalyzer.API.Services
{
    // Service for extracting text from PDFs and other document formats
    // Uses Azure AI Document Intelligence
    public class DocumentIntelligenceService
    {
        private readonly DocumentAnalysisClient _client;
        private readonly ILogger<DocumentIntelligenceService> _logger;

        public DocumentIntelligenceService(
            IConfiguration configuration,
            ILogger<DocumentIntelligenceService> logger)
        {
            _logger = logger;

            // Get Azure AI credentials from appsettings.json
            var endpoint = configuration["AzureAI:Endpoint"];
            var apiKey = configuration["AzureAI:ApiKey"];

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Azure AI credentials not configured. Service will operate in demo mode.");
                // In production this would throw an exception
                // For dev/testing, just use placeholders
            }

            _client = new DocumentAnalysisClient(
                new Uri(endpoint ?? "https://placeholder.cognitiveservices.azure.com/"),
                new AzureKeyCredential(apiKey ?? "placeholder-key"));
        }

        // Main method - extracts all text from a document file
        // Works with PDF, DOCX, images, etc.
        public async Task<string> ExtractTextFromDocumentAsync(Stream fileStream, string fileName)
        {
            try
            {
                _logger.LogInformation("Starting document analysis for file: {FileName}", fileName);

                // Start analyzing - using Azure's pre-built read model
                // WaitUntil.Completed means we wait for it to finish before continuing
                var operation = await _client.AnalyzeDocumentAsync(
                    WaitUntil.Completed,
                    "prebuilt-read",
                    fileStream);

                _logger.LogInformation("Document analysis completed for: {FileName}", fileName);

                var result = operation.Value;
                var extractedText = new System.Text.StringBuilder();

                // Go through each page and extract the text
                foreach (var page in result.Pages)
                {
                    _logger.LogDebug("Processing page {PageNumber} of {FileName}", page.PageNumber, fileName);

                    // Azure AI maintains reading order which is nice
                    foreach (var line in page.Lines)
                    {
                        extractedText.AppendLine(line.Content);
                    }

                    extractedText.AppendLine(); // Separate pages
                }

                var finalText = extractedText.ToString().Trim();
                _logger.LogInformation("Extracted {CharCount} characters from {FileName}", 
                    finalText.Length, fileName);

                return finalText;
            }
            catch (RequestFailedException ex)
            {
                // Azure-specific error (quota, bad key, etc.)
                _logger.LogError(ex, "Azure AI request failed for file: {FileName}. Status: {Status}, Error: {Error}",
                    fileName, ex.Status, ex.Message);
                throw new ApplicationException($"Failed to analyze document: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during document analysis for file: {FileName}", fileName);
                throw new ApplicationException("An unexpected error occurred during document analysis.", ex);
            }
        }

        // Placeholder for future feature - could extract structured data like emails, phones, dates
        // Would need a custom trained model for this
        public async Task<Dictionary<string, string>> ExtractStructuredDataAsync(Stream fileStream)
        {
            _logger.LogInformation("Structured data extraction called (not yet implemented)");
            
            var structuredData = new Dictionary<string, string>();

            // TODO: Train custom model to extract:
            // - Name, Email, Phone, LinkedIn
            // - Education (schools, degrees, dates)
            // - Work history (companies, titles, dates)
            // - Certifications

            return await Task.FromResult(structuredData);
        }

        // Check if file format is supported before processing
        public bool IsSupportedFileFormat(string fileName)
        {
            var supportedExtensions = new[] { ".pdf", ".docx", ".doc", ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".txt" };
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            
            var isSupported = supportedExtensions.Contains(extension);
            _logger.LogDebug("File format validation for {FileName}: {IsSupported}", fileName, isSupported);
            
            return isSupported;
        }
    }
}
