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
            var inputLower = input.ToLower();
            
            // Log NLP interaction
            LogActivity($"NLP processing: '{input}'");
            
            // Enhanced NLP for direct task creation with timing
            if (ContainsAdvancedTaskCreation(inputLower))
            {
                LogActivity("NLP: Recognized advanced task creation intent");
                return HandleAdvancedTaskCreation(input);
            }
            
            // Enhanced NLP for basic task management
            if (ContainsKeywords(inputLower, new[] {"add", "create", "new"}, new[] {"task", "reminder", "todo"}) ||
                ContainsKeywords(inputLower, new[] {"remind me", "set reminder"}))
            {
                LogActivity("NLP: Recognized basic task creation intent");
                return "I'll help you create a new cybersecurity task! You can say something like 'create a task in 10 minutes to enable 2FA' or 'remind me in 1 day and 2 hours to secure my network'. Or use the 'Add New Task' button for more options.";
            }
            
            // Handle task completion through natural language
            var completionKeywords = new[] {"complete", "done", "finished", "mark as done", "mark complete", "finished with"};
            if (ContainsAnyKeyword(inputLower, completionKeywords))
            {
                var taskKeywords = ExtractTaskKeywords(inputLower);
                if (taskKeywords.Count > 0)
                {
                    var matchedTask = FindTaskByKeywords(taskKeywords);
                    if (matchedTask != null)
                    {
                        CompleteTask(matchedTask);
                        return $"✅ Great job! I've marked the task '{matchedTask.Title}' as completed. Keep up the excellent work with your cybersecurity practices!";
                    }
                    else
                    {
                        return $"I couldn't find a task matching '{string.Join(" ", taskKeywords)}'. Use the 'View All Tasks' button to see your current tasks and manage them.";
                    }
                }
                else
                {
                    return "Which task would you like to mark as complete? You can say 'set [task name] to finished' or use the 'View All Tasks' button to manage your tasks.";
                }
            }
            
            // Handle showing tasks summary
            if (ContainsKeywords(inputLower, new[] {"what have you done", "what did you do", "summary", "recent actions"}))
            {
                return GetRecentActivitySummary();
            }
            
            // Handle specific commands
            if (ContainsKeywords(inputLower, new[] {"add task", "create task"}))
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
        
        // NLP Helper Methods
        private bool ContainsKeywords(string input, string[] primaryKeywords, string[]? secondaryKeywords = null)
        {
            var hasPrimary = primaryKeywords.Any(keyword => input.Contains(keyword));
            if (secondaryKeywords == null) return hasPrimary;
            var hasSecondary = secondaryKeywords.Any(keyword => input.Contains(keyword));
            return hasPrimary && hasSecondary;
        }
        
        private bool ContainsAnyKeyword(string input, string[] keywords)
        {
            return keywords.Any(keyword => input.Contains(keyword));
        }
        
        private List<string> ExtractTaskKeywords(string input)
        {
            // Remove completion keywords to focus on task identification
            var cleanInput = input;
            var removeWords = new[] {"set", "mark", "complete", "done", "finished", "to", "as", "the", "task", "reminder"};
            
            foreach (var word in removeWords)
            {
                cleanInput = cleanInput.Replace(" " + word + " ", " ").Replace(word + " ", "").Replace(" " + word, "");
            }
            
            // Split and filter meaningful words
            var words = cleanInput.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                 .Where(w => w.Length > 2 && !removeWords.Contains(w.ToLower()))
                                 .ToList();
            
            return words;
        }
        
        private TaskData? FindTaskByKeywords(List<string> keywords)
        {
            // Try exact title match first
            var keywordString = string.Join(" ", keywords).ToLower();
            var exactMatch = _tasks.FirstOrDefault(t => t.Status != "Completed" && t.Title.ToLower().Contains(keywordString));
            if (exactMatch != null) return exactMatch;
            
            // Try individual keyword matches
            foreach (var keyword in keywords)
            {
                var match = _tasks.FirstOrDefault(t => t.Status != "Completed" && 
                    (t.Title.ToLower().Contains(keyword.ToLower()) || t.Description.ToLower().Contains(keyword.ToLower())));
                if (match != null) return match;
            }
            
            return null;
        }
        
        private bool ContainsAdvancedTaskCreation(string input)
        {
            // Pattern 1: "create a task in X time to Y"
            if (input.Contains("create a task in") && input.Contains(" to "))
                return true;
                
            // Pattern 2: "remind me in X time to Y"
            if (input.Contains("remind me in") && input.Contains(" to "))
                return true;
                
            // Pattern 3: "set a task for X time to Y"
            if (input.Contains("set a task for") && input.Contains(" to "))
                return true;
                
            // Pattern 4: "create task in X to Y"
            if (input.Contains("create task in") && input.Contains(" to "))
                return true;
                
            // Pattern 5: "add task in X to Y"
            if (input.Contains("add task in") && input.Contains(" to "))
                return true;
                
            // Pattern 6: "schedule task in X to Y"
            if (input.Contains("schedule task in") && input.Contains(" to "))
                return true;
                
            // Pattern 7: "set reminder in X to Y"
            if (input.Contains("set reminder in") && input.Contains(" to "))
                return true;
                
            // Pattern 8: "make a task in X to Y"
            if (input.Contains("make a task in") && input.Contains(" to "))
                return true;
                
            // Pattern 9: "set a reminder to X in Y" (IMPORTANT FIX)
            if (input.Contains("set a reminder to") && input.Contains(" in "))
                return true;
                
            // Pattern 10: "set reminder to X in Y"
            if (input.Contains("set reminder to") && input.Contains(" in "))
                return true;
            
            return false;
        }
        
        private string HandleAdvancedTaskCreation(string input)
        {
            try
            {
                var (timeExpression, taskDescription) = ParseTaskCreationInput(input);
                
                if (string.IsNullOrEmpty(taskDescription))
                {
                    return "I understand you want to create a task, but I couldn't determine what the task should be. Please try saying something like 'create a task in 10 minutes to enable 2FA' or 'remind me in 1 day to secure my network'.";
                }
                
                if (string.IsNullOrEmpty(timeExpression))
                {
                    return $"I understand you want to create a task '{taskDescription}', but when would you like to be reminded? You can say something like 'in 30 minutes', 'in 2 hours', 'tomorrow', or 'in 1 day and 3 hours'.";
                }
                
                // Parse the time expression
                var reminderTime = ReminderService.ParseReminderTime(timeExpression);
                
                // Create the task
                var task = new TaskData
                {
                    Title = taskDescription,
                    Description = $"Cybersecurity task: {taskDescription}",
                    ReminderText = $"Reminder set for: {reminderTime:MMM dd, yyyy h:mm tt}",
                    CreatedDate = DateTime.Now,
                    Status = "Pending",
                    Priority = "Medium",
                    Category = "Cybersecurity",
                    DueDate = reminderTime
                };
                
                _tasks.Add(task);
                SaveTasks();
                
                // Add to reminder service
                ReminderService.AddReminder(taskDescription, $"Time to work on: {taskDescription}", reminderTime);
                
                LogActivity($"Created task via natural language: '{taskDescription}' for {reminderTime}");
                
                return $"✅ Perfect! I've created a task '{taskDescription}' with a reminder set for {reminderTime:MMM dd, yyyy} at {reminderTime:h:mm tt}.\n\n" +
                       $"📱 You'll receive both a popup notification and a Windows notification when it's time to complete this task.\n\n" +
                       $"💡 You can view and manage all your tasks using the 'View All Tasks' button on the right.";
            }
            catch (Exception ex)
            {
                LogActivity($"Error creating task via natural language: {ex.Message}");
                return $"I had trouble creating that task: {ex.Message}. Please try using the 'Add New Task' button for more reliable task creation, or try rephrasing your request.";
            }
        }
        
        private (string timeExpression, string taskDescription) ParseTaskCreationInput(string input)
        {
            var lowerInput = input.ToLower();
            
            // Handle "set a reminder to X in Y" pattern specifically
            if (lowerInput.Contains("set a reminder to") || lowerInput.Contains("set reminder to"))
            {
                var reminderToIndex = lowerInput.IndexOf("reminder to");
                if (reminderToIndex != -1)
                {
                    var afterReminderTo = input.Substring(reminderToIndex + 11).Trim(); // "reminder to".Length = 11
                    var reminderInIndex = afterReminderTo.IndexOf(" in ");
                    
                    if (reminderInIndex != -1)
                    {
                        var reminderTaskDescription = afterReminderTo.Substring(0, reminderInIndex).Trim();
                        var reminderTimeExpression = afterReminderTo.Substring(reminderInIndex + 4).Trim(); // " in ".Length = 4
                        return (reminderTimeExpression, reminderTaskDescription);
                    }
                }
            }
            
            // Handle standard "X in Y to Z" patterns
            var inIndex = -1;
            var toIndex = -1;
            
            // Look for various patterns
            var inPatterns = new[] { " in ", "task in ", "reminder in " };
            var toPatterns = new[] { " to ", " to do ", " for " };
            
            foreach (var pattern in inPatterns)
            {
                var index = lowerInput.IndexOf(pattern);
                if (index != -1)
                {
                    inIndex = index + pattern.Length;
                    break;
                }
            }
            
            foreach (var pattern in toPatterns)
            {
                var index = lowerInput.IndexOf(pattern, inIndex > 0 ? inIndex : 0);
                if (index != -1)
                {
                    toIndex = index;
                    break;
                }
            }
            
            if (inIndex == -1 || toIndex == -1 || toIndex <= inIndex)
            {
                return ("", "");
            }
            
            // Extract time expression (between "in" and "to")
            var timeExpression = input.Substring(inIndex, toIndex - inIndex).Trim();
            
            // Extract task description (after "to")
            var taskStart = toIndex;
            while (taskStart < input.Length && (input[taskStart] == ' ' || input[taskStart] == 't' || input[taskStart] == 'o'))
            {
                taskStart++;
            }
            
            var taskDescription = "";
            if (taskStart < input.Length)
            {
                taskDescription = input.Substring(taskStart).Trim();
                
                // Clean up common prefixes
                if (taskDescription.StartsWith("do "))
                    taskDescription = taskDescription.Substring(3).Trim();
            }
            
            return (timeExpression, taskDescription);
        }
        
        private string GetRecentActivitySummary()
        {
            var recentTasks = _tasks.Where(t => t.CreatedDate > DateTime.Now.AddDays(-7)).ToList();
            var completedTasks = _tasks.Where(t => t.Status == "Completed" && t.CompletedDate > DateTime.Now.AddDays(-7)).ToList();
            
            var summary = "📊 Here's a summary of recent actions:\n\n";
            
            if (recentTasks.Count > 0)
            {
                summary += $"📝 Tasks created this week: {recentTasks.Count}\n";
                foreach (var task in recentTasks.Take(3))
                {
                    summary += $"   • {task.Title} (Created: {task.CreatedDate:MMM dd})\n";
                }
            }
            
            if (completedTasks.Count > 0)
            {
                summary += $"\n✅ Tasks completed this week: {completedTasks.Count}\n";
                foreach (var task in completedTasks.Take(3))
                {
                    summary += $"   • {task.Title} (Completed: {task.CompletedDate:MMM dd})\n";
                }
            }
            
            if (recentTasks.Count == 0 && completedTasks.Count == 0)
            {
                summary += "No recent activity. Start by creating a new cybersecurity task!";
            }
            else
            {
                summary += "\nKeep up the great work with your cybersecurity practices! 🛡️";
            }
            
            LogActivity("Provided recent activity summary");
            return summary;
        }
    }
}
