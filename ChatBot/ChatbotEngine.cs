using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CybersecurityChatbot
{
    public class ChatbotEngine
    {
        private List<TaskData> _tasks = new List<TaskData>();
        private List<ActivityLogEntry> _activityLog = new List<ActivityLogEntry>();
        private Dictionary<string, List<string>> _keywordResponses = LoadResponsesFromJson();
        private List<string> _positiveSentiments = new List<string> { "interested", "curious", "excited", "keen" };
        private List<string> _worriedSentiments = new List<string> { "worried", "scared", "nervous", "concerned", "anxious" };
        private List<string> _frustratedSentiments = new List<string> { "frustrated", "tired", "angry", "fed up" };
        private UserProfile _userProfile;
        
        public ChatbotEngine(UserProfile userProfile)
        {
            _userProfile = userProfile;
            LoadTasks();
        }
        
        private void LoadTasks()
        {
            try
            {
                _tasks = JsonDataService.LoadTasks();
                LogActivity($"Loaded {_tasks.Count} tasks from storage");
            }
            catch (Exception ex)
            {
                LogActivity($"Error loading tasks: {ex.Message}");
                _tasks = new List<TaskData>();
            }
        }
        
        private void SaveTasks()
        {
            try
            {
                JsonDataService.SaveTasks(_tasks);
            }
            catch (Exception ex)
            {
                LogActivity($"Error saving tasks: {ex.Message}");
            }
        }

        public string ProcessMessage(string input)
        {
            LogActivity($"User message: {input}");
            
            // Save chat message
            var response = ProcessMessageInternal(input);
            JsonDataService.SaveConversation(input, response, "chatbot");
            
            return response;
        }
        
        private string ProcessMessageInternal(string input)
        {
            // Handle specific commands
            if (input.Contains("add task") || input.Contains("create task"))
            {
                return "Please use the 'Add New Task' button on the right to create a new cybersecurity task with all the details and reminder options.";
            }
            
            if (input.Contains("show tasks") || input.Contains("list tasks") || input.Contains("view tasks"))
            {
                return "Click the 'View All Tasks' button on the right to see your current cybersecurity tasks and manage them.";
            }
            
            if (input.Contains("start quiz") || input.Contains("begin quiz") || input.Contains("take quiz"))
            {
                return "Ready to test your cybersecurity knowledge? Click the 'Start Quiz' button on the right to begin!";
            }
            
            if (input.Contains("activity log") || input.Contains("show activity") || input.Contains("show log"))
            {
                return "You can view your activity log by clicking the 'View Activity' button on the right side panel.";
            }
            
            if (input.Contains("help") || input.Contains("commands"))
            {
                return ShowHelp();
            }

            // Sentiment detection
            if (DetectSentiment(input, out string mood))
            {
                return RespondWithSentiment(mood);
            }

            // Check for favorite subject first
            if (_userProfile != null && !string.IsNullOrWhiteSpace(_userProfile.FavoriteSubject))
            {
                var favoriteSubject = _userProfile.FavoriteSubject.ToLower();
                if (input.ToLower().Contains(favoriteSubject) || 
                    (favoriteSubject == "2fa" && input.ToLower().Contains("two-factor")) ||
                    (favoriteSubject == "incident response" && input.ToLower().Contains("incident")))
                {
                    var favoriteResponse = _userProfile.GetFavoriteSubjectResponse();
                    if (!string.IsNullOrEmpty(favoriteResponse))
                    {
                        var tips = _keywordResponses.ContainsKey(favoriteSubject) ? _keywordResponses[favoriteSubject] : new List<string>();
                        if (tips.Count > 0)
                        {
                            var random = new Random();
                            string tip = tips[random.Next(tips.Count)];
                            LogActivity($"Provided favorite subject tip about: {favoriteSubject}");
                            return $"{favoriteResponse}\n\n💡 {tip}";
                        }
                        else
                        {
                            LogActivity($"Provided favorite subject response about: {favoriteSubject}");
                            return favoriteResponse;
                        }
                    }
                }
            }
            
            // Keyword recognition
            foreach (var keyword in _keywordResponses.Keys)
            {
                if (input.ToLower().Contains(keyword))
                {
                    var tips = _keywordResponses[keyword];
                    var random = new Random();
                    string tip = tips[random.Next(tips.Count)];
                    LogActivity($"Provided tip about: {keyword}");
                    
                    // Add personalized greeting for known users
                    if (_userProfile != null && !string.IsNullOrWhiteSpace(_userProfile.Name))
                    {
                        return $"💡 Here's some advice for you, {_userProfile.Name}: {tip}";
                    }
                    else
                    {
                        return $"💡 {tip}";
                    }
                }
            }

            // Default response with personalization
            if (_userProfile != null && !string.IsNullOrWhiteSpace(_userProfile.Name))
            {
                return $"I'm not sure about that, {_userProfile.Name}. Try asking about passwords, phishing, malware, or use the buttons on the right to access specific features!";
            }
            else
            {
                return "I'm not sure about that. Try asking about passwords, phishing, malware, or use the buttons on the right to access specific features!";
            }
        }

        public void AddTask(string title, string description, string reminderTime)
        {
            var task = new TaskData
            {
                Title = title,
                Description = description,
                ReminderText = reminderTime,
                CreatedDate = DateTime.Now,
                Status = "Pending",
                Priority = "Medium",
                Category = "Cybersecurity"
            };
            
            _tasks.Add(task);
            SaveTasks();
            LogActivity($"Added task: {title}");
        }

        public List<TaskData> GetTasks()
        {
            return _tasks.ToList();
        }

        public int GetTaskCount()
        {
            return _tasks.Count(t => t.Status != "Completed");
        }
        
        public int GetCompletedTaskCount()
        {
            return _tasks.Count(t => t.Status == "Completed");
        }
        
        public (int total, int pending, int completed) GetTaskStatistics()
        {
            int total = _tasks.Count;
            int completed = _tasks.Count(t => t.Status == "Completed");
            int pending = total - completed;
            return (total, pending, completed);
        }

        public void CompleteTask(TaskData task)
        {
            task.Status = "Completed";
            task.CompletedDate = DateTime.Now;
            SaveTasks();
            LogActivity($"Completed task: {task.Title}");
        }

        public void DeleteTask(TaskData task)
        {
            _tasks.Remove(task);
            SaveTasks();
            LogActivity($"Deleted task: {task.Title}");
        }

        public List<ActivityLogEntry> GetActivityLogs()
        {
            return _activityLog.TakeLast(50).ToList();
        }

        public void ClearActivityLog()
        {
            _activityLog.Clear();
            LogActivity("Activity log cleared");
        }

        private bool DetectSentiment(string input, out string sentiment)
        {
            foreach (var word in _worriedSentiments)
                if (input.Contains(word)) { sentiment = "worried"; return true; }
            foreach (var word in _frustratedSentiments)
                if (input.Contains(word)) { sentiment = "frustrated"; return true; }
            foreach (var word in _positiveSentiments)
                if (input.Contains(word)) { sentiment = "positive"; return true; }
            sentiment = "";
            return false;
        }

        private string RespondWithSentiment(string sentiment)
        {
            LogActivity($"Responded to {sentiment} sentiment");
            
            return sentiment switch
            {
                "worried" => "I understand you're feeling worried. Let's take cybersecurity step-by-step to make it less overwhelming. Would you like to start with a simple task or take the quiz to assess your current knowledge?",
                "frustrated" => "I'm sorry you're feeling frustrated. Cybersecurity can be complex, but I'm here to guide you through it. What specific area is troubling you? Try asking about passwords, phishing, or malware.",
                "positive" => "That's awesome! I love your enthusiasm for cybersecurity. Let's channel that energy into learning something new! Try the quiz or explore the task management features.",
                _ => "I'm here to help you with cybersecurity! What would you like to learn about today?"
            };
        }

        private string ShowHelp()
        {
            LogActivity("Viewed help information");
            return "🆘 Here's how I can help you:\n\n" +
                   "📝 TASK MANAGEMENT:\n" +
                   "• Use 'Add New Task' to create cybersecurity tasks\n" +
                   "• Use 'View All Tasks' to manage your tasks\n\n" +
                   "🎯 QUIZ SYSTEM:\n" +
                   "• Click 'Start Quiz' to test your knowledge\n\n" +
                   "📊 ACTIVITY TRACKING:\n" +
                   "• View your activity with 'View Activity'\n" +
                   "• Clear logs with 'Clear Log'\n\n" +
                   "🧠 SMART RESPONSES:\n" +
                   "• Ask about: passwords, phishing, malware, 2FA, VPN, etc.\n" +
                   "• Express feelings: worried, excited, frustrated, etc.\n\n" +
                   "Use the buttons on the right for easy access to all features!";
        }
        
        public void LogActivity(string action)
        {
            _activityLog.Add(new ActivityLogEntry
            {
                Action = action,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        private static Dictionary<string, List<string>> LoadResponsesFromJson()
        {
            try
            {
                var responsesFilePath = Path.Combine("Data", "responses.json");
                if (File.Exists(responsesFilePath))
                {
                    var jsonString = File.ReadAllText(responsesFilePath);
                    var responseData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<string>>>>(jsonString);
                    
                    if (responseData != null && responseData.ContainsKey("keywordResponses"))
                    {
                        return responseData["keywordResponses"];
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading responses: {ex.Message}");
            }
            
            // Fallback responses if JSON loading fails
            return GetFallbackResponses();
        }

        private static Dictionary<string, List<string>> GetFallbackResponses()
        {
            return new Dictionary<string, List<string>>
            {
                { "passwords", new List<string> { "🔐 Create strong passwords with 12+ characters using uppercase, lowercase, numbers, and symbols. Each account should have a unique password!" } },
                { "phishing", new List<string> { "🎣 Be cautious of suspicious emails! Never click links or download attachments from unknown senders. Always verify sender identity through a separate communication channel." } },
                { "malware", new List<string> { "🦠 Keep your antivirus updated and run regular full system scans. Windows Defender is good, but consider Malwarebytes for additional protection." } },
                { "2fa", new List<string> { "🔒 Enable two-factor authentication (2FA) on ALL important accounts - email, banking, social media. It's your best defense against account takeovers!" } }
            };
        }
    }
}
