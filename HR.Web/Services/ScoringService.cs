using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Web.Mvc;
using HR.Web.Data;
using HR.Web.Models;
using HR.Web.Services;
using Newtonsoft.Json;

namespace HR.Web.Services
{
    public class ScoringService
    {
        private readonly UnitOfWork _uow;
        private readonly MCPService _mcpService;

        public ScoringService()
        {
            _uow = new UnitOfWork();
            _mcpService = new MCPService();
        }

        /// <summary>
        /// Calculate total score for an application based on questionnaire responses
        /// </summary>
        public decimal CalculateApplicationScore(int applicationId)
        {
            var application = _uow.Applications.Get(applicationId);
            if (application == null) return 0;

            return CalculateApplicationScore(application);
        }

        /// <summary>
        /// Calculate total score for an application
        /// </summary>
        public decimal CalculateApplicationScore(Application application)
        {
            var score = 0m;

            // Get position questions and their order
            var positionQuestions = _uow.Context.Set<PositionQuestion>()
                .Where(pq => pq.PositionId == application.PositionId)
                .Include(pq => pq.Question)
                .OrderBy(pq => pq.Order)
                .ToList();

            // Get application answers
            var answers = _uow.Context.Set<ApplicationAnswer>()
                .Where(aa => aa.ApplicationId == application.Id)
                .ToList();

            foreach (var positionQuestion in positionQuestions)
            {
                var answer = answers.FirstOrDefault(a => a.QuestionId == positionQuestion.QuestionId);
                if (answer != null)
                {
                    score += CalculateQuestionScore(positionQuestion.Question, answer.AnswerText, application.PositionId);
                }
            }

            return score;
        }

