using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CybersecurityChatbot
{
    public class JsonDataService
    {
        private static string QuestionsFileName => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "quiz_questions.json");
        private const string ResponsesFileName = "Data/quiz_responses.json";
        private const string ConversationFileName = "Data/conversation_history.json";
        private static string DataDirectory => "Data";

        public class QuestionData
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = "";
            
            [JsonPropertyName("options")]
            public List<string> Options { get; set; } = new List<string>();
            
            [JsonPropertyName("correctAnswerIndex")]
            public int CorrectAnswerIndex { get; set; }
            
            [JsonPropertyName("explanation")]
            public string Explanation { get; set; } = "";
            
            [JsonPropertyName("category")]
            public string Category { get; set; } = "";
        }

        public class QuizResponse
        {
            public DateTime Timestamp { get; set; }
            public List<UserAnswer> Answers { get; set; } = new List<UserAnswer>();
            public int Score { get; set; }
            public int TotalQuestions { get; set; }
            public double Percentage { get; set; }
            public TimeSpan TimeTaken { get; set; }
        }

        public class UserAnswer
        {
            public string Question { get; set; } = "";
            public string UserResponse { get; set; } = "";
            public string CorrectAnswer { get; set; } = "";
            public bool IsCorrect { get; set; }
            public string Category { get; set; } = "";
        }

        public class ConversationEntry
        {
            public DateTime Timestamp { get; set; }
            public string UserMessage { get; set; } = "";
            public string BotResponse { get; set; } = "";
            public string ResponseType { get; set; } = ""; // "chatbot", "gpt", "command"
        }

        public static List<QuizQuestion> LoadQuestions()
        {
            try
            {
                var questions = new List<QuizQuestion>();
                
                if (File.Exists(QuestionsFileName))
                {
                    var jsonString = File.ReadAllText(QuestionsFileName);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var questionData = JsonSerializer.Deserialize<Dictionary<string, List<QuestionData>>>(jsonString, options);
                    
                    if (questionData != null && questionData.ContainsKey("questions"))
                    {
                        foreach (var q in questionData["questions"])
                        {
                            questions.Add(new QuizQuestion
                            {
                                Text = q.Text,
                                Options = q.Options,
                                CorrectAnswerIndex = q.CorrectAnswerIndex,
                                Explanation = q.Explanation,
                                Category = q.Category
                            });
                        }
                    }
                }
                
                return questions.Count > 0 ? questions : QuizQuestions.GetFallbackQuestions();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading questions: {ex.Message}");
                return QuizQuestions.GetFallbackQuestions();
            }
        }

        public static void SaveQuizResponse(QuizResult result)
        {
            try
            {
                EnsureDataDirectory();
                
                var response = new QuizResponse
                {
                    Timestamp = DateTime.Now,
                    Score = result.Score,
                    TotalQuestions = result.TotalQuestions,
                    Percentage = result.Percentage,
                    TimeTaken = result.TimeTaken,
                    Answers = new List<UserAnswer>()
                };

                foreach (var questionResult in result.QuestionResults)
                {
                    response.Answers.Add(new UserAnswer
                    {
                        Question = questionResult.Question.Text,
                        UserResponse = questionResult.Question.Options[questionResult.UserAnswerIndex],
                        CorrectAnswer = questionResult.Question.Options[questionResult.Question.CorrectAnswerIndex],
                        IsCorrect = questionResult.IsCorrect,
                        Category = questionResult.Question.Category
                    });
                }

                var responses = LoadQuizResponses();
                responses.Add(response);

                var jsonString = JsonSerializer.Serialize(responses, new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    Converters = { new JsonStringEnumConverter() }
                });
                
                File.WriteAllText(ResponsesFileName, jsonString);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving quiz response: {ex.Message}");
            }
        }

        public static List<QuizResponse> LoadQuizResponses()
        {
            try
            {
                if (File.Exists(ResponsesFileName))
                {
                    var jsonString = File.ReadAllText(ResponsesFileName);
                    return JsonSerializer.Deserialize<List<QuizResponse>>(jsonString) ?? new List<QuizResponse>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading quiz responses: {ex.Message}");
            }
            
            return new List<QuizResponse>();
        }

        public static void SaveConversation(string userMessage, string botResponse, string responseType = "chatbot")
        {
            try
            {
                EnsureDataDirectory();
                
                var conversations = LoadConversationHistory();
                conversations.Add(new ConversationEntry
                {
                    Timestamp = DateTime.Now,
                    UserMessage = userMessage,
                    BotResponse = botResponse,
                    ResponseType = responseType
                });

                // Keep only last 100 conversations to prevent file from growing too large
                if (conversations.Count > 100)
                {
                    conversations = conversations.GetRange(conversations.Count - 100, 100);
                }

                var jsonString = JsonSerializer.Serialize(conversations, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                File.WriteAllText(ConversationFileName, jsonString);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving conversation: {ex.Message}");
            }
        }

        public static List<ConversationEntry> LoadConversationHistory()
        {
            try
            {
                if (File.Exists(ConversationFileName))
                {
                    var jsonString = File.ReadAllText(ConversationFileName);
                    return JsonSerializer.Deserialize<List<ConversationEntry>>(jsonString) ?? new List<ConversationEntry>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading conversation history: {ex.Message}");
            }
            
            return new List<ConversationEntry>();
        }

        private static void EnsureDataDirectory()
        {
            var dataDir = Path.GetDirectoryName(QuestionsFileName);
            if (!string.IsNullOrEmpty(dataDir) && !Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
        }
        
        private static void EnsureDataDirectoryExists()
        {
            if (!Directory.Exists(DataDirectory))
            {
                Directory.CreateDirectory(DataDirectory);
            }
        }


        public static QuizStatistics GetQuizStatistics()
        {
            var responses = LoadQuizResponses();
            var stats = new QuizStatistics();

            if (responses.Count > 0)
            {
                stats.TotalQuizzesTaken = responses.Count;
                stats.AverageScore = responses.Average(r => r.Percentage);
                stats.BestScore = responses.Max(r => r.Percentage);
                stats.LastTaken = responses.Max(r => r.Timestamp);
                
                // Category performance
                var categoryPerformance = new Dictionary<string, List<double>>();
                foreach (var response in responses)
                {
                    foreach (var answer in response.Answers)
                    {
                        if (!categoryPerformance.ContainsKey(answer.Category))
                            categoryPerformance[answer.Category] = new List<double>();
                        
                        categoryPerformance[answer.Category].Add(answer.IsCorrect ? 100.0 : 0.0);
                    }
                }

                stats.CategoryPerformance = categoryPerformance.ToDictionary(
                    kvp => kvp.Key, 
                    kvp => kvp.Value.Average()
                );
            }

            return stats;
        }
        
        public static void SaveTasks(List<TaskData> tasks)
        {
            try
            {
                EnsureDataDirectoryExists();
                var tasksFilePath = Path.Combine(DataDirectory, "tasks.json");
                
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                var jsonString = JsonSerializer.Serialize(tasks, options);
                File.WriteAllText(tasksFilePath, jsonString);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save tasks: {ex.Message}", ex);
            }
        }
        
        public static List<TaskData> LoadTasks()
        {
            try
            {
                var tasksFilePath = Path.Combine(DataDirectory, "tasks.json");
                
                if (!File.Exists(tasksFilePath))
                {
                    return new List<TaskData>();
                }
                
                var jsonString = File.ReadAllText(tasksFilePath);
                
                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    return new List<TaskData>();
                }
                
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                var tasks = JsonSerializer.Deserialize<List<TaskData>>(jsonString, options);
                return tasks ?? new List<TaskData>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load tasks: {ex.Message}", ex);
            }
        }
    }

    public class QuizStatistics
    {
        public int TotalQuizzesTaken { get; set; }
        public double AverageScore { get; set; }
        public double BestScore { get; set; }
        public DateTime LastTaken { get; set; }
        public Dictionary<string, double> CategoryPerformance { get; set; } = new Dictionary<string, double>();
    }
}
