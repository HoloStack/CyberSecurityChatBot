using System;
using System.Collections.Generic;

namespace CybersecurityChatbot
{
    class Program
    {
        // Core application managers
        private static ConsoleTaskManager taskManager = null!;
        private static ConsoleQuizManager quizManager = null!;
        private static ConsoleChatbotEngine chatbotEngine = null!;
        private static ConsoleActivityManager activityManager = null!;
        
        // Data collections
        private static List<TaskData> tasks = new List<TaskData>();
        private static List<ActivityLogEntry> activityLog = new List<ActivityLogEntry>();
        
        public static void Main(string[] args)
        {
            Console.Clear();
            InitializeApplication();
            InitializeManagers();
            ConsoleUI.ShowWelcome();
            RunChatbot();
        }
        
        static void InitializeApplication()
        {
            ConsoleUI.InitializeApplication();
            ReminderService.StartService();
        }
        
        static void InitializeManagers()
        {
            // Initialize activity manager first (needed for LogActivity callback)
            activityManager = new ConsoleActivityManager(activityLog, BotResponse);
            
            // Initialize other managers with proper callbacks
            taskManager = new ConsoleTaskManager(tasks, LogActivity, BotResponse);
            quizManager = new ConsoleQuizManager(LogActivity, BotResponse);
            chatbotEngine = new ConsoleChatbotEngine(LogActivity, BotResponse);
        }
        
        static void RunChatbot()
        {
            string userName = "User";
            LogActivity("Started console chatbot session");
            
            while (true)
            {
                string input = ConsoleUI.GetUserInput();
                
                if (string.IsNullOrWhiteSpace(input))
                {
                    BotResponse("I'm not sure what you mean. Could you rephrase that?");
                    continue;
                }
                
                if (ConsoleUI.ShouldExit(input))
                {
                    ConsoleUI.ShowGoodbye(userName);
                    LogActivity("User exited the chatbot");
                    break;
                }
                
                ProcessUserInput(input, userName);
            }
        }
        
        static void ProcessUserInput(string input, string userName)
        {
            LogActivity($"User input: {input}");
            
            // Advanced Task Creation
            if (taskManager.ContainsAdvancedTaskCreation(input))
            {
                taskManager.HandleAdvancedTaskCreation(input);
            }
            // Basic Task Creation
            else if (taskManager.ContainsTaskCreation(input))
            {
                var taskInfo = taskManager.ParseTaskCreationInput(input);
                if (string.IsNullOrEmpty(taskInfo.title))
                {
                    BotResponse("What task would you like to add? For example: 'add task enable 2FA' or 'create a task in 10 minutes to enable 2fa'");
                    return;
                }
                taskManager.AddTaskWithTiming(taskInfo.title, taskInfo.timing);
            }
            // Task Completion
            else if (taskManager.ContainsTaskCompletion(input))
            {
                taskManager.HandleTaskCompletion(input);
            }
            // Show Tasks
            else if (input.Contains("show tasks") || input.Contains("list tasks") || input.Contains("view tasks") || 
                     input.Contains("my tasks") || input == "tasks" || input.Contains("task list"))
            {
                taskManager.ShowTasks();
            }
            // Complete Task (manual selection)
            else if (input.Contains("complete task"))
            {
                taskManager.CompleteTask(input);
            }
            // Delete Task
            else if (input.Contains("delete task"))
            {
                taskManager.DeleteTask(input);
            }
            // Quiz
            else if (quizManager.ShouldStartQuiz(input))
            {
                quizManager.StartQuiz();
            }
            // Activity Log
            else if (activityManager.ShouldProcessActivityLog(input))
            {
                activityManager.ShowActivityLog();
            }
            // Clear Log
            else if (activityManager.ShouldClearLog(input))
            {
                activityManager.ClearActivityLog();
            }
            // Help
            else if (chatbotEngine.ShouldShowHelp(input))
            {
                chatbotEngine.ShowHelp();
            }
            // Sentiment Detection
            else if (chatbotEngine.DetectSentiment(input, out string mood))
            {
                chatbotEngine.RespondWithSentiment(mood, userName);
            }
            // Keyword Recognition
            else if (chatbotEngine.ProcessKeywordResponse(input))
            {
                // Response handled by engine
            }
            // Unknown Input
            else
            {
                chatbotEngine.ProcessUnknownInput(input, userName);
            }
        }
        
        // Shared callback methods
        static void BotResponse(string message)
        {
            ConsoleUI.BotResponse(message);
        }
        
        static void LogActivity(string action)
        {
            activityManager.LogActivity(action);
        }
    }
}
