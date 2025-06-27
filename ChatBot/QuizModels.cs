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
        public string Why { get; set; } = "";
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
            // Return empty list - should rely only on JSON file
            return new List<QuizQuestion>();
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
