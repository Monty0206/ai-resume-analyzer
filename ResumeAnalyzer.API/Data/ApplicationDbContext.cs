using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.API.Models;

namespace ResumeAnalyzer.API.Data
{
    /// <summary>
    /// Database context for the Resume Analyzer application
    /// Manages all database operations and entity relationships
    /// Configured to work with SQL Server for production deployment
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Constructor accepting configuration options
        /// Options are injected by the dependency injection container
        /// </summary>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // --- DBSETS: Define database tables ---
        /// <summary>
        /// Resumes table - stores uploaded resume files and metadata
        /// </summary>
        public DbSet<Resume> Resumes { get; set; }

        /// <summary>
        /// Analyses table - stores AI analysis results
        /// </summary>
        public DbSet<Analysis> Analyses { get; set; }

        /// <summary>
        /// Skills table - stores detected skills from resumes
        /// </summary>
        public DbSet<Skill> Skills { get; set; }

        /// <summary>
        /// Recommendations table - stores improvement suggestions
        /// </summary>
        public DbSet<Recommendation> Recommendations { get; set; }

        /// <summary>
        /// Configures entity relationships and database schema
        /// Called by EF Core when building the model
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- RESUME ENTITY CONFIGURATION ---
            modelBuilder.Entity<Resume>(entity =>
            {
                // Set table name explicitly for clarity
                entity.ToTable("Resumes");

                // Configure primary key
                entity.HasKey(r => r.Id);

                // Configure indexes for common queries
                entity.HasIndex(r => r.UploadedDate)
                    .HasDatabaseName("IX_Resume_UploadedDate");
                
                entity.HasIndex(r => r.Status)
                    .HasDatabaseName("IX_Resume_Status");

                // Configure one-to-one relationship with Analysis
                entity.HasOne(r => r.Analysis)
                    .WithOne(a => a.Resume)
                    .HasForeignKey<Analysis>(a => a.ResumeId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete analysis when resume is deleted
            });

            // --- ANALYSIS ENTITY CONFIGURATION ---
            modelBuilder.Entity<Analysis>(entity =>
            {
                entity.ToTable("Analyses");

                entity.HasKey(a => a.Id);

                // Index for querying by resume
                entity.HasIndex(a => a.ResumeId)
                    .HasDatabaseName("IX_Analysis_ResumeId");

                // Index for date-based queries
                entity.HasIndex(a => a.AnalyzedDate)
                    .HasDatabaseName("IX_Analysis_AnalyzedDate");

                // Configure decimal precision for scores
                entity.Property(a => a.OverallScore)
                    .HasPrecision(5, 2); // 999.99 max

                entity.Property(a => a.AtsScore)
                    .HasPrecision(5, 2);

                entity.Property(a => a.CompletenessScore)
                    .HasPrecision(5, 2);

                entity.Property(a => a.KeywordScore)
                    .HasPrecision(5, 2);

                entity.Property(a => a.FormattingScore)
                    .HasPrecision(5, 2);

                // Configure one-to-many relationship with Skills
                entity.HasMany(a => a.DetectedSkills)
                    .WithOne(s => s.Analysis)
                    .HasForeignKey(s => s.AnalysisId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Configure one-to-many relationship with Recommendations
                entity.HasMany(a => a.Recommendations)
                    .WithOne(r => r.Analysis)
                    .HasForeignKey(r => r.AnalysisId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // --- SKILL ENTITY CONFIGURATION ---
            modelBuilder.Entity<Skill>(entity =>
            {
                entity.ToTable("Skills");

                entity.HasKey(s => s.Id);

                // Composite index for efficient filtering
                entity.HasIndex(s => new { s.AnalysisId, s.Category })
                    .HasDatabaseName("IX_Skill_AnalysisId_Category");

                // Index for in-demand skills queries
                entity.HasIndex(s => s.IsInDemand)
                    .HasDatabaseName("IX_Skill_IsInDemand");
            });

            // --- RECOMMENDATION ENTITY CONFIGURATION ---
            modelBuilder.Entity<Recommendation>(entity =>
            {
                entity.ToTable("Recommendations");

                entity.HasKey(r => r.Id);

                // Index for querying by analysis
                entity.HasIndex(r => r.AnalysisId)
                    .HasDatabaseName("IX_Recommendation_AnalysisId");

                // Index for priority-based queries
                entity.HasIndex(r => r.Priority)
                    .HasDatabaseName("IX_Recommendation_Priority");

                // Index for category filtering
                entity.HasIndex(r => r.Category)
                    .HasDatabaseName("IX_Recommendation_Category");
            });
        }
    }
}
