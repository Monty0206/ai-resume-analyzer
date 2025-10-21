using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.API.Data;
using ResumeAnalyzer.API.DTOs;
using ResumeAnalyzer.API.Models;
using ResumeAnalyzer.API.Services;

namespace ResumeAnalyzer.API.Controllers
{
    // Main controller for handling all resume operations
    [ApiController]
    [Route("api/[controller]")]
    public class ResumeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly DocumentIntelligenceService _documentService;
        private readonly ResumeAnalysisService _analysisService;
        private readonly ILogger<ResumeController> _logger;

        public ResumeController(
            ApplicationDbContext context,
            DocumentIntelligenceService documentService,
            ResumeAnalysisService analysisService,
            ILogger<ResumeController> logger)
        {
            _context = context;
            _documentService = documentService;
            _analysisService = analysisService;
            _logger = logger;
        }

        // Simple health check to make sure API is up
        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }

        // Main endpoint - this is where the magic happens
        [HttpPost("analyze")]
        [ProducesResponseType(typeof(AnalysisResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AnalysisResultDto>> AnalyzeResume([FromForm] UploadResumeDto request)
        {
            try
            {
                // Check if file was actually uploaded
                if (request.File == null || request.File.Length == 0)
                {
                    _logger.LogWarning("Upload attempt with no file");
                    return BadRequest(new { error = "No file uploaded. Please select a resume file." });
                }

                // Don't accept huge files - 5MB is plenty for a resume
                const long maxFileSize = 5 * 1024 * 1024;
                if (request.File.Length > maxFileSize)
                {
                    _logger.LogWarning("File too large: {Size} bytes", request.File.Length);
                    return BadRequest(new { error = "File size exceeds 5MB limit. Please upload a smaller file." });
                }

                // Make sure it's a supported format
                if (!_documentService.IsSupportedFileFormat(request.File.FileName))
                {
                    _logger.LogWarning("Unsupported file format: {FileName}", request.File.FileName);
                    return BadRequest(new { error = "Unsupported file format. Please upload PDF, DOCX, or TXT files." });
                }

                _logger.LogInformation("Processing resume upload: {FileName}, Size: {Size} bytes", 
                    request.File.FileName, request.File.Length);

                // Create a new resume record
                var resume = new Resume
                {
                    Id = Guid.NewGuid(),
                    FileName = request.File.FileName,
                    FileType = Path.GetExtension(request.File.FileName).TrimStart('.'),
                    FileSize = request.File.Length,
                    UploadedDate = DateTime.UtcNow,
                    Status = "Processing"
                };

                // Save the file content to database
                using (var memoryStream = new MemoryStream())
                {
                    await request.File.CopyToAsync(memoryStream);
                    resume.FileContent = memoryStream.ToArray();
                }

                _context.Resumes.Add(resume);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Resume saved to database: {ResumeId}", resume.Id);

                // Extract text using Azure AI - this is the cool part
                string extractedText;
                using (var stream = new MemoryStream(resume.FileContent))
                {
                    extractedText = await _documentService.ExtractTextFromDocumentAsync(
                        stream, 
                        resume.FileName);
                }

                resume.ExtractedText = extractedText;
                _logger.LogInformation("Text extracted successfully: {CharCount} characters", extractedText.Length);

                // Run the analysis
                var analysis = await _analysisService.AnalyzeResumeAsync(extractedText, resume.FileName);
                
                analysis.ResumeId = resume.Id;
                resume.Analysis = analysis;
                resume.Status = "Completed";

                _context.Analyses.Add(analysis);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Analysis completed and saved: {AnalysisId}, Score: {Score}", 
                    analysis.Id, analysis.OverallScore);

                // Send back the results
                var result = MapToDto(resume, analysis);

                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Application error during resume analysis");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during resume analysis");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "An unexpected error occurred. Please try again later." });
            }
        }

        // Get a previous analysis by ID
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AnalysisResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AnalysisResultDto>> GetAnalysis(Guid id)
        {
            try
            {
                _logger.LogInformation("Retrieving analysis for resume: {ResumeId}", id);

                // Load everything at once - Include is great for this
                var resume = await _context.Resumes
                    .Include(r => r.Analysis)
                        .ThenInclude(a => a!.DetectedSkills)
                    .Include(r => r.Analysis)
                        .ThenInclude(a => a!.Recommendations)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (resume == null || resume.Analysis == null)
                {
                    _logger.LogWarning("Resume not found: {ResumeId}", id);
                    return NotFound(new { error = "Resume analysis not found." });
                }

                var result = MapToDto(resume, resume.Analysis);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving analysis for resume: {ResumeId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "An error occurred while retrieving the analysis." });
            }
        }

        // List all resumes - useful for building a history view
        [HttpGet("list")]
        [ProducesResponseType(typeof(IEnumerable<AnalysisResultDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AnalysisResultDto>>> ListAnalyses()
        {
            try
            {
                _logger.LogInformation("Retrieving all resume analyses");

                var resumes = await _context.Resumes
                    .Include(r => r.Analysis)
                        .ThenInclude(a => a!.DetectedSkills)
                    .Include(r => r.Analysis)
                        .ThenInclude(a => a!.Recommendations)
                    .Where(r => r.Analysis != null)
                    .OrderByDescending(r => r.UploadedDate)
                    .Take(50) // Don't return everything if there's tons of data
                    .ToListAsync();

                var results = resumes
                    .Where(r => r.Analysis != null)
                    .Select(r => MapToDto(r, r.Analysis!))
                    .ToList();

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving resume list");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "An error occurred while retrieving the resume list." });
            }
        }

        // Delete endpoint in case user wants to remove their data
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteResume(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting resume: {ResumeId}", id);

                var resume = await _context.Resumes
                    .Include(r => r.Analysis)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (resume == null)
                {
                    _logger.LogWarning("Resume not found for deletion: {ResumeId}", id);
                    return NotFound(new { error = "Resume not found." });
                }

                _context.Resumes.Remove(resume);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Resume deleted successfully: {ResumeId}", id);
                return Ok(new { message = "Resume deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting resume: {ResumeId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "An error occurred while deleting the resume." });
            }
        }

        // Helper method to convert database models to DTOs
        private AnalysisResultDto MapToDto(Resume resume, Analysis analysis)
        {
            return new AnalysisResultDto
            {
                AnalysisId = analysis.Id,
                ResumeId = resume.Id,
                FileName = resume.FileName,
                AnalyzedDate = analysis.AnalyzedDate,
                
                OverallScore = analysis.OverallScore,
                AtsScore = analysis.AtsScore,
                CompletenessScore = analysis.CompletenessScore,
                KeywordScore = analysis.KeywordScore,
                FormattingScore = analysis.FormattingScore,
                
                StrengthsSummary = analysis.StrengthsSummary,
                WeaknessesSummary = analysis.WeaknessesSummary,
                
                ExtractedText = resume.ExtractedText,
                
                DetectedSkills = analysis.DetectedSkills.Select(s => new SkillDto
                {
                    Name = s.Name,
                    Category = s.Category,
                    ConfidenceLevel = s.ConfidenceLevel,
                    Frequency = s.Frequency,
                    IsInDemand = s.IsInDemand
                }).ToList(),
                
                Recommendations = analysis.Recommendations.Select(r => new RecommendationDto
                {
                    Title = r.Title,
                    Description = r.Description,
                    Category = r.Category,
                    Priority = r.Priority,
                    ImpactScore = r.ImpactScore,
                    ActionSteps = r.ActionSteps,
                    Example = r.Example
                }).ToList()
            };
        }

        // Job matching feature - compares resume against job posting
        [HttpPost("{id}/match-job")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MatchJobDescription(Guid id, [FromBody] JobMatchRequest request)
        {
            try
            {
                _logger.LogInformation("Matching job description for resume {ResumeId}", id);

                var resume = await _context.Resumes
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (resume == null)
                    return NotFound(new { error = "Resume not found" });

                if (string.IsNullOrEmpty(request.JobDescription))
                    return BadRequest(new { error = "Job description is required" });

                // Use GPT-4 to calculate how well the resume matches
                var openAIService = HttpContext.RequestServices.GetRequiredService<AzureOpenAIService>();
                var (matchScore, matchingKeywords, missingKeywords) = await openAIService.CalculateJobMatchAsync(
                    resume.ExtractedText ?? "",
                    request.JobDescription);

                return Ok(new
                {
                    resumeId = id,
                    matchScore = matchScore,
                    matchingKeywords = matchingKeywords,
                    missingKeywords = missingKeywords,
                    recommendation = matchScore >= 80 ? "Excellent match! Apply with confidence." :
                                    matchScore >= 60 ? "Good match. Consider adding missing keywords." :
                                    "Significant gaps. Update resume to match requirements."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error matching job description");
                return StatusCode(500, new { error = "Failed to match job description" });
            }
        }

        // AI chat endpoint - lets users ask questions about their resume
        [HttpPost("{id}/chat")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChatWithExpert(Guid id, [FromBody] ChatRequest request)
        {
            try
            {
                _logger.LogInformation("Chat question for resume {ResumeId}: {Question}", id, request.Question);

                var resume = await _context.Resumes
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (resume == null)
                    return NotFound(new { error = "Resume not found" });

                if (string.IsNullOrEmpty(request.Question))
                    return BadRequest(new { error = "Question is required" });

                // GPT-4 answers based on the resume content
                var openAIService = HttpContext.RequestServices.GetRequiredService<AzureOpenAIService>();
                var answer = await openAIService.ChatWithExpertAsync(
                    request.Question,
                    resume.ExtractedText ?? "");

                return Ok(new
                {
                    question = request.Question,
                    answer = answer,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in chat");
                return StatusCode(500, new { error = "Failed to process chat question" });
            }
        }

        // Rewriter feature - takes bland text and makes it sound professional
        [HttpPost("rewrite")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RewriteSection([FromBody] RewriteRequest request)
        {
            try
            {
                _logger.LogInformation("Rewriting section: {Type}", request.SectionType);

                if (string.IsNullOrEmpty(request.OriginalText))
                    return BadRequest(new { error = "Original text is required" });

                // GPT-4 rewrites it with better impact
                var openAIService = HttpContext.RequestServices.GetRequiredService<AzureOpenAIService>();
                var rewrittenText = await openAIService.RewriteResumeSectionAsync(
                    request.OriginalText,
                    request.SectionType ?? "experience");

                return Ok(new
                {
                    original = request.OriginalText,
                    rewritten = rewrittenText,
                    sectionType = request.SectionType
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rewriting section");
                return StatusCode(500, new { error = "Failed to rewrite section" });
            }
        }
    }

    // Request models for the new endpoints
    public class JobMatchRequest
    {
        public string JobDescription { get; set; } = string.Empty;
    }

    public class ChatRequest
    {
        public string Question { get; set; } = string.Empty;
    }

    public class RewriteRequest
    {
        public string OriginalText { get; set; } = string.Empty;
        public string? SectionType { get; set; }
    }
}
