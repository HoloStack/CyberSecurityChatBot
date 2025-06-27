using System;
using CybersecurityChatbot.Audio;

namespace CybersecurityChatbot
{
    public static class ConsoleUI
    {
        public static void InitializeApplication()
        {
            Console.Title = "🛡️ Cybersecurity Awareness Chatbot";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine("           🛡️ CYBERSECURITY AWARENESS CHATBOT 🛡️");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.ResetColor();
        }

        public static void ShowWelcome()
        {
            // Play greeting sound
            try
            {
                AudioService.PlayGreeting();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Note: Could not play greeting sound: {ex.Message}");
            }
            
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
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("💡 NEW: Advanced Natural Language Support!");
            Console.WriteLine("Try saying things like:");
            Console.WriteLine("• 'create a task in 10 minutes to enable 2FA'");
            Console.WriteLine("• 'remind me in 1 day and 2 hours to secure my network'");
            Console.WriteLine("• 'set the 2fa reminder to done'");
            Console.WriteLine("• 'mark the password task as complete'");
            Console.ResetColor();
        }

        public static void BotResponse(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"🤖 Bot: {message}");
            Console.ResetColor();
        }

        public static string GetUserInput()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\n👤 You: ");
            Console.ResetColor();
            return Console.ReadLine()?.ToLower() ?? "";
        }

        public static void ShowGoodbye(string userName)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n🛡️ Goodbye {userName}! Stay cyber safe out there! 🛡️");
            Console.WriteLine("Remember to:");
            Console.WriteLine("• Keep your software updated");
            Console.WriteLine("• Use strong, unique passwords");
            Console.WriteLine("• Enable two-factor authentication");
            Console.WriteLine("• Stay vigilant against phishing");
            Console.WriteLine("\nThank you for using the Cybersecurity Awareness Chatbot!");
            Console.ResetColor();
        }

        public static bool ShouldExit(string input)
        {
            return input.Contains("exit") || input.Contains("quit") || input.Contains("bye");
        }

        public static void ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Error: {message}");
            Console.ResetColor();
        }

        public static void ShowSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ {message}");
            Console.ResetColor();
        }

        public static void ShowWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠️ {message}");
            Console.ResetColor();
        }

        public static void ShowInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"ℹ️ {message}");
            Console.ResetColor();
        }

        public static void ClearScreen()
        {
            Console.Clear();
        }

        public static void Pause(int milliseconds = 1500)
        {
            System.Threading.Thread.Sleep(milliseconds);
        }

        public static ConsoleKeyInfo ReadKey()
        {
            return Console.ReadKey();
        }

        public static string ReadLine()
        {
            return Console.ReadLine() ?? "";
        }
    }
}