        /// <summary>
        /// Calculate score for a single question
        /// </summary>
        public decimal CalculateQuestionScore(Question question, string answerText, int positionId)
        {
            if (string.IsNullOrEmpty(answerText)) return 0;

            switch (question.Type.ToLower())
            {
                case "choice":
                    return CalculateChoiceScore(question, answerText, positionId);
                case "rating":
                    return CalculateRatingScore(question, answerText);
                case "number":
                    return CalculateNumberScore(question, answerText);
                case "text":
                    return CalculateTextScore(question, answerText);
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Calculate score for choice questions
        /// </summary>
        private decimal CalculateChoiceScore(Question question, string answerText, int positionId)
        {
            // Get question options
            var options = _uow.Context.Set<HR.Web.Models.QuestionOption>()
                .Where(qo => qo.QuestionId == question.Id)
                .ToList();

            // Check for position-specific overrides
            var positionQuestion = _uow.Context.Set<PositionQuestion>()
                .FirstOrDefault(pq => pq.PositionId == positionId && pq.QuestionId == question.Id);

            if (positionQuestion != null)
            {
                var positionOptions = _uow.Context.Set<PositionQuestionOption>()
                    .Where(pqo => pqo.PositionQuestionId == positionQuestion.Id)
                    .Include(pqo => pqo.QuestionOption)
                    .ToList();

                // Use position-specific points if available
                var matchedOption = positionOptions.FirstOrDefault(pqo => 
                    pqo.QuestionOption.Text.Equals(answerText, StringComparison.OrdinalIgnoreCase));

                if (matchedOption != null && matchedOption.Points.HasValue)
                {
                    return matchedOption.Points.Value;
                }
            }

            // Fallback to default points
            var defaultOption = options.FirstOrDefault(o => 
                o.Text.Equals(answerText, StringComparison.OrdinalIgnoreCase));

            return defaultOption?.Points ?? 0;
        }

        /// <summary>
        /// Calculate score for rating questions
        /// </summary>
        private decimal CalculateRatingScore(Question question, string answerText)
        {
            if (int.TryParse(answerText, out int rating))
            {
                // Convert rating to points (1-5 scale -> 0-10 points)
                return Math.Max(0, Math.Min(10, rating * 2));
            }

            return 0;
        }

        /// <summary>
        /// Calculate score for number questions
        /// </summary>
        private decimal CalculateNumberScore(Question question, string answerText)
        {
            if (decimal.TryParse(answerText, out decimal number))
            {
                // For years of experience: 0-10 points based on experience level
                if (question.Text.ToLower().Contains("year") && question.Text.ToLower().Contains("experience"))
                {
                    return Math.Min(10, Math.Max(0, number * 2));
                }

                // For other numeric questions, use a simple scaling
                return Math.Min(10, Math.Max(0, number));
            }

            return 0;
        }

        /// <summary>
        /// Calculate score for text questions using AI analysis
        /// </summary>
        private decimal CalculateTextScore(Question question, string answerText)
        {
            // Basic scoring based on answer quality
            var score = 0m;

            // Length and completeness
            if (answerText.Length < 20)
            {
                score += 1;
            }
            else if (answerText.Length < 100)
            {
                score += 4;
            }
            else if (answerText.Length < 300)
            {
                score += 7;
            }
            else
            {
                score += 9;
            }

            // Keywords that indicate good answers
            var positiveKeywords = new[] { "experience", "developed", "implemented", "managed", "led", "created", "improved", "achieved" };
            var keywordCount = positiveKeywords.Count(keyword => 
                answerText.ToLower().Contains(keyword));

            score += Math.Min(2, keywordCount * 0.5m);

            // Check for specific examples
            if (answerText.ToLower().Contains("example") || answerText.ToLower().Contains("specific"))
            {
                score += 1;
            }

            return Math.Min(10, score);
        }

        /// <summary>
        /// Get maximum possible score for a position
        /// </summary>
        public decimal GetMaxScoreForPosition(int positionId)
        {
            var positionQuestions = _uow.Context.Set<PositionQuestion>()
                .Where(pq => pq.PositionId == positionId)
                .Include(pq => pq.Question)
                .ToList();

            var maxScore = 0m;

            foreach (var positionQuestion in positionQuestions)
            {
                maxScore += GetMaxScoreForQuestion(positionQuestion.Question, positionId);
            }

            return maxScore;
        }

        /// <summary>
        /// Get maximum possible score for a question
        /// </summary>
        public decimal GetMaxScoreForQuestion(Question question, int positionId)
        {
            switch (question.Type.ToLower())
            {
                case "choice":
                    return GetMaxChoiceScore(question, positionId);
                case "rating":
                    return 10; // 1-5 rating scale -> max 10 points
                case "number":
                    return 10; // Scaled to max 10 points
                case "text":
                    return 10; // Max 10 points for text answers
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Get maximum score for choice questions
        /// </summary>
        private decimal GetMaxChoiceScore(Question question, int positionId)
        {
            // Check for position-specific overrides
            var positionQuestion = _uow.Context.Set<PositionQuestion>()
                .FirstOrDefault(pq => pq.PositionId == positionId && pq.QuestionId == question.Id);

            if (positionQuestion != null)
            {
                var positionOptions = _uow.Context.Set<PositionQuestionOption>()
                    .Where(pqo => pqo.PositionQuestionId == positionQuestion.Id)
                    .ToList();

                if (positionOptions.Any())
                {
                    return positionOptions.Where(pqo => pqo.Points.HasValue)
                        .Max(pqo => pqo.Points.Value);
                }
            }

            // Fallback to default options
            var defaultOptions = _uow.Context.Set<HR.Web.Models.QuestionOption>()
                .Where(qo => qo.QuestionId == question.Id)
                .ToList();

            return defaultOptions.Any() ? defaultOptions.Max(o => o.Points) : 0;
        }

        /// <summary>
        /// Get score breakdown for an application
        /// </summary>
        public List<QuestionScoreBreakdown> GetScoreBreakdown(int applicationId)
        {
            var application = _uow.Applications.Get(applicationId);
            if (application == null) return new List<QuestionScoreBreakdown>();

            var breakdown = new List<QuestionScoreBreakdown>();

            var positionQuestions = _uow.Context.Set<PositionQuestion>()
                .Where(pq => pq.PositionId == application.PositionId)
                .Include(pq => pq.Question)
                .OrderBy(pq => pq.Order)
                .ToList();

            var answers = _uow.Context.Set<ApplicationAnswer>()
                .Where(aa => aa.ApplicationId == application.Id)
                .ToList();

            foreach (var positionQuestion in positionQuestions)
            {
                var answer = answers.FirstOrDefault(a => a.QuestionId == positionQuestion.QuestionId);
                var score = answer != null ? 
                    CalculateQuestionScore(positionQuestion.Question, answer.AnswerText, application.PositionId) : 0;
                var maxScore = GetMaxScoreForQuestion(positionQuestion.Question, application.PositionId);

                breakdown.Add(new QuestionScoreBreakdown
                {
                    QuestionId = positionQuestion.QuestionId,
                    QuestionText = positionQuestion.Question.Text,
                    QuestionType = positionQuestion.Question.Type,
                    Order = positionQuestion.Order,
                    Answer = answer?.AnswerText ?? "Not answered",
                    Score = score,
                    MaxScore = maxScore,
                    Percentage = maxScore > 0 ? (score / maxScore) * 100 : 0
                });
            }

            return breakdown;
        }

        /// <summary>
        /// Rank candidates for a position
        /// </summary>
        public List<CandidateRanking> RankCandidatesForPosition(int positionId)
        {
            var applications = _uow.Applications.GetAll(
                a => a.Applicant,
                a => a.Position
            ).Where(a => a.PositionId == positionId).ToList();

            var maxScore = GetMaxScoreForPosition(positionId);
            var rankings = new List<CandidateRanking>();

            foreach (var application in applications)
            {
                var score = CalculateApplicationScore(application);
                var breakdown = GetScoreBreakdown(application.Id);

                rankings.Add(new CandidateRanking
                {
                    ApplicationId = application.Id,
                    CandidateName = application.Applicant?.FullName ?? "Unknown",
                    CandidateEmail = application.Applicant?.Email ?? "",
                    TotalScore = score,
                    MaxScore = maxScore,
                    Percentage = maxScore > 0 ? (score / maxScore) * 100 : 0,
                    AppliedDate = application.AppliedOn,
                    Status = application.Status ?? "Pending",
                    ScoreBreakdown = breakdown
                });
            }

            return rankings.OrderByDescending(r => r.Percentage).ToList();
        }

        /// <summary>
        /// Analyze question performance
        /// </summary>
        public async Task<QuestionPerformanceAnalysis> AnalyzeQuestionPerformance(int questionId)
        {
            var question = _uow.Questions.Get(questionId);
            if (question == null) return null;

            // Get all answers for this question
            var answers = _uow.Context.Set<ApplicationAnswer>()
                .Where(aa => aa.QuestionId == questionId)
                .ToList();

            if (!answers.Any()) return null;

            // Calculate score distribution
            var scoreDistribution = new Dictionary<string, int>();
            var totalScore = 0m;

            foreach (var answer in answers)
            {
                var application = _uow.Applications.Get(answer.ApplicationId);
                if (application != null)
                {
                    var score = CalculateQuestionScore(question, answer.AnswerText, application.PositionId);
                    totalScore += score;
                    
                    var scoreRange = GetScoreRange(score);
                    int current = 0;
                    scoreDistribution.TryGetValue(scoreRange, out current);
                    scoreDistribution[scoreRange] = current + 1;
                }
            }

            var averageScore = answers.Count > 0 ? totalScore / answers.Count : 0;

            // Use MCP to get additional insights
            var mcpAnalysis = await GetMCPQuestionAnalysis(questionId, scoreDistribution, (double)averageScore, answers.Count);

            return new QuestionPerformanceAnalysis
            {
                QuestionId = questionId,
                QuestionText = question.Text,
                TotalResponses = answers.Count,
                AverageScore = averageScore,
                ScoreDistribution = scoreDistribution,
                MCPAnalysis = mcpAnalysis
            };
        }

        private async Task<object> GetMCPQuestionAnalysis(int questionId, Dictionary<string, int> distribution, double averageScore, int totalResponses)
        {
            try
            {
                var parameters = new
                {
                    questionId = questionId.ToString(),
                    responseDistribution = distribution,
                    averageScore = averageScore,
                    totalResponses = totalResponses
                };

                var response = await _mcpService.CallToolAsync("analyze-performance", parameters);
                
                if (response.Success)
                {
                    var content = response.Result.contents[0];
                    return JsonConvert.DeserializeObject<dynamic>(content.text);
                }
            }
            catch
            {
                // Fallback if MCP is not available
            }

            return null;
        }

        private string GetScoreRange(decimal score)
        {
            if (score <= 2) return "0-2";
            if (score <= 4) return "3-4";
            if (score <= 6) return "5-6";
            if (score <= 8) return "7-8";
            return "9-10";
        }
    }

    // Supporting classes
    public class QuestionScoreBreakdown
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }
        public int Order { get; set; }
        public string Answer { get; set; }
        public decimal Score { get; set; }
        public decimal MaxScore { get; set; }
        public decimal Percentage { get; set; }
    }

    public class CandidateRanking
    {
        public int ApplicationId { get; set; }
        public string CandidateName { get; set; }
        public string CandidateEmail { get; set; }
        public decimal TotalScore { get; set; }
        public decimal MaxScore { get; set; }
        public decimal Percentage { get; set; }
        public DateTime AppliedDate { get; set; }
        public string Status { get; set; }
        public List<QuestionScoreBreakdown> ScoreBreakdown { get; set; }
    }

    public class QuestionPerformanceAnalysis
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public int TotalResponses { get; set; }
        public decimal AverageScore { get; set; }
        public Dictionary<string, int> ScoreDistribution { get; set; }
        public dynamic MCPAnalysis { get; set; }
    }
}
