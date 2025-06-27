using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CybersecurityChatbot
{
    public class ConsoleQuizManager
    {
        private List<Question> quizQuestions = new List<Question>();
        private Action<string> logActivity;
        private Action<string> botResponse;
        private int quizScore = 0;

        public ConsoleQuizManager(Action<string> logActivityCallback, Action<string> botResponseCallback)
        {
            logActivity = logActivityCallback;
            botResponse = botResponseCallback;
            InitializeQuizQuestions();
        }

        private void InitializeQuizQuestions()
        {
            quizQuestions = new List<Question>
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
            };
        }

        public bool ShouldStartQuiz(string input)
        {
            return input.Contains("start quiz") || input.Contains("begin quiz") || input.Contains("take quiz") ||
                   input == "quiz" || input.Contains("test knowledge") || input.Contains("test me");
        }

        public void StartQuiz()
        {
            if (quizQuestions.Count == 0)
            {
                botResponse("Sorry, no quiz questions are available right now.");
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
            logActivity($"Completed quiz with score: {quizScore}/{questionsToAsk.Count}");
        }

        private void ShowQuizResults(int totalQuestions)
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

        public int GetLastQuizScore()
        {
            return quizScore;
        }

        public List<Question> GetQuestions()
        {
            return quizQuestions.ToList();
        }
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
