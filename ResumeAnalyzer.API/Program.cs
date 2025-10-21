using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.API.Data;
using ResumeAnalyzer.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add controllers so we can use API endpoints
builder.Services.AddControllers();

// Setup Swagger for testing the API - super helpful during development
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Resume Analyzer API",
        Version = "v1",
        Description = "AI-powered resume analysis service using Azure AI Document Intelligence",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Developer",
            Email = "your.email@example.com"
        }
    });

    // TODO: Add XML comments later if needed
    // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    // options.IncludeXmlComments(xmlPath);
});

// Database setup - using LocalDB for dev, will migrate to Azure SQL later
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // Added retry logic because connection drops were annoying during testing
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });
    
    // Only log sensitive stuff in dev - learned this the hard way
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Register my custom services
builder.Services.AddScoped<DocumentIntelligenceService>();
builder.Services.AddScoped<ResumeAnalysisService>();
builder.Services.AddScoped<AzureOpenAIService>(); // GPT-4 integration!

// CORS setup - had to add this because my HTML file couldn't connect at first
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:4200",
                "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
    
    // For dev, just allow everything - makes testing easier
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Limit file uploads to 10MB - don't want massive files breaking things
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024;
});

var app = builder.Build();

// Swagger UI is great for testing endpoints
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Resume Analyzer API V1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// Using AllowAll in dev so my local HTML file works
app.UseCors(app.Environment.IsDevelopment() ? "AllowAll" : "AllowFrontend");

app.UseRouting();

// Not using auth yet but keeping this here for later
app.UseAuthorization();

app.MapControllers();

// Simple root endpoint to show API is running
app.MapGet("/", () => Results.Ok(new
{
    service = "Resume Analyzer API",
    version = "1.0.0",
    status = "Running",
    documentation = "/swagger",
    endpoints = new[]
    {
        "POST /api/resume/analyze - Upload and analyze a resume",
        "GET /api/resume/{id} - Get analysis by ID",
        "GET /api/resume/list - List all analyses",
        "DELETE /api/resume/{id} - Delete a resume",
        "GET /api/resume/health - Health check"
    }
}));

// Auto-apply migrations on startup - saves me from running commands manually
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate();
            
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Database initialized successfully");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while initializing the database");
            // Keep going even if DB fails - can fix it later
        }
    }
}

app.Run();
