using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace CybersecurityChatbot
{
    public partial class TaskDialog : Window
    {
        public string TaskTitle { get; set; } = "";
        public string TaskDescription { get; set; } = "";
        public string ReminderTime { get; private set; } = "";

        public TaskDialog()
        {
            InitializeComponent();

            // Enable/disable custom reminder textbox based on radio button
            CustomRadio.Checked += (s, e) => CustomReminderRichTextBox.IsEnabled = true;
            CustomRadio.Unchecked += (s, e) => CustomReminderRichTextBox.IsEnabled = false;
            
            // Clear placeholder text when focused and set up proper input
            SetupRichTextBox(TitleRichTextBox, "Enable two-factor authentication");
            SetupRichTextBox(DescriptionRichTextBox, "Set up two-factor authentication on all important accounts to add an extra layer of security.");
            SetupRichTextBox(CustomReminderRichTextBox, "e.g., in 2 weeks, next month, etc.");
            
            // Add Enter key support for submitting the task
            TitleRichTextBox.KeyDown += RichTextBox_KeyDown;
            DescriptionRichTextBox.KeyDown += RichTextBox_KeyDown;
            CustomReminderRichTextBox.KeyDown += RichTextBox_KeyDown;
            
            // Set focus to title field and make CREATE TASK button default
            TitleRichTextBox.Focus();
            AddButton.IsDefault = true;
        }
        
        private void RichTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Enter submits the task
                AddButton_Click(sender, new RoutedEventArgs());
                e.Handled = true;
            }
        }
        
        private void SetupRichTextBox(RichTextBox richTextBox, string placeholderText)
        {
            // Clear existing content and set up proper input
            richTextBox.Document.Blocks.Clear();
            richTextBox.Document.Blocks.Add(new Paragraph(new Run("")));
            
            // Handle focus events for placeholder behavior
            richTextBox.GotFocus += (s, e) =>
            {
                if (richTextBox.Document.Blocks.Count == 0 || 
                    (richTextBox.Document.Blocks.FirstBlock is Paragraph p && 
                     p.Inlines.Count == 0))
                {
                    richTextBox.Document.Blocks.Clear();
                    richTextBox.Document.Blocks.Add(new Paragraph(new Run("")));
                }
            };
            
            richTextBox.LostFocus += (s, e) =>
            {
                var text = GetRichTextBoxText(richTextBox);
                if (string.IsNullOrWhiteSpace(text))
                {
                    richTextBox.Document.Blocks.Clear();
                    richTextBox.Document.Blocks.Add(new Paragraph(new Run("")));
                }
            };
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var titleText = GetRichTextBoxText(TitleRichTextBox);
            if (string.IsNullOrWhiteSpace(titleText))
            {
                MessageBox.Show("Please enter a task title.", "Missing Information", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TaskTitle = titleText.Trim();
            TaskDescription = GetRichTextBoxText(DescriptionRichTextBox).Trim();
            
            var userProfile = UserProfile.LoadProfile();

            // Determine reminder time
            if (NoReminderRadio.IsChecked == true)
            {
                ReminderTime = "No reminder set";
            }
            else if (TomorrowRadio.IsChecked == true)
            {
                ReminderTime = "Reminder set for tomorrow";
            }
            else if (ThreeDaysRadio.IsChecked == true)
            {
                ReminderTime = "Reminder set for 3 days from now";
            }
            else if (OneWeekRadio.IsChecked == true)
            {
                ReminderTime = "Reminder set for 1 week from now";
            }
            else if (CustomRadio.IsChecked == true)
            {
                var customText = GetRichTextBoxText(CustomReminderRichTextBox).Trim();
                if (string.IsNullOrWhiteSpace(customText) || customText == "e.g., in 2 weeks, next month, etc.")
                {
                    MessageBox.Show("Please enter a custom reminder time.", "Missing Information", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                ReminderTime = $"Reminder set for: {customText}";
            }

            // Create Windows reminder if not "No reminder"
            if (!NoReminderRadio.IsChecked == true)
            {
                try
                {
                    var success = WindowsReminderService.CreateWindowsReminder(
                        TaskTitle, 
                        TaskDescription, 
                        ReminderTime, 
                        userProfile.Name);
                        
                    if (success)
                    {
                        var timeDesc = WindowsReminderService.GetReminderTimeDescription(ReminderTime);
                        MessageBox.Show(
                            $"✅ Task created successfully!\n\n" +
                            $"📝 Task: {TaskTitle}\n" +
                            $"⏰ Windows reminder set for: {timeDesc}\n\n" +
                            $"You will receive a notification when it's time to complete this task.", 
                            "Task Created", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(
                            $"✅ Task created successfully!\n\n" +
                            $"⚠️ Note: Could not create Windows reminder, but your task has been saved.", 
                            "Task Created", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"✅ Task created successfully!\n\n" +
                        $"⚠️ Note: Could not create Windows reminder: {ex.Message}", 
                        "Task Created", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show(
                    $"✅ Task created successfully!\n\n" +
                    $"📝 Task: {TaskTitle}\n" +
                    $"No reminder set.", 
                    "Task Created", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
            
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CreateReminderButton_Click(object sender, RoutedEventArgs e)
        {
            var titleText = GetRichTextBoxText(TitleRichTextBox);
            if (string.IsNullOrWhiteSpace(titleText))
            {
                MessageBox.Show("Please enter a task title first.", "Missing Information", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Get reminder time based on selection
            string reminderText = "";
            if (NoReminderRadio.IsChecked == true)
            {
                reminderText = "No reminder";
            }
            else if (TomorrowRadio.IsChecked == true)
            {
                reminderText = "Tomorrow";
            }
            else if (ThreeDaysRadio.IsChecked == true)
            {
                reminderText = "In 3 days";
            }
            else if (OneWeekRadio.IsChecked == true)
            {
                reminderText = "In 1 week";
            }
            else if (CustomRadio.IsChecked == true)
            {
                var customText = GetRichTextBoxText(CustomReminderRichTextBox).Trim();
                if (string.IsNullOrWhiteSpace(customText) || customText == "e.g., in 2 weeks, next month, etc.")
                {
                    MessageBox.Show("Please enter a custom reminder time.", "Missing Information", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                reminderText = customText;
            }

            // Create the reminder message
            var reminderMessage = $"⏰ CYBERSECURITY REMINDER ⏰\n\n" +
                                $"Task: {GetRichTextBoxText(TitleRichTextBox).Trim()}\n" +
                                $"Description: {GetRichTextBoxText(DescriptionRichTextBox).Trim()}\n" +
                                $"Reminder Time: {reminderText}\n\n" +
                                $"Don't forget to complete this important cybersecurity task!\n" +
                                $"Your digital security depends on staying vigilant.";

            // Show reminder details and allow user to copy or save
            var result = MessageBox.Show(
                $"Reminder created successfully!\n\n{reminderMessage}\n\n" +
                "Would you like to copy this reminder to your clipboard?", 
                "Reminder Created", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    System.Windows.Clipboard.SetText(reminderMessage);
                    MessageBox.Show("Reminder copied to clipboard! You can paste it into your calendar or reminder app.", 
                        "Copied to Clipboard", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch
                {
                    MessageBox.Show("Could not copy to clipboard, but your reminder has been created.", 
                        "Copy Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            // Log the reminder creation
            try
            {
                JsonDataService.SaveConversation(
                    $"Create reminder: {GetRichTextBoxText(TitleRichTextBox).Trim()}", 
                    $"Reminder created for '{GetRichTextBoxText(TitleRichTextBox).Trim()}' - {reminderText}", 
                    "reminder");
            }
            catch
            {
                // Silently handle logging errors
            }
        }

        private string GetRichTextBoxText(RichTextBox richTextBox)
        {
            return new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd).Text.Trim();
        }

        private void SetRichTextBoxText(RichTextBox richTextBox, string text)
        {
            richTextBox.Document.Blocks.Clear();
            if (!string.IsNullOrEmpty(text))
            {
                richTextBox.Document.Blocks.Add(new Paragraph(new Run(text)));
            }
        }
        
        // Method to populate dialog with existing task data for editing
        public void PopulateTaskData(string title, string description)
        {
            SetRichTextBoxText(TitleRichTextBox, title);
            SetRichTextBoxText(DescriptionRichTextBox, description);
            TaskTitle = title;
            TaskDescription = description;
        }
    }
}
