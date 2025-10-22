# AI Resume Analyzer 🚀

An intelligent resume analysis application powered by **Azure AI Document Intelligence** and **ASP.NET Core**. This tool helps job seekers optimize their resumes for Applicant Tracking Systems (ATS) and provides actionable recommendations to improve their chances of landing interviews.

## 🎯 Project Overview

The AI Resume Analyzer extracts text from uploaded resumes, performs comprehensive analysis using AI algorithms, and provides detailed feedback including:

- **Overall Quality Score** (0-100)
- **ATS Compatibility Score** - How well your resume works with applicant tracking systems
- **Completeness Score** - Checks for all essential resume sections
- **Keyword Optimization Score** - Evaluates relevant technical skills and industry terms
- **Formatting Quality Score** - Assesses structure and readability

## ✨ Features

### Core Functionality
- 📄 **Multi-Format Support**: Upload PDF, DOCX, DOC, or TXT files
- 🤖 **AI-Powered Analysis**: Leverages Azure AI Document Intelligence for text extraction
- 📊 **Comprehensive Scoring**: Multiple metrics to evaluate resume quality
- 💡 **Smart Recommendations**: Prioritized, actionable improvement suggestions
- 🔍 **Skill Detection**: Automatically identifies technical skills with confidence levels
- 🎯 **In-Demand Skills Highlighting**: Flags skills that are currently sought-after in the job market

### Technical Highlights
- RESTful API built with ASP.NET Core 9.0
- Entity Framework Core with SQL Server
- Azure AI Document Intelligence integration
- Swagger/OpenAPI documentation
- CORS-enabled for frontend integration
- Comprehensive logging and error handling

## 🏗️ Architecture

### Tech Stack
- **Backend**: ASP.NET Core 9.0 Web API
- **Database**: SQL Server (LocalDB for development)
- **Cloud AI**: Azure AI Document Intelligence
- **ORM**: Entity Framework Core 9.0
- **API Documentation**: Swagger/Swashbuckle

### Project Structure
```
ResumeAnalyzer.API/
├── Controllers/          # API endpoints (ResumeController)
├── Models/              # Database entities (Resume, Analysis, Skill, Recommendation)
├── DTOs/                # Data Transfer Objects for API responses
├── Services/            # Business logic and AI services
│   ├── DocumentIntelligenceService.cs
│   └── ResumeAnalysisService.cs
├── Data/                # Database context and configuration
├── Program.cs           # Application startup and configuration
└── appsettings.json     # Configuration settings
```

## 🚀 Getting Started

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server or SQL Server LocalDB
- Azure subscription (for Azure AI Document Intelligence)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/ai-resume-analyzer.git
   cd ResumeAnalyzer
   ```

2. **Configure Azure AI Document Intelligence**
   - Create an Azure AI Document Intelligence resource in the [Azure Portal](https://portal.azure.com)
   - Copy the endpoint and API key
   - Update `appsettings.json`:
     ```json
     "AzureAI": {
       "Endpoint": "https://your-resource.cognitiveservices.azure.com/",
       "ApiKey": "your-api-key-here"
     }
     ```

3. **Configure Database Connection**
   - Update the connection string in `appsettings.json` if needed:
     ```json
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ResumeAnalyzerDb;Trusted_Connection=true;"
     }
     ```

4. **Apply Database Migrations**
   ```bash
   cd ResumeAnalyzer.API
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

5. **Build and Run**
   ```bash
   dotnet build
   dotnet run
   ```

6. **Access the API**
   - API: `https://localhost:5001`
   - Swagger UI: `https://localhost:5001/swagger`

## 📡 API Endpoints

### Main Endpoints

#### `POST /api/resume/analyze`
Upload and analyze a resume file.

**Request**: Multipart form-data with file upload
```json
{
  "file": "resume.pdf",
  "targetRole": "DevOps Engineer" (optional),
  "industry": "Technology" (optional)
}
```

**Response**: Complete analysis results
```json
{
  "analysisId": "guid",
  "resumeId": "guid",
  "fileName": "resume.pdf",
  "overallScore": 85.5,
  "atsScore": 90.0,
  "completeness Score": 95.0,
  "keywordScore": 75.0,
  "formattingScore": 82.0,
  "strengthsSummary": "...",
  "weaknessesSummary": "...",
  "detectedSkills": [...],
  "recommendations": [...]
}
```

#### `GET /api/resume/{id}`
Retrieve a previously analyzed resume by ID.

#### `GET /api/resume/list`
List all analyzed resumes (latest 50).

#### `DELETE /api/resume/{id}`
Delete a resume and its analysis.

#### `GET /api/resume/health`
Health check endpoint.

## 🧠 How It Works

### 1. Document Upload
User uploads a resume file through the API endpoint.

### 2. Text Extraction
Azure AI Document Intelligence extracts text content while preserving document structure.

### 3. AI Analysis
The resume analysis service evaluates:
- **ATS Compatibility**: Checks for ATS-friendly formatting
- **Completeness**: Verifies all essential sections exist
- **Keywords**: Analyzes technical skills and action verbs
- **Formatting**: Evaluates structure and readability

### 4. Skill Detection
Regex-based skill detection identifies technical skills with:
- Confidence levels (based on frequency)
- Categorization (Cloud, DevOps, Programming, etc.)
- In-demand status flagging

### 5. Recommendations
AI generates prioritized recommendations with:
- Impact scores
- Priority levels (High/Medium/Low)
- Actionable steps
- Examples

## 💾 Database Schema

### Key Entities
- **Resume**: Stores uploaded file and metadata
- **Analysis**: Contains all analysis scores and summaries
- **Skill**: Detected skills with confidence and categories
- **Recommendation**: Improvement suggestions with priority

### Relationships
- Resume ↔ Analysis (One-to-One)
- Analysis ↔ Skills (One-to-Many)
- Analysis ↔ Recommendations (One-to-Many)

## 🔒 Security Considerations

- API keys stored in configuration files (use Azure Key Vault in production)
- File size limits enforced (5MB max)
- File type validation
- SQL injection prevention through parameterized queries
- HTTPS enforced in production

## 📈 Future Enhancements

- [ ] User authentication and authorization
- [ ] Resume comparison feature
- [ ] Job description matching
- [ ] Custom industry-specific analysis
- [ ] AI-powered resume rewriting suggestions
- [ ] PDF report generation
- [ ] Email notifications
- [ ] Integration with job boards
- [ ] React/Angular frontend

## 🤝 Contributing

Contributions are welcome! Please follow these steps:
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 👨‍💻 Author

**Montell Boks**
- Portfolio: [https://calm-dune-0801d2110.1.azurestaticapps.net/](https://calm-dune-0801d2110.1.azurestaticapps.net/)
- GitHub: [@Monty0206](https://github.com/Monty0206)
- LinkedIn: [Your LinkedIn](https://linkedin.com/in/yourprofile)

## 🙏 Acknowledgments

- **Azure AI Document Intelligence**: For powerful document processing capabilities
- **ASP.NET Core Team**: For the excellent web framework
- **Entity Framework Core**: For seamless database operations
- **Swagger**: For comprehensive API documentation

---

**Note**: This project was developed as part of a portfolio to demonstrate full-stack development skills, cloud integration, and AI-powered applications for DevOps and Cloud Computing roles.

## 📞 Support

For questions or support, please open an issue in the GitHub repository or contact the author directly.

---

**Built with ❤️ for job seekers worldwide**
