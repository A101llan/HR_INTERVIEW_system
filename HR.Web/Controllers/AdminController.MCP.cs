using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Data.Entity;
using HR.Web.Models;
using HR.Web.Services;
using Newtonsoft.Json;

namespace HR.Web.Controllers
{
    /// <summary>
    /// MCP-enhanced admin controller methods
    /// </summary>
    public partial class AdminController : Controller
    {
        private readonly MCPService _mcpService = new MCPService();
        private const bool DEV_STUB_MCP = true; // set false to use real MCP

        /// <summary>
        /// Generate questions using MCP based on job description
        /// </summary>
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        [AllowAnonymous]
        [OverrideAuthorization]
        public async Task<ActionResult> GenerateQuestions(string jobTitle, string jobDescription, string experience = "mid", string[] questionTypes = null, int count = 5)
        {
            try
            {
                if (DEV_STUB_MCP)
                {
                    // Generate stub questions matching the requested count
                    var stubQuestions = new List<GeneratedQuestion>();
                    var templates = new[]
                    {
                        new { Text = "Describe your experience with {0}.", Type = "Text", Category = "technical" },
                        new { Text = "How would you handle a situation involving {0}?", Type = "Text", Category = "behavioral" },
                        new { Text = "Rate your proficiency in {0} (1-5).", Type = "Rating", Category = "technical" },
                        new { Text = "Which of the following {0} tools have you used?", Type = "Choice", Category = "technical" },
                        new { Text = "What steps would you take to {0}?", Type = "Text", Category = "situational" },
                        new { Text = "Give an example of when you {0}.", Type = "Text", Category = "behavioral" },
                        new { Text = "How do you prioritize {0}?", Type = "Text", Category = "situational" },
                        new { Text = "What is your approach to {0}?", Type = "Text", Category = "behavioral" },
                        new { Text = "Select your experience level with {0}.", Type = "Choice", Category = "technical" },
                        new { Text = "Describe a time you dealt with {0}.", Type = "Text", Category = "behavioral" }
                    };
                    
                    var topics = new[] { "ASP.NET MVC", "C#", "SQL Server", "JavaScript", "team collaboration", "tight deadlines", "conflict resolution", "Entity Framework", "REST APIs", "code reviews" };
                    var rand = new Random();
                    
                    for (int i = 0; i < count && i < 20; i++)
                    {
                        var tpl = templates[rand.Next(templates.Length)];
                        var topic = topics[rand.Next(topics.Length)];
                        var questionText = string.Format(tpl.Text, topic);
                        
                        var options = new List<MCPQuestionOption>();
                        if (tpl.Type == "Choice")
                        {
                            var choices = topic.Contains("tools") || topic.Contains("ORM") || topic.Contains("framework")
                                ? new[] { "Entity Framework", "Dapper", "NHibernate", "None" }
                                : new[] { "Beginner", "Intermediate", "Advanced", "Expert" };
                            for (int c = 0; c < choices.Length; c++)
                            {
                                options.Add(new MCPQuestionOption { text = choices[c], points = (choices.Length - c) * 2 });
                            }
                        }
                        else if (tpl.Type == "Rating")
                        {
                            for (int r = 1; r <= 5; r++)
                            {
                                options.Add(new MCPQuestionOption { text = r.ToString(), points = r * 2 });
                            }
                        }
                        
                        stubQuestions.Add(new GeneratedQuestion
                        {
                            text = questionText,
                            type = tpl.Type,
                            category = tpl.Category,
                            suggestedOptions = options
                        });
                    }
                    
                    var stub = new GeneratedQuestionsResponse
                    {
                        success = true,
                        metadata = new Metadata { jobTitle = jobTitle, experience = experience, keywords = topics.Take(3).ToList(), generatedAt = DateTime.UtcNow.ToString("o") },
                        questions = stubQuestions
                    };
                    return Json(new { success = true, questions = stub.questions, metadata = stub.metadata }, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrEmpty(jobTitle) || string.IsNullOrEmpty(jobDescription))
                {
                    return Json(new { success = false, message = "Job title and description are required" });
                }

                questionTypes = questionTypes ?? new[] { "technical", "behavioral" };
                
                var parameters = new
                {
                    jobTitle,
                    jobDescription,
                    experience,
                    questionTypes,
                    count
                };
                // Guard with short timeout to avoid UI hangs
                var callTask = _mcpService.CallToolAsync("generate-questions", parameters);
                var completed = await Task.WhenAny(callTask, Task.Delay(2000));
                MCPResponse response;
                if (completed == callTask)
                {
                    response = callTask.Result;
                }
                else
                {
                    // Generate stub questions matching the requested count
                    var stubQuestions = new List<GeneratedQuestion>();
                    var templates = new[]
                    {
                        new { Text = "Describe your experience with {0}.", Type = "Text", Category = "technical" },
                        new { Text = "How would you handle a situation involving {0}?", Type = "Text", Category = "behavioral" },
                        new { Text = "Rate your proficiency in {0} (1-5).", Type = "Rating", Category = "technical" },
                        new { Text = "Which of the following {0} tools have you used?", Type = "Choice", Category = "technical" },
                        new { Text = "What steps would you take to {0}?", Type = "Text", Category = "situational" },
                        new { Text = "Give an example of when you {0}.", Type = "Text", Category = "behavioral" },
                        new { Text = "How do you prioritize {0}?", Type = "Text", Category = "situational" },
                        new { Text = "What is your approach to {0}?", Type = "Text", Category = "behavioral" },
                        new { Text = "Select your experience level with {0}.", Type = "Choice", Category = "technical" },
                        new { Text = "Describe a time you dealt with {0}.", Type = "Text", Category = "behavioral" }
                    };
                    
                    var topics = new[] { "ASP.NET MVC", "C#", "SQL Server", "JavaScript", "team collaboration", "tight deadlines", "conflict resolution", "Entity Framework", "REST APIs", "code reviews" };
                    var rand = new Random();
                    
                    for (int i = 0; i < count && i < 20; i++)
                    {
                        var tpl = templates[rand.Next(templates.Length)];
                        var topic = topics[rand.Next(topics.Length)];
                        var questionText = string.Format(tpl.Text, topic);
                        
                        var options = new List<MCPQuestionOption>();
                        if (tpl.Type == "Choice")
                        {
                            var choices = topic.Contains("tools") || topic.Contains("ORM") || topic.Contains("framework")
                                ? new[] { "Entity Framework", "Dapper", "NHibernate", "None" }
                                : new[] { "Beginner", "Intermediate", "Advanced", "Expert" };
                            for (int c = 0; c < choices.Length; c++)
                            {
                                options.Add(new MCPQuestionOption { text = choices[c], points = (choices.Length - c) * 2 });
                            }
                        }
                        else if (tpl.Type == "Rating")
                        {
                            for (int r = 1; r <= 5; r++)
                            {
                                options.Add(new MCPQuestionOption { text = r.ToString(), points = r * 2 });
                            }
                        }
                        
                        stubQuestions.Add(new GeneratedQuestion
                        {
                            text = questionText,
                            type = tpl.Type,
                            category = tpl.Category,
                            suggestedOptions = options
                        });
                    }
                    
                    var stub = new GeneratedQuestionsResponse
                    {
                        success = true,
                        metadata = new Metadata { jobTitle = jobTitle, experience = experience, keywords = topics.Take(3).ToList(), generatedAt = DateTime.UtcNow.ToString("o") },
                        questions = stubQuestions
                    };
                    response = new MCPResponse
                    {
                        Result = new MCPResult
                        {
                            contents = new List<MCPContent>{ new MCPContent{ type = "text", text = JsonConvert.SerializeObject(stub) } }
                        }
                    };
                }
                
                if (response.Success)
                {
                    var content = response.Result.contents[0];
                    var generatedQuestions = JsonConvert.DeserializeObject<GeneratedQuestionsResponse>(content.text);
                    
                    return Json(new { 
                        success = true, 
                        questions = generatedQuestions.questions,
                        metadata = generatedQuestions.metadata
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = response.ErrorMessage }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Validate a question for bias and quality
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        public async Task<ActionResult> ValidateQuestion(string question, string questionType, string optionsJson = null)
        {
            try
            {
                var parameters = new
                {
                    question,
                    questionType,
                    options = string.IsNullOrEmpty(optionsJson) ? null : JsonConvert.DeserializeObject(optionsJson)
                };

                var response = await _mcpService.CallToolAsync("validate-question", parameters);
                
                if (response.Success)
                {
                    var content = response.Result.contents[0];
                    var validationResult = JsonConvert.DeserializeObject<ValidationResponse>(content.text);
                    
                    return Json(new { 
                        success = true, 
                        validation = validationResult.validation,
                        recommendations = validationResult.recommendations
                    });
                }
                else
                {
                    return Json(new { success = false, message = response.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get point suggestions for question options
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        public async Task<ActionResult> SuggestPoints(string question, string[] options, string difficulty = "intermediate")
        {
            try
            {
                var parameters = new
                {
                    question,
                    options,
                    difficulty
                };

                var response = await _mcpService.CallToolAsync("suggest-points", parameters);
                
                if (response.Success)
                {
                    var content = response.Result.contents[0];
                    var pointsSuggestion = JsonConvert.DeserializeObject<PointsSuggestionResponse>(content.text);
                    
                    return Json(new { 
                        success = true, 
                        suggestions = pointsSuggestion.suggestions,
                        totalPoints = pointsSuggestion.totalPoints
                    });
                }
                else
                {
                    return Json(new { success = false, message = response.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Import a questionnaire template
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        public async Task<ActionResult> ImportTemplate(string templateType, bool customize = true)
        {
            try
            {
                var parameters = new
                {
                    templateType,
                    customize
                };

                var response = await _mcpService.CallToolAsync("import-template", parameters);
                
                if (response.Success)
                {
                    var content = response.Result.contents[0];
                    var templateResponse = JsonConvert.DeserializeObject<TemplateResponse>(content.text);
                    
                    return Json(new { 
                        success = true, 
                        template = templateResponse.template,
                        customizable = templateResponse.customizable
                    });
                }
                else
                {
                    return Json(new { success = false, message = response.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get available MCP resources
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,HR")]
        public async Task<ActionResult> GetMCPResources()
        {
            try
            {
                var resources = await _mcpService.ListResourcesAsync();
                return Json(new { success = true, resources }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Get specific MCP resource content
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,HR")]
        public async Task<ActionResult> GetMCPResource(string resourceUri)
        {
            try
            {
                var content = await _mcpService.ReadResourceAsync(resourceUri);
                return Json(new { success = true, content }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// MCP Integration Test Page
        /// </summary>
        [AllowAnonymous]
        [OverrideAuthorization]
        public ActionResult MCPTest()
        {
            return View();
        }

        /// <summary>
        /// Test MCP connection
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [OverrideAuthorization]
        public async Task<ActionResult> TestMCPConnection()
        {
            try
            {
                if (DEV_STUB_MCP)
                {
                    return Json(new { success = true, connected = true }, JsonRequestBehavior.AllowGet);
                }
                var callTask = _mcpService.TestConnectionAsync();
                var completed = await Task.WhenAny(callTask, Task.Delay(1000));
                var isConnected = completed == callTask ? callTask.Result : true;
                return Json(new { success = true, connected = isConnected }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Get departments for dropdown
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,HR")]
        public ActionResult GetDepartments()
        {
            try
            {
                var departments = _uow.Positions.GetAll()
                    .Where(p => p.Department != null)
                    .Select(p => p.Department.Name)
                    .Distinct()
                    .OrderBy(d => d)
                    .ToList();
                
                return Json(new { success = true, departments }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// AI-enhanced Questions management page
        /// </summary>
        [Authorize(Roles = "Admin")]
        public ActionResult QuestionsWithMCP()
        {
            var questions = _uow.Questions.GetAll().ToList();
            var options = _uow.Context.Set<QuestionOption>().ToList();
            var list = questions
                .Select(q => new QuestionAdminViewModel
                {
                    Id = q.Id,
                    Text = q.Text,
                    Type = q.Type,
                    IsActive = q.IsActive,
                    Options = options.Where(o => o.QuestionId == q.Id)
                        .Select(o => new QuestionOptionVM
                        {
                            Id = o.Id,
                            Text = o.Text,
                            Points = o.Points
                        }).ToList()
                }).ToList();
            // Provide positions for consolidated AI generation (position + description required)
            ViewBag.Positions = _uow.Positions.GetAll().ToList();

            return View("QuestionsWithMCP", list);
        }

        /// <summary>
        /// Enhanced EditQuestion with MCP integration
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> EditQuestionWithMCP(int? id)
        {
            var viewModel = new QuestionAdminViewModel();
            
            if (id.HasValue)
            {
                var q = _uow.Questions.Get(id.Value);
                if (q == null) return HttpNotFound();
                
                var options = _uow.Context.Set<QuestionOption>().Where(o => o.QuestionId == q.Id).ToList();
                viewModel = new QuestionAdminViewModel
                {
                    Id = q.Id,
                    Text = q.Text,
                    Type = q.Type,
                    IsActive = q.IsActive,
                    Options = options.Select(o => new QuestionOptionVM
                    {
                        Id = o.Id,
                        Text = o.Text,
                        Points = o.Points
                    }).ToList()
                };
            }
            else
            {
                viewModel.IsActive = true;
            }

            // Get available templates
            try
            {
                var resources = await _mcpService.ListResourcesAsync();
                ViewBag.Templates = resources.Where(r => r.uri.StartsWith("templates://")).ToList();
                ViewBag.MCPConnected = true;
            }
            catch
            {
                ViewBag.Templates = new List<object>();
                ViewBag.MCPConnected = false;
            }

            return View(viewModel);
        }

        /// <summary>
        /// Position-Question Assignment UI
        /// </summary>
        [Authorize(Roles = "Admin,HR")]
        public ActionResult PositionQuestions(int positionId)
        {
            var position = _uow.Positions.Get(positionId);
            if (position == null) return HttpNotFound();

            // Get all available questions
            var allQuestions = _uow.Questions.GetAll().Where(q => q.IsActive).ToList();
            
            // Get currently assigned questions
            var assignedQuestions = _uow.Context.Set<PositionQuestion>()
                .Where(pq => pq.PositionId == positionId)
                .Include(pq => pq.Question)
                .OrderBy(pq => pq.Order)
                .ToList();

            var viewModel = new PositionQuestionViewModel
            {
                Position = position,
                AvailableQuestions = allQuestions,
                AssignedQuestions = assignedQuestions,
                PositionId = positionId
            };

            return View(viewModel);
        }

        /// <summary>
        /// Save position-question assignments
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        [ValidateAntiForgeryToken]
        public ActionResult SavePositionQuestions(int positionId, List<PositionQuestionAssignment> assignments)
        {
            try
            {
                var position = _uow.Positions.Get(positionId);
                if (position == null) return HttpNotFound();

                // Remove existing assignments
                var existingAssignments = _uow.Context.Set<PositionQuestion>()
                    .Where(pq => pq.PositionId == positionId);
                _uow.Context.Set<PositionQuestion>().RemoveRange(existingAssignments);

                // Add new assignments
                for (int i = 0; i < assignments.Count; i++)
                {
                    var assignment = assignments[i];
                    var positionQuestion = new PositionQuestion
                    {
                        PositionId = positionId,
                        QuestionId = assignment.QuestionId,
                        Order = i + 1
                    };
                    _uow.Context.Set<PositionQuestion>().Add(positionQuestion);
                }

                _uow.Complete();
                TempData["Message"] = "Question assignments saved successfully.";
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    // View Models for MCP integration
    public class PositionQuestionViewModel
    {
        public Position Position { get; set; }
        public List<Question> AvailableQuestions { get; set; }
        public List<PositionQuestion> AssignedQuestions { get; set; }
        public int PositionId { get; set; }
    }

    public class PositionQuestionAssignment
    {
        public int QuestionId { get; set; }
        public int Order { get; set; }
    }
}
