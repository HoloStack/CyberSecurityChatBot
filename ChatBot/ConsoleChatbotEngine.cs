using System;
using System.Collections.Generic;
using System.Linq;

namespace CybersecurityChatbot
{
    public class ConsoleChatbotEngine
    {
        private Dictionary<string, List<string>> keywordResponses;
        private List<string> positiveSentiments;
        private List<string> worriedSentiments;
        private List<string> frustratedSentiments;
        private Action<string> logActivity;
        private Action<string> botResponse;

        public ConsoleChatbotEngine(Action<string> logActivityCallback, Action<string> botResponseCallback)
        {
            logActivity = logActivityCallback;
            botResponse = botResponseCallback;
            
            keywordResponses = ResponcesList.keywordResponses;
            positiveSentiments = new List<string> { "interested", "curious", "excited", "keen" };
            worriedSentiments = new List<string> { "worried", "scared", "nervous", "concerned", "anxious" };
            frustratedSentiments = new List<string> { "frustrated", "tired", "angry", "fed up" };
        }

        public bool DetectSentiment(string input, out string sentiment)
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

        public void RespondWithSentiment(string sentiment, string name)
        {
            switch (sentiment)
            {
                case "worried":
                    botResponse($"I understand you're feeling worried, {name}. Let's take cybersecurity step-by-step to make it less overwhelming. Would you like to start with a simple task?");
                    break;
                case "frustrated":
                    botResponse($"I'm sorry you're feeling frustrated, {name}. Cybersecurity can be complex, but I'm here to guide you through it. What specific area is troubling you?");
                    break;
                case "positive":
                    botResponse($"That's awesome, {name}! I love your enthusiasm for cybersecurity. Let's channel that energy into learning something new!");
                    break;
            }
            logActivity($"Responded to {sentiment} sentiment");
        }

        public bool ProcessKeywordResponse(string input)
        {
            foreach (var keyword in keywordResponses.Keys)
            {
                if (input.Contains(keyword))
                {
                    ShowRandomTip(keyword);
                    return true;
                }
            }
            return false;
        }

        private void ShowRandomTip(string topic)
        {
            var tips = keywordResponses[topic];
            var random = new Random();
            string tip = tips[random.Next(tips.Count)];
            botResponse($"💡 {tip}");
            logActivity($"Provided tip about: {topic}");
        }

        public bool ShouldShowHelp(string input)
        {
            return input.Contains("help") || input.Contains("commands");
        }

        public void ShowHelp()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nℹ️ AVAILABLE COMMANDS:");
            Console.WriteLine("════════════════════════");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("📝 TASK MANAGEMENT:");
            Console.WriteLine("  • add task [description] / create task / new task");
            Console.WriteLine("  • show tasks / view tasks / list tasks / tasks");
            Console.WriteLine("  • complete task - Mark a task as completed");
            Console.WriteLine("  • delete task - Remove a task");
            Console.WriteLine("  • Advanced: 'create a task in [time] to [description]'");
            Console.WriteLine("  • Advanced: 'remind me in [time] to [description]'");
            Console.WriteLine();
            Console.WriteLine("🎯 QUIZ SYSTEM:");
            Console.WriteLine("  • start quiz / quiz / take quiz / test me");
            Console.WriteLine();
            Console.WriteLine("📊 ACTIVITY TRACKING:");
            Console.WriteLine("  • activity log / view activity / log / history");
            Console.WriteLine("  • clear log - Clear the activity log");
            Console.WriteLine();
            Console.WriteLine("🧠 SMART RESPONSES:");
            Console.WriteLine("  • Ask about: passwords, phishing, malware, 2FA, VPN, etc.");
            Console.WriteLine("  • Express feelings: worried, excited, frustrated, etc.");
            Console.WriteLine();
            Console.WriteLine("🔧 GENERAL:");
            Console.WriteLine("  • help - Show this help menu");
            Console.WriteLine("  • exit - Close the chatbot");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("💡 NATURAL LANGUAGE EXAMPLES:");
            Console.WriteLine("  • 'create a task in 10 minutes to enable 2FA'");
            Console.WriteLine("  • 'remind me in 1 day and 2 hours to secure my network'");
            Console.WriteLine("  • 'set the 2fa reminder to done'");
            Console.WriteLine("  • 'mark the password task as complete'");
            Console.ResetColor();
            logActivity("Viewed help menu");
        }

        public bool ShouldProcessActivityLog(string input)
        {
            return input.Contains("activity log") || input.Contains("show activity") || input.Contains("show log") ||
                   input.Contains("view activity") || input.Contains("view log") || input.Contains("my activity") ||
                   input == "activity" || input == "log" || input == "history" ||
                   input.Contains("show history") || input.Contains("view history");
        }

        public bool ShouldClearLog(string input)
        {
            return input.Contains("clear log");
        }

        public void ProcessUnknownInput(string input, string userName)
        {
            botResponse($"I'm not sure about that, {userName}. Try asking about passwords, phishing, malware, or use 'help' to see all commands.");
        }
    }
}
