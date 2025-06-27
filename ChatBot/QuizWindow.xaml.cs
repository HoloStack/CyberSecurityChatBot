using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace CybersecurityChatbot
{
    public partial class QuizWindow : Window
    {
        public event Action<int, int>? QuizCompleted;
        
        private List<QuizQuestion> _questions = new List<QuizQuestion>();
        private int _currentIndex;
        private DispatcherTimer _timer = new DispatcherTimer();
        private int _elapsedSeconds;
        private QuizResult _quizResult = new QuizResult();

        public QuizWindow()
        {
            InitializeComponent();
            LoadQuiz();
        }

        private void LoadQuiz()
        {
            var allQuestions = JsonDataService.LoadQuestions();
            _questions = GetRandomQuestions(allQuestions, 10); // Get 10 random questions
            _quizResult = new QuizResult { TotalQuestions = _questions.Count };
            _currentIndex = 0;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += TimerOnTick;

            ShowStartScreen();
        }

        private List<QuizQuestion> GetRandomQuestions(List<QuizQuestion> allQuestions, int count)
        {
            if (allQuestions.Count == 0) return new List<QuizQuestion>();
            
            var random = new Random();
            var selectedQuestions = new List<QuizQuestion>();
            var availableQuestions = new List<QuizQuestion>(allQuestions);

            while (selectedQuestions.Count < Math.Min(count, availableQuestions.Count))
            {
                var randomIndex = random.Next(availableQuestions.Count);
                var question = availableQuestions[randomIndex];
                selectedQuestions.Add(question);
                availableQuestions.RemoveAt(randomIndex); // Remove to avoid duplicates
            }

            return selectedQuestions;
        }


        private void ShowStartScreen()
        {
            StartScreen.Visibility = Visibility.Visible;
            QuizScreen.Visibility = Visibility.Collapsed;
            ResultsScreen.Visibility = Visibility.Collapsed;
            ReviewScreen.Visibility = Visibility.Collapsed;
        }

        private void StartQuizButton_Click(object sender, RoutedEventArgs e)
        {
            StartScreen.Visibility = Visibility.Collapsed;
            QuizScreen.Visibility = Visibility.Visible;
            ShowQuestion(_currentIndex);
            _timer.Start();
            _elapsedSeconds = 0;
        }

        private void ShowQuestion(int index)
        {
            if (index < 0 || index >= _questions.Count) return;

            var question = _questions[index];
            QuestionText.Text = question.Text;
            CategoryText.Text = $"Category: {question.Category}";
            QuestionProgressText.Text = $"Question {index + 1} of {_questions.Count}";
            QuizProgressBar.Value = (index + 1) * 100 / _questions.Count;

            AnswerOptionsPanel.Children.Clear();
            for (int i = 0; i < question.Options.Count; i++)
            {
                var optionButton = new RadioButton
                {
                    Content = question.Options[i],
                    Margin = new Thickness(0, 5, 0, 5),
                    Style = (Style)FindResource("ModernRadioButtonStyle")
                };
                optionButton.Checked += (s, e) => NextButton.IsEnabled = true;
                AnswerOptionsPanel.Children.Add(optionButton);
            }

            PreviousButton.IsEnabled = index != 0;
            NextButton.IsEnabled = false;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedOption = AnswerOptionsPanel.Children.OfType<RadioButton>().FirstOrDefault(rb => rb.IsChecked == true);
            if (selectedOption == null) return; // Shouldn't happen, as button is disabled otherwise

            StoreAnswer(selectedOption);
            _currentIndex++;

            if (_currentIndex >= _questions.Count)
            {
                EndQuiz();
            }
            else
            {
                ShowQuestion(_currentIndex);
            }
        }

        private void StoreAnswer(RadioButton selectedOption)
        {
            var question = _questions[_currentIndex];
            var userAnswerIndex = AnswerOptionsPanel.Children.IndexOf(selectedOption);
            var isCorrect = userAnswerIndex == question.CorrectAnswerIndex;
            
            _quizResult.QuestionResults.Add(new QuestionResult {
                Question = question,
                UserAnswerIndex = userAnswerIndex,
                IsCorrect = isCorrect
            });

            if (isCorrect) 
            {
                _quizResult.Score++;
                // Could log correct answers if needed
            }
            else
            {
                // Log incorrect answers for learning tracking
                System.Diagnostics.Debug.WriteLine($"Quiz: Incorrect answer for '{question.Text}' - Category: {question.Category}");
            }
            
            ScoreText.Text = $"Score: {_quizResult.Score} / {_questions.Count}";
        }

        private void EndQuiz()
        {
            _timer.Stop();
            _quizResult.TimeTaken = TimeSpan.FromSeconds(_elapsedSeconds);
            ShowResults();
        }

        private void ShowResults()
        {
            QuizScreen.Visibility = Visibility.Collapsed;
            ResultsScreen.Visibility = Visibility.Visible;

            FinalScoreText.Text = _quizResult.Score >= (_questions.Count * 0.8) ? "Excellent!" :
                                  _quizResult.Score >= (_questions.Count * 0.5) ? "Good Job!" :
                                  "Keep Practicing!";
            FinalScoreDetails.Text = $"You scored {_quizResult.Score} out of {_questions.Count}" +
                                     $" ({(int)_quizResult.Percentage}%)";
            TimeTakenText.Text = $"Time taken: {(_quizResult.TimeTaken.Minutes)}:{_quizResult.TimeTaken.Seconds:D2}";

            PerformanceMessage.Text = _quizResult.Score >= (_questions.Count * 0.8) ?
                "You're a cybersecurity pro! Maintain your knowledge and stay secure!" :
                _quizResult.Score >= (_questions.Count * 0.5) ?
                "Nice effort! Improving your skills will make you safer online." :
                "Don't worry, keep learning and you'll improve in no time!";
        }

        private void TimerOnTick(object? sender, EventArgs e)
        {
            _elapsedSeconds++;
            TimerText.Text = $"Time: {(_elapsedSeconds / 60)}:{(_elapsedSeconds % 60):D2}";
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            _currentIndex--;
            ShowQuestion(_currentIndex);
        }

        private void RetakeQuizButton_Click(object sender, RoutedEventArgs e)
        {
            LoadQuiz();
        }
        
        private void ViewReviewButton_Click(object sender, RoutedEventArgs e)
        {
            ShowReview();
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            // Save quiz result to JSON
            JsonDataService.SaveQuizResponse(_quizResult);
            
            QuizCompleted?.Invoke(_quizResult.Score, _quizResult.TotalQuestions);
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BackToResultsButton_Click(object sender, RoutedEventArgs e)
        {
            ReviewScreen.Visibility = Visibility.Collapsed;
            ResultsScreen.Visibility = Visibility.Visible;
        }

        private void ShowReview()
        {
            ResultsScreen.Visibility = Visibility.Collapsed;
            ReviewScreen.Visibility = Visibility.Visible;
            ReviewPanel.Children.Clear();

            foreach (var result in _quizResult.QuestionResults)
            {
                var reviewItem = CreateReviewItem(result);
                ReviewPanel.Children.Add(reviewItem);
            }
        }

        private UIElement CreateReviewItem(QuestionResult result)
        {
            var border = new Border
            {
                Background = result.IsCorrect ? (SolidColorBrush)FindResource("SuccessBrush")
                                              : (SolidColorBrush)FindResource("AccentBrush"),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 10)
            };
            
            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = result.Question.Text,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap
            });

            stack.Children.Add(new TextBlock
            {
                Text = "Your answer: " + result.Question.Options[result.UserAnswerIndex],
                Foreground = result.IsCorrect ? Brushes.White : Brushes.OrangeRed,
                TextWrapping = TextWrapping.Wrap
            });

            stack.Children.Add(new TextBlock
            {
                Text = "Correct answer: " + result.Question.Options[result.Question.CorrectAnswerIndex],
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap
            });

            stack.Children.Add(new TextBlock
            {
                Text = result.Question.Explanation,
                Foreground = Brushes.WhiteSmoke,
                TextWrapping = TextWrapping.Wrap
            });
            
            // Show why explanation for wrong answers
            if (!result.IsCorrect && !string.IsNullOrWhiteSpace(result.Question.Why))
            {
                stack.Children.Add(new TextBlock
                {
                    Text = "\n💡 Why this is correct: " + result.Question.Why,
                    Foreground = Brushes.LightBlue,
                    FontStyle = FontStyles.Italic,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 5, 0, 0)
                });
            }

            border.Child = stack;
            return border;
        }
    }
}
