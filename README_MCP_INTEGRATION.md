# HR Questionnaire System with MCP Integration

## Overview
This enhanced HR questionnaire system integrates Model Context Protocol (MCP) to provide AI-powered question generation, validation, and intelligent scoring capabilities.

## 🚀 Key Features Implemented

### 1. MCP Server (`mcp-server/`)
- **Question Bank Resources**: Pre-built technical and behavioral questions
- **Templates**: Ready-to-use questionnaires for different roles
- **AI Tools**: Question generation, validation, and point optimization
- **Scoring Rubrics**: Standardized evaluation criteria

### 2. Enhanced Admin Interface
- **AI-Powered Question Creation**: Generate questions from job descriptions
- **Real-time Validation**: Check for bias and question quality
- **Smart Point Suggestions**: AI-recommended scoring values
- **Template Import**: Quick setup with pre-built questionnaires

### 3. Position-Question Assignment
- **Drag-and-Drop Interface**: Easy question ordering
- **Visual Assignment**: Clear view of available vs assigned questions
- **Bulk Operations**: Efficient management of multiple positions

### 4. Advanced Scoring System
- **Multi-Type Scoring**: Support for text, choice, number, and rating questions
- **Position-Specific Points**: Different scoring for the same question across positions
- **Real-time Calculation**: Live score updates and candidate ranking
- **Performance Analytics**: Question effectiveness analysis

### 5. Questionnaire Testing & Preview
- **Interactive Preview**: See exactly how candidates will experience the questionnaire
- **Test Mode**: Complete the questionnaire and see scoring in action
- **Performance Insights**: Detailed breakdown of test results
- **Recommendations**: AI-powered suggestions for improvement

## 📁 Project Structure

```
HR/
├── mcp-server/                 # MCP Server
│   ├── package.json           # Node.js dependencies
│   └── server.js              # MCP server implementation
├── HR.Web/
│   ├── Controllers/
│   │   ├── AdminController.MCP.cs      # MCP-enhanced admin functions
│   │   ├── AdminController.Scoring.cs  # Advanced scoring system
│   │   └── QuestionnaireController.cs  # Preview & testing interface
│   ├── Services/
│   │   ├── MCPService.cs        # MCP integration layer
│   │   └── ScoringService.cs    # Enhanced scoring logic
│   └── Views/
│       ├── Admin/
│       │   ├── QuestionsWithMCP.cshtml      # AI-enhanced question management
│       │   ├── EditQuestionWithMCP.cshtml  # AI-powered question editor
│       │   ├── PositionQuestions.cshtml     # Question assignment UI
│       │   └── EnhancedCandidateRankings.cshtml # Advanced rankings
│       └── Questionnaire/
│           ├── Preview.cshtml    # Questionnaire preview
│           ├── Test.cshtml       # Interactive testing
│           └── TestResult.cshtml # Detailed test results
```

## 🛠 Setup Instructions

### 1. Install MCP Server
```bash
cd mcp-server
npm install
```

### 2. Start MCP Server
```bash
npm start
```

### 3. Update HR.Web Configuration
- Ensure Node.js is installed and accessible
- Update MCP server path in `MCPService.cs` if needed

### 4. Run HR Application
- Open in Visual Studio
- Build and run the project

## 🎯 Key Workflows

### Admin Question Management
1. Navigate to `/Admin/QuestionsWithMCP`
2. Use AI generation or import templates
3. Validate questions for bias and quality
4. Assign questions to positions via drag-and-drop
5. Preview and test questionnaires

### Candidate Evaluation
1. Applications automatically scored based on questionnaire responses
2. Real-time rankings updated in `/Admin/EnhancedCandidateRankings`
3. Detailed score breakdowns available for each candidate
4. Performance analytics for question optimization

### Questionnaire Testing
1. Access `/Questionnaire/Test/{positionId}`
2. Complete questionnaire as a test candidate
3. Receive immediate scoring and performance insights
4. Get AI-powered recommendations for improvement

## 🔧 MCP Tools Available

### generate-questions
Create relevant questions from job descriptions
```json
{
  "jobTitle": "Senior Software Engineer",
  "jobDescription": "Full job description...",
  "experience": "senior",
  "questionTypes": ["technical", "behavioral"],
  "count": 5
}
```

### validate-question
Check questions for bias and quality
```json
{
  "question": "Describe your experience...",
  "questionType": "Text",
  "options": [...]
}
```

### suggest-points
Get AI-recommended scoring values
```json
{
  "question": "How would you handle...",
  "options": ["Option 1", "Option 2", "Option 3"],
  "difficulty": "intermediate"
}
```

### import-template
Load pre-built questionnaires
```json
{
  "templateType": "senior-developer",
  "customize": true
}
```

### analyze-performance
Get insights on question effectiveness
```json
{
  "questionId": "tech_001",
  "responseDistribution": {...},
  "averageScore": 7.2,
  "totalResponses": 45
}
```

## 🎨 UI Features

### Enhanced Question Editor
- AI generation panel with job description input
- Real-time question validation
- Smart point suggestions
- Template import functionality

### Position Question Assignment
- Visual drag-and-drop interface
- Question preview with scoring
- Bulk assignment operations
- Order management

### Advanced Rankings Dashboard
- Real-time score updates
- Visual progress indicators
- Detailed score breakdowns
- Export functionality

### Interactive Testing Interface
- Progress tracking
- Auto-save functionality
- Immediate scoring feedback
- Performance analytics

## 📊 Scoring System

### Question Types Supported
- **Text**: AI-powered content analysis
- **Choice**: Point-based selection
- **Number**: Scaled scoring
- **Rating**: 1-5 scale conversion

### Position-Specific Scoring
- Default points from question options
- Position-specific overrides
- Flexible weighting systems
- Real-time calculation

### Performance Analytics
- Question effectiveness metrics
- Response distribution analysis
- AI-powered optimization suggestions
- Cross-position comparisons

## 🔍 Benefits of MCP Integration

### For Administrators
- **Reduced Setup Time**: AI-generated questions and templates
- **Improved Quality**: Bias detection and validation
- **Better Insights**: Performance analytics and recommendations
- **Efficient Management**: Bulk operations and smart suggestions

### For Candidates
- **Better Experience**: Clear, well-structured questions
- **Fair Evaluation**: Bias-free and validated questions
- **Immediate Feedback**: Real-time scoring in test mode

### For Organizations
- **Consistency**: Standardized question banks
- **Compliance**: Bias detection and validation
- **Analytics**: Data-driven question optimization
- **Scalability**: Easy template-based setup

## 🚀 Next Steps

1. **Deploy MCP Server**: Set up production MCP instance
2. **Customize Question Banks**: Add industry-specific questions
3. **Configure Scoring**: Fine-tune point values and weights
4. **Train Administrators**: Ensure proper use of AI features
5. **Monitor Performance**: Use analytics to continuously improve

This enhanced system transforms your HR questionnaire process from manual management to an AI-powered, intelligent system that saves time, improves quality, and provides valuable insights for better hiring decisions.
