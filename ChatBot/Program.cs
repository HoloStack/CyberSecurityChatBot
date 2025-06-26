using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CybersecurityChatbot;

namespace CybersecurityChatbot
{
    class Program
    {
        private static Dictionary<string, List<string>> keywordResponses = ResponcesList.keywordResponses;
        private static Dictionary<string, string> memory = new Dictionary<string, string>();
        private static List<string> positiveSentiments = new List<string> { "interested", "curious", "excited", "keen" };
        private static List<string> worriedSentiments = new List<string> { "worried", "scared", "nervous", "concerned", "anxious" };
        private static List<string> frustratedSentiments = new List<string> { "frustrated", "tired", "angry", "fed up" };
        private static List<TaskData> tasks = new List<TaskData>();
        private static List<ActivityLogEntry> activityLog = new List<ActivityLogEntry>();
        private static List<Question> quizQuestions = new List<Question>();
        private static int quizScore = 0;
        
        static void Main(string[] args)
        {
            Console.Clear();
            InitializeApplication();
            InitializeQuizQuestions();
            ShowWelcome();
            RunChatbot();
        }
        
        static void InitializeApplication()
        {
            Console.Title = "🛡️ Cybersecurity Awareness Chatbot";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine("           🛡️ CYBERSECURITY AWARENESS CHATBOT 🛡️");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.ResetColor();
        }
        
        static void InitializeQuizQuestions()
        {
            quizQuestions.AddRange(new[]
            {
                new Question("What should you do if you receive an email asking for your password?",
                    new[] { "Reply with your password", "Delete the email", "Report the email as phishing", "Ignore it" },
                    2, "Correct! Reporting phishing emails helps prevent scams."),
                new Question("Which of these is the strongest password?",
                    new[] { "password123", "P@ssw0rd2024!", "123456", "qwerty" },
                    1, "Correct! Strong passwords use a mix of uppercase, lowercase, numbers, and symbols."),
                new Question("What does 2FA stand for?",
                    new[] { "Two Factor Authentication", "Two File Access", "Trusted File Archive", "Two Factor Access" },
                    0, "Correct! Two Factor Authentication adds an extra layer of security."),
                new Question("What should you do before clicking a link in an email?",
                    new[] { "Click immediately", "Hover over it to check the URL", "Forward it to friends", "Delete the email" },
                    1, "Correct! Always verify links before clicking to avoid phishing attacks."),
                new Question("How often should you update your software?",
                    new[] { "Never", "Once a year", "When updates are available", "Only when it breaks" },
                    2, "Correct! Regular updates patch security vulnerabilities."),
                new Question("What is social engineering?",
                    new[] { "Building networks", "Manipulating people for information", "Creating social media", "Engineering software" },
                    1, "Correct! Social engineering tricks people into revealing confidential information."),
                new Question("What should you do on public Wi-Fi?",
                    new[] { "Access banking sites", "Use a VPN", "Share passwords", "Download everything" },
                    1, "Correct! VPNs encrypt your data on public networks."),
                new Question("What is ransomware?",
                    new[] { "Free software", "Malware that encrypts files for money", "A type of antivirus", "A web browser" },
                    1, "Correct! Ransomware locks your files until you pay a ransom."),
                new Question("How can you spot a phishing email?",
                    new[] { "Perfect grammar", "Urgent language and suspicious links", "From known contacts", "Professional design" },
                    1, "Correct! Phishing emails often use urgency and suspicious elements."),
                new Question("What is the purpose of a firewall?",
                    new[] { "Speed up internet", "Block unauthorized access", "Store passwords", "Create backups" },
                    1, "Correct! Firewalls monitor and control network traffic for security.")
            });
        }
        
        static void ShowWelcome()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n🎯 Welcome to your personal cybersecurity assistant!");
            Console.WriteLine("I can help you with:");
            Console.WriteLine("📝 Task Management (add cybersecurity tasks with reminders)");
            Console.WriteLine("🎯 Interactive Quiz (test your knowledge)");
            Console.WriteLine("📊 Activity Logging (track your progress)");
            Console.WriteLine("🧠 Smart Responses (I understand your questions)");
            Console.WriteLine("\nTry commands like:");
            Console.WriteLine("• 'add task enable 2FA' - Add a security task");
            Console.WriteLine("• 'start quiz' - Begin the cybersecurity quiz");
            Console.WriteLine("• 'show tasks' - View your current tasks");
            Console.WriteLine("• 'activity log' - See your recent activity");
            Console.WriteLine("• 'help' - Show all available commands");
            Console.WriteLine("• Ask about passwords, phishing, malware, etc.");
            Console.ResetColor();
        }
        
