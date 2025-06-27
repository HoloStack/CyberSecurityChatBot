using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace CybersecurityChatbot
{
    public partial class MainWindow : Window
    {
        private ChatbotEngine _chatbotEngine;
        private UserProfile _userProfile;
        private int _bestQuizScore = 0;
        private static readonly HttpClient httpClient = new HttpClient();
        private string _openAiApiKey = ""; // Loaded from config.json
        private DateTime _lastGPTRequest = DateTime.MinValue;
        private const int GPT_REQUEST_DELAY_SECONDS = 3; // Minimum 3 seconds between requests

        public MainWindow()
        {
            InitializeComponent();
            
            // Load or create user profile
            _userProfile = UserProfile.LoadProfile();
            
            // Show user setup if profile is incomplete
            if (!_userProfile.IsComplete())
            {
                var setupDialog = new UserSetupDialog();
                if (setupDialog.ShowDialog() == true)
                {
                    _userProfile = setupDialog.UserProfile;
                }
                else
                {
                    _userProfile = setupDialog.UserProfile; // Use minimal profile
                }
            }
            
            _chatbotEngine = new ChatbotEngine(_userProfile);
            InitializeChat();
            LoadChatHistory();
            UpdateTaskCount();
            
            // Load API key from config.json
            LoadApiKey();
            
            // Configure HttpClient for OpenAI
            if (!string.IsNullOrEmpty(_openAiApiKey))
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAiApiKey}");
            }
            
        }

        private void InitializeChat()
        {
            var greeting = _userProfile.GetGreeting();
            AddBotMessage($"🛡️ {greeting} Welcome to your personal cybersecurity assistant!");
            AddBotMessage("I can help you with:");
            AddBotMessage("📝 Task Management - Add cybersecurity tasks with reminders");
            AddBotMessage("🎯 Interactive Quiz - Test your knowledge");
            AddBotMessage("📊 Activity Logging - Track your progress");
            AddBotMessage("🧠 Smart Responses - I understand your questions");
            AddBotMessage("🤖 ChatGPT Integration - Get advanced AI assistance");
            
            if (!string.IsNullOrWhiteSpace(_userProfile.FavoriteSubject))
            {
                AddBotMessage($"💡 I noticed you're interested in {_userProfile.FavoriteSubject}. Try asking me about it for personalized advice!");
            }
            
            AddBotMessage("Try asking about passwords, phishing, malware, or use the buttons on the right!");
            
            // Ensure RichTextBox is properly configured
            MessageRichTextBox.Document.Blocks.Clear();
            MessageRichTextBox.Foreground = Brushes.Black;
            MessageRichTextBox.Background = Brushes.White;
            MessageRichTextBox.FontSize = 14;
            MessageRichTextBox.Focus();
            
            // Enable ChatGPT button
            ChatGPTButton.IsEnabled = true;
        }

        private void AddUserMessage(string message)
        {
            var messagePanel = CreateMessagePanel(message, true);
            ChatMessagesPanel.Children.Add(messagePanel);
            ScrollToBottom();
        }

        private void AddBotMessage(string message)
        {
            var messagePanel = CreateMessagePanel(message, false);
            ChatMessagesPanel.Children.Add(messagePanel);
            ScrollToBottom();
        }

        private void AddGPTMessage(string message)
        {
            var messagePanel = CreateMessagePanel(message, false, true);
            ChatMessagesPanel.Children.Add(messagePanel);
            ScrollToBottom();
        }

        private Border CreateMessagePanel(string message, bool isUser, bool isGPT = false)
        {
            var border = new Border
            {
                Margin = new Thickness(0, 5, 0, 5),
                Padding = new Thickness(15, 10, 15, 10),
                CornerRadius = new CornerRadius(15),
                MaxWidth = 400,
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left
            };

            if (isUser)
            {
                border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498DB"));
            }
            else if (isGPT)
            {
                border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9B59B6"));
            }
            else
            {
                border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                border.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    Opacity = 0.1,
                    BlurRadius = 5,
                    ShadowDepth = 1
                };
            }

            var stackPanel = new StackPanel();
            
            if (!isUser)
            {
                var header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 5) };
                header.Children.Add(new TextBlock 
                { 
                    Text = isGPT ? "🤖" : "🛡️", 
                    FontSize = 16, 
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 5, 0)
                });
                header.Children.Add(new TextBlock 
                { 
                    Text = isGPT ? "ChatGPT Assistant" : "Cybersecurity Bot", 
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 12,
                    Foreground = isGPT ? Brushes.White : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F8C8D")),
                    VerticalAlignment = VerticalAlignment.Center
                });
                stackPanel.Children.Add(header);
            }

            var textBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                Foreground = isUser || isGPT ? Brushes.White : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C3E50")),
                FontSize = 14,
                LineHeight = 20
            };

            stackPanel.Children.Add(textBlock);
            border.Child = stackPanel;

            return border;
        }

        private void ScrollToBottom()
        {
            ChatScrollViewer.ScrollToBottom();
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await ProcessMessage(false);
        }

        private async void ChatGPTButton_Click(object sender, RoutedEventArgs e)
        {
            await ProcessMessage(true);
        }

        private async void MessageRichTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await ProcessMessage(false);
            }
        }

        private async Task ProcessMessage(bool useGPT)
        {
            var message = GetRichTextBoxText();
            if (string.IsNullOrEmpty(message))
                return;

            AddUserMessage(message);
            MessageRichTextBox.Document.Blocks.Clear();
            MessageRichTextBox.Focus(); // Keep focus on the text box

            // Show typing indicator
            var typingBorder = CreateTypingIndicator(useGPT);
            ChatMessagesPanel.Children.Add(typingBorder);
            ScrollToBottom();

            try
            {
                string response;
                if (useGPT)
                {
                    response = await GetChatGPTResponse(message);
                    AddGPTMessage(response);
                    JsonDataService.SaveConversation(message, response, "gpt");
                }
                else
                {
                    // Simulate processing delay
                    await Task.Delay(800);
                    response = _chatbotEngine.ProcessMessage(message.ToLower());
                    AddBotMessage(response);
                    JsonDataService.SaveConversation(message, response, "chatbot");
                }
            }
            catch (Exception ex)
            {
                AddBotMessage($"❌ Error: {ex.Message}");
            }
            finally
            {
                // Remove typing indicator
                ChatMessagesPanel.Children.Remove(typingBorder);
                UpdateTaskCount();
            }
        }

        private async Task<string> GetChatGPTResponse(string message)
        {
            try
            {
                // Check rate limiting
                var timeSinceLastRequest = DateTime.Now - _lastGPTRequest;
                if (timeSinceLastRequest.TotalSeconds < GPT_REQUEST_DELAY_SECONDS)
                {
                    var waitTime = GPT_REQUEST_DELAY_SECONDS - (int)timeSinceLastRequest.TotalSeconds;
                    return $"⏱️ Please wait {waitTime} more second(s) before making another ChatGPT request to avoid rate limits.";
                }

                _lastGPTRequest = DateTime.Now;

                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content = "Cybersecurity assistant. Brief, practical advice only."
                        },
                        new
                        {
                            role = "user",
                            content = message
                        }
                    },
                    max_tokens = 150,
                    temperature = 0.5
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var chatResponse = jsonResponse.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
                    
                    _chatbotEngine.LogActivity($"ChatGPT query: {message}");
                    return chatResponse ?? "Sorry, I couldn't generate a response.";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    return "⚠️ ChatGPT is currently rate-limited. Please try again in a few minutes. You can still use the regular chatbot features!";
                }
                else
                {
                    return $"Sorry, I couldn't connect to ChatGPT right now. Error: {response.StatusCode}. Try using the regular Send button instead!";
                }
            }
            catch (Exception ex)
            {
                return $"Sorry, there was an error connecting to ChatGPT: {ex.Message}. The regular chatbot is still available!";
            }
        }

        private Border CreateTypingIndicator(bool isGPT = false)
        {
            var border = new Border
            {
                Margin = new Thickness(0, 5, 0, 5),
                Padding = new Thickness(15, 10, 15, 10),
                CornerRadius = new CornerRadius(15),
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = isGPT ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9B59B6")) : 
                                   new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF")),
                Effect = isGPT ? null : new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    Opacity = 0.1,
                    BlurRadius = 5,
                    ShadowDepth = 1
                }
            };

            var textBlock = new TextBlock
            {
                Text = isGPT ? "🤖 ChatGPT is thinking..." : "🛡️ Bot is typing...",
                Foreground = isGPT ? Brushes.White : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F8C8D")),
                FontStyle = FontStyles.Italic,
                FontSize = 14
            };

            border.Child = textBlock;
            return border;
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new TaskDialog();
            if (dialog.ShowDialog() == true)
            {
                _chatbotEngine.AddTask(dialog.TaskTitle, dialog.TaskDescription, dialog.ReminderTime);
                _chatbotEngine.LogActivity($"Created new task: '{dialog.TaskTitle}' with reminder: {dialog.ReminderTime}");
                AddBotMessage($"✅ Task added: '{dialog.TaskTitle}'. {dialog.ReminderTime}");
                UpdateTaskCount();
            }
        }

        private void ViewTasksButton_Click(object sender, RoutedEventArgs e)
        {
            var tasks = _chatbotEngine.GetTasks();
            _chatbotEngine.LogActivity($"Viewed task list - {tasks.Count} total tasks");
            
            if (tasks.Count == 0)
            {
                AddBotMessage("📝 You don't have any tasks yet. Use 'Add New Task' to create one!");
                return;
            }

            var tasksWindow = new TasksWindow(tasks, _chatbotEngine);
            tasksWindow.TasksUpdated += () => UpdateTaskCount();
            tasksWindow.ShowDialog();
        }

        private void StartQuizButton_Click(object sender, RoutedEventArgs e)
        {
            _chatbotEngine.LogActivity("Started cybersecurity quiz");
            var quizWindow = new QuizWindow();
            quizWindow.QuizCompleted += (score, total) =>
            {
                var percentage = (double)score / total * 100;
                _chatbotEngine.LogActivity($"Completed quiz - Score: {score}/{total} ({percentage:F0}%)");
                
                if (score > _bestQuizScore)
                {
                    _bestQuizScore = score;
                    QuizScoreLabel.Text = $"Best Score: {score}/{total} ({percentage:F0}%)";
                    _chatbotEngine.LogActivity($"New best quiz score achieved: {score}/{total}");
                }
                
                AddBotMessage($"🏆 Quiz completed! Score: {score}/{total} ({percentage:F0}%)");
                
                if (percentage >= 80)
                {
                    AddBotMessage("🎉 Excellent! You're a cybersecurity pro!");
                    _chatbotEngine.LogActivity("Achieved excellent quiz score (80%+)");
                }
                else if (percentage >= 60)
                {
                    AddBotMessage("👍 Good job! Keep learning to stay safe online!");
                    _chatbotEngine.LogActivity("Achieved good quiz score (60-79%)");
                }
                else
                {
                    AddBotMessage("📚 Keep studying! Cybersecurity knowledge is crucial for staying safe.");
                    _chatbotEngine.LogActivity("Quiz score below 60% - recommended additional study");
                }
            };
            
            quizWindow.ShowDialog();
        }

        private void ViewLogButton_Click(object sender, RoutedEventArgs e)
        {
            var logs = _chatbotEngine.GetActivityLogs();
            if (logs.Count == 0)
            {
                AddBotMessage("📊 No activity recorded yet. Start using features to see your activity log!");
                return;
            }

            var logWindow = new ActivityLogWindow(logs);
            logWindow.ShowDialog();
        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to clear the activity log?", 
                "Clear Activity Log", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                _chatbotEngine.ClearActivityLog();
                AddBotMessage("🗑️ Activity log cleared.");
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            var helpWindow = new HelpWindow();
            helpWindow.ShowDialog();
        }

        private void CreateAccountButton_Click(object sender, RoutedEventArgs e)
        {
            var setupDialog = new UserSetupDialog();
            if (setupDialog.ShowDialog() == true)
            {
                _userProfile = setupDialog.UserProfile;
                _chatbotEngine = new ChatbotEngine(_userProfile);
                
                // Clear chat and reinitialize with new profile
                ChatMessagesPanel.Children.Clear();
                InitializeChat();
                UpdateTaskCount();
                
                AddBotMessage("✅ Account setup completed! Your profile has been updated.");
            }
        }

        private void ConsoleButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Would you like to launch the console version of the chatbot?", 
                "Launch Console Mode", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/K dotnet run --console",
                        WorkingDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                        UseShellExecute = true
                    };
                    Process.Start(processInfo);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to launch console mode: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UpdateTaskCount()
        {
            var taskCount = _chatbotEngine.GetTaskCount();
            TaskCountLabel.Text = $"{taskCount} Active Task{(taskCount != 1 ? "s" : "")}";
        }

        private string GetRichTextBoxText()
        {
            var textRange = new TextRange(MessageRichTextBox.Document.ContentStart, MessageRichTextBox.Document.ContentEnd);
            return textRange.Text?.Trim() ?? "";
        }
        
        private void LoadApiKey()
        {
            try
            {
                var configPath = "config.json";
                if (File.Exists(configPath))
                {
                    var configJson = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<JsonElement>(configJson);
                    
                    if (config.TryGetProperty("OpenAI", out var openAiSection) &&
                        openAiSection.TryGetProperty("ApiKey", out var apiKeyElement))
                    {
                        _openAiApiKey = apiKeyElement.GetString() ?? "";
                        
                        if (string.IsNullOrEmpty(_openAiApiKey) || _openAiApiKey == "YOUR_OPENAI_API_KEY_HERE")
                        {
                            _chatbotEngine.LogActivity("OpenAI API key not configured - ChatGPT features disabled");
                        }
                        else
                        {
                            _chatbotEngine.LogActivity("OpenAI API key loaded successfully");
                        }
                    }
                }
                else
                {
                    _chatbotEngine.LogActivity("config.json not found - ChatGPT features disabled");
                }
            }
            catch (Exception ex)
            {
                _chatbotEngine.LogActivity($"Error loading API key: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error loading API key: {ex.Message}");
            }
        }
        
        private void LoadChatHistory()
        {
            try
            {
                var conversations = JsonDataService.LoadConversationHistory();
                var recentConversations = conversations.TakeLast(5).ToList(); // Show last 5 conversations
                
                if (recentConversations.Count > 0)
                {
                    AddBotMessage("--- Loading recent conversation history ---");
                    
                    foreach (var conversation in recentConversations)
                    {
                        AddUserMessage(conversation.UserMessage);
                        if (conversation.ResponseType == "gpt")
                        {
                            AddGPTMessage(conversation.BotResponse);
                        }
                        else
                        {
                            AddBotMessage(conversation.BotResponse);
                        }
                    }
                    
                    AddBotMessage("--- End of history ---");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading chat history: {ex.Message}");
            }
        }
    }
}
