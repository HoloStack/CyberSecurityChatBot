using System;
using System.Collections.Generic;

namespace CybersecurityChatbot
{
    public class QuizQuestion
    {
        public string Text { get; set; } = "";
        public List<string> Options { get; set; } = new List<string>();
        public int CorrectAnswerIndex { get; set; }
        public string Explanation { get; set; } = "";
        public string Category { get; set; } = "";
    }

    public class QuizResult
    {
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public double Percentage => (double)Score / TotalQuestions * 100;
        public List<QuestionResult> QuestionResults { get; set; } = new List<QuestionResult>();
        public TimeSpan TimeTaken { get; set; }
    }

    public class QuestionResult
    {
        public QuizQuestion Question { get; set; } = new QuizQuestion();
        public int UserAnswerIndex { get; set; }
        public bool IsCorrect { get; set; }
    }

    public static class QuizQuestions
    {
        public static List<QuizQuestion> GetAllQuestions()
        {
            return JsonDataService.LoadQuestions();
        }

        public static List<QuizQuestion> GetFallbackQuestions()
        {
            return new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Text = "What is the most effective way to create a strong password?",
                    Options = new List<string>
                    {
                        "Use your birthday and name",
                        "Use a long, random combination of letters, numbers, and symbols",
                        "Use the same password for all accounts",
                        "Use common dictionary words"
                    },
                    CorrectAnswerIndex = 1,
                    Explanation = "Strong passwords should be long (12+ characters), unique, and contain a mix of uppercase, lowercase, numbers, and symbols.",
                    Category = "Passwords"
                },
                new QuizQuestion
                {
                    Text = "What is phishing?",
                    Options = new List<string>
                    {
                        "A type of computer virus",
                        "A method of password encryption",
                        "A fraudulent attempt to obtain sensitive information by pretending to be trustworthy",
                        "A secure communication protocol"
                    },
                    CorrectAnswerIndex = 2,
                    Explanation = "Phishing is a social engineering attack where attackers impersonate legitimate entities to steal sensitive information like passwords or credit card numbers.",
                    Category = "Phishing"
                }
            };
        }

        public static List<QuizQuestion> GetRandomQuestions(int count = 10)
        {
            var allQuestions = GetAllQuestions();
            if (allQuestions.Count == 0) return new List<QuizQuestion>();
            
            var random = new Random();
            var selectedQuestions = new List<QuizQuestion>();
            var availableQuestions = new List<QuizQuestion>(allQuestions);

            while (selectedQuestions.Count < Math.Min(count, allQuestions.Count) && availableQuestions.Count > 0)
            {
                var randomIndex = random.Next(availableQuestions.Count);
                var question = availableQuestions[randomIndex];
                selectedQuestions.Add(question);
                availableQuestions.RemoveAt(randomIndex); // Remove to avoid duplicates
            }

            return selectedQuestions;
        }
    }
}