        static void RunChatbot()
        {
            string userName = "User";
            
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("\n👤 You: ");
                Console.ResetColor();
                string input = Console.ReadLine()?.ToLower() ?? "";
                
                if (string.IsNullOrWhiteSpace(input))
                {
                    BotResponse("I'm not sure what you mean. Could you rephrase that?");
                    continue;
                }
                
                if (input.Contains("exit") || input.Contains("quit") || input.Contains("bye"))
                {
                    BotResponse($"Goodbye {userName}! Stay cyber safe out there! 🛡️");
                    LogActivity("User exited the chatbot");
                    break;
                }
                
                ProcessUserInput(input, userName);
            }
        }
        
        static void ProcessUserInput(string input, string userName)
        {
            // Task Management
            if (input.Contains("add task") || input.Contains("create task"))
            {
                string taskTitle = input.Replace("add task", "").Replace("create task", "").Trim();
                if (string.IsNullOrEmpty(taskTitle))
                {
                    BotResponse("What task would you like to add? For example: 'add task enable 2FA'");
                    return;
                }
                AddTask(taskTitle);
            }
            else if (input.Contains("show tasks") || input.Contains("list tasks") || input.Contains("view tasks"))
            {
                ShowTasks();
            }
            else if (input.Contains("complete task"))
            {
                CompleteTask(input);
            }
            else if (input.Contains("delete task"))
            {
                DeleteTask(input);
            }
            // Quiz Management
            else if (input.Contains("start quiz") || input.Contains("begin quiz") || input.Contains("take quiz"))
            {
                StartQuiz();
            }
            // Activity Log
            else if (input.Contains("activity log") || input.Contains("show activity") || input.Contains("show log"))
            {
                ShowActivityLog();
            }
            else if (input.Contains("clear log"))
            {
                ClearActivityLog();
            }
            // Help
            else if (input.Contains("help") || input.Contains("commands"))
            {
                ShowHelp();
            }
            // Sentiment Detection
            else if (DetectSentiment(input, out string mood))
            {
                RespondWithSentiment(mood, userName);
            }
            // Keyword Recognition
            else
            {
                bool matched = false;
                foreach (var keyword in keywordResponses.Keys)
                {
                    if (input.Contains(keyword))
                    {
                        ShowRandomTip(keyword);
                        matched = true;
                        break;
                    }
                }
                
                if (!matched)
                {
                    BotResponse($"I'm not sure about that, {userName}. Try asking about passwords, phishing, malware, or use 'help' to see all commands.");
                }
            }
        }
        
        static void AddTask(string title)
        {
            var task = new TaskData
            {
                Title = title,
                Description = $"Cybersecurity task: {title}",
                ReminderText = "No reminder set",
                CreatedDate = DateTime.Now
            };
            
            tasks.Add(task);
            BotResponse($"✅ Task added: '{title}'. Would you like to set a reminder? (Type 'yes' or 'no')");
            LogActivity($"Added task: {title}");
            
            // Simple reminder logic
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("👤 You: ");
            Console.ResetColor();
            string response = Console.ReadLine()?.ToLower() ?? "";
            
            if (response.Contains("yes"))
            {
                BotResponse("Great! When would you like to be reminded? (e.g., 'in 3 days', 'tomorrow', 'next week')");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("👤 You: ");
                Console.ResetColor();
                string reminderTime = Console.ReadLine() ?? "later";
                task.ReminderText = $"Reminder set for: {reminderTime}";
                BotResponse($"⏰ Reminder set! I'll remind you to '{title}' {reminderTime}.");
                LogActivity($"Set reminder for task: {title} - {reminderTime}");
            }
        }
        
        static void ShowTasks()
        {
            if (tasks.Count == 0)
            {
                BotResponse("📝 You don't have any tasks yet. Use 'add task [description]' to create one!");
                return;
            }
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n📝 YOUR CYBERSECURITY TASKS:");
            Console.WriteLine("════════════════════════════");
            Console.ResetColor();
            
            for (int i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{i + 1}. {task.Title}");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"   Description: {task.Description}");
                Console.WriteLine($"   Created: {task.CreatedDate:yyyy-MM-dd HH:mm}");
                Console.WriteLine($"   {task.ReminderText}");
                Console.WriteLine();
            }
            Console.ResetColor();
            LogActivity("Viewed task list");
        }
        
        static void CompleteTask(string input)
        {
            if (tasks.Count == 0)
            {
                BotResponse("You don't have any tasks to complete.");
                return;
            }
            
            ShowTasks();
            BotResponse("Which task number would you like to mark as complete?");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("👤 You: ");
            Console.ResetColor();
            
            if (int.TryParse(Console.ReadLine(), out int taskIndex) && taskIndex > 0 && taskIndex <= tasks.Count)
            {
                var completedTask = tasks[taskIndex - 1];
                tasks.RemoveAt(taskIndex - 1);
                BotResponse($"✅ Completed task: '{completedTask.Title}'. Great work on improving your cybersecurity!");
                LogActivity($"Completed task: {completedTask.Title}");
            }
            else
            {
                BotResponse("Invalid task number. Please try again.");
            }
        }
        
        static void DeleteTask(string input)
        {
            if (tasks.Count == 0)
            {
                BotResponse("You don't have any tasks to delete.");
                return;
            }
            
            ShowTasks();
            BotResponse("Which task number would you like to delete?");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("👤 You: ");
            Console.ResetColor();
            
            if (int.TryParse(Console.ReadLine(), out int taskIndex) && taskIndex > 0 && taskIndex <= tasks.Count)
            {
                var deletedTask = tasks[taskIndex - 1];
                tasks.RemoveAt(taskIndex - 1);
                BotResponse($"🗑️ Deleted task: '{deletedTask.Title}'.");
                LogActivity($"Deleted task: {deletedTask.Title}");
            }
            else
            {
                BotResponse("Invalid task number. Please try again.");
            }
        }
        
        static void StartQuiz()
        {
            if (quizQuestions.Count == 0)
            {
                BotResponse("Sorry, no quiz questions are available right now.");
                return;
            }
            
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n🎯 CYBERSECURITY KNOWLEDGE QUIZ");
            Console.WriteLine("══════════════════════════════════");
            Console.WriteLine($"This quiz has {quizQuestions.Count} questions. Let's test your cybersecurity knowledge!");
            Console.ResetColor();
            
            quizScore = 0;
            var questionsToAsk = quizQuestions.Take(Math.Min(5, quizQuestions.Count)).ToList();
            
            for (int i = 0; i < questionsToAsk.Count; i++)
            {
                var question = questionsToAsk[i];
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"\nQuestion {i + 1}/{questionsToAsk.Count}:");
                Console.WriteLine(question.Text);
                Console.ResetColor();
                
                for (int j = 0; j < question.Answers.Length; j++)
                {
                    Console.WriteLine($"{j + 1}. {question.Answers[j]}");
                }
                
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Your answer (1-4): ");
                Console.ResetColor();
                
                if (int.TryParse(Console.ReadLine(), out int answer) && answer > 0 && answer <= question.Answers.Length)
                {
                    if (answer - 1 == question.CorrectAnswer)
                    {
                        quizScore++;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("✅ " + question.Feedback);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"❌ Incorrect. The correct answer was: {question.Answers[question.CorrectAnswer]}");
                        Console.WriteLine(question.Feedback);
                    }
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ Invalid answer. Marked as incorrect.");
                    Console.ResetColor();
                }
                
                Thread.Sleep(1500); // Brief pause
            }
            
            ShowQuizResults(questionsToAsk.Count);
            LogActivity($"Completed quiz with score: {quizScore}/{questionsToAsk.Count}");
        }
        
        static void ShowQuizResults(int totalQuestions)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n🏆 QUIZ COMPLETED!");
            Console.WriteLine("═══════════════════");
            Console.WriteLine($"Your Score: {quizScore}/{totalQuestions}");
            
            double percentage = (double)quizScore / totalQuestions * 100;
            
            if (percentage >= 80)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("🎉 Excellent! You're a cybersecurity pro!");
            }
            else if (percentage >= 60)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("👍 Good job! Keep learning to stay safe online!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("📚 Keep studying! Cybersecurity knowledge is crucial for staying safe.");
            }
            Console.ResetColor();
        }
        
        static void ShowActivityLog()
        {
            if (activityLog.Count == 0)
            {
                BotResponse("📊 No activity recorded yet. Start using features to see your activity log!");
                return;
            }
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n📊 RECENT ACTIVITY LOG:");
            Console.WriteLine("═══════════════════════");
            Console.ResetColor();
            
            var recentLogs = activityLog.TakeLast(10).ToList();
            foreach (var log in recentLogs)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"[{log.Timestamp}] {log.Action}");
            }
            Console.ResetColor();
        }
        
        static void ClearActivityLog()
        {
            activityLog.Clear();
            BotResponse("🗑️ Activity log cleared.");
        }
        
        static void ShowHelp()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nℹ️ AVAILABLE COMMANDS:");
            Console.WriteLine("════════════════════════");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("📝 TASK MANAGEMENT:");
            Console.WriteLine("  • add task [description] - Add a cybersecurity task");
            Console.WriteLine("  • show tasks - View all your tasks");
            Console.WriteLine("  • complete task - Mark a task as completed");
            Console.WriteLine("  • delete task - Remove a task");
            Console.WriteLine();
            Console.WriteLine("🎯 QUIZ SYSTEM:");
            Console.WriteLine("  • start quiz - Begin the cybersecurity knowledge quiz");
            Console.WriteLine();
            Console.WriteLine("📊 ACTIVITY TRACKING:");
            Console.WriteLine("  • activity log - View your recent activity");
            Console.WriteLine("  • clear log - Clear the activity log");
            Console.WriteLine();
            Console.WriteLine("🧠 SMART RESPONSES:");
            Console.WriteLine("  • Ask about: passwords, phishing, malware, 2FA, VPN, etc.");
            Console.WriteLine("  • Express feelings: worried, excited, frustrated, etc.");
            Console.WriteLine();
            Console.WriteLine("🔧 GENERAL:");
            Console.WriteLine("  • help - Show this help menu");
            Console.WriteLine("  • exit - Close the chatbot");
            Console.ResetColor();
            LogActivity("Viewed help menu");
        }
        
        static bool DetectSentiment(string input, out string sentiment)
        {
            foreach (var word in worriedSentiments)
                if (input.Contains(word)) { sentiment = "worried"; return true; }
            foreach (var word in frustratedSentiments)
                if (input.Contains(word)) { sentiment = "frustrated"; return true; }
            foreach (var word in positiveSentiments)
                if (input.Contains(word)) { sentiment = "positive"; return true; }
            sentiment = "";
            return false;
        }
        
        static void RespondWithSentiment(string sentiment, string name)
        {
            switch (sentiment)
            {
                case "worried":
                    BotResponse($"I understand you're feeling worried, {name}. Let's take cybersecurity step-by-step to make it less overwhelming. Would you like to start with a simple task?");
                    break;
                case "frustrated":
                    BotResponse($"I'm sorry you're feeling frustrated, {name}. Cybersecurity can be complex, but I'm here to guide you through it. What specific area is troubling you?");
                    break;
                case "positive":
                    BotResponse($"That's awesome, {name}! I love your enthusiasm for cybersecurity. Let's channel that energy into learning something new!");
                    break;
            }
            LogActivity($"Responded to {sentiment} sentiment");
        }
        
        static void ShowRandomTip(string topic)
        {
            var tips = keywordResponses[topic];
            var random = new Random();
            string tip = tips[random.Next(tips.Count)];
            BotResponse($"💡 {tip}");
            LogActivity($"Provided tip about: {topic}");
        }
        
        static void BotResponse(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"🤖 Bot: {message}");
            Console.ResetColor();
        }
        
        static void LogActivity(string action)
        {
            activityLog.Add(new ActivityLogEntry
            {
                Action = action,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }
    }
    
    public class TaskData
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string ReminderText { get; set; } = "";
        public DateTime CreatedDate { get; set; }
    }
    
    public class ActivityLogEntry
    {
        public string Action { get; set; } = "";
        public string Timestamp { get; set; } = "";
    }
    
    public class Question
    {
        public string Text { get; set; }
        public string[] Answers { get; set; }
        public int CorrectAnswer { get; set; }
        public string Feedback { get; set; }
        
        public Question(string text, string[] answers, int correctAnswer, string feedback)
        {
            Text = text;
            Answers = answers;
            CorrectAnswer = correctAnswer;
            Feedback = feedback;
        }
    }
}
