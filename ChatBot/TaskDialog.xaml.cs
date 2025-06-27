using System;
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

            // Enable/disable quick options panel based on radio button
            QuickOptionsRadio.Checked += (s, e) => {
                QuickOptionsPanel.IsEnabled = true;
                ExactTimePanel.IsEnabled = false;
            };
            QuickOptionsRadio.Unchecked += (s, e) => QuickOptionsPanel.IsEnabled = false;
            
            // Enable/disable exact time panel based on radio button
            ExactTimeRadio.Checked += (s, e) => {
                ExactTimePanel.IsEnabled = true;
                QuickOptionsPanel.IsEnabled = false;
                UpdateSelectedTimeDisplay();
            };
            ExactTimeRadio.Unchecked += (s, e) => ExactTimePanel.IsEnabled = false;
            
            // No reminder radio button
            NoReminderRadio.Checked += (s, e) => {
                QuickOptionsPanel.IsEnabled = false;
                ExactTimePanel.IsEnabled = false;
            };
            
            // Clear placeholder text when focused and set up proper input
            SetupRichTextBox(TitleRichTextBox, "Enable two-factor authentication");
            SetupRichTextBox(DescriptionRichTextBox, "Set up two-factor authentication on all important accounts to add an extra layer of security.");
            
            // Add Enter key support for submitting the task
            TitleRichTextBox.KeyDown += RichTextBox_KeyDown;
            DescriptionRichTextBox.KeyDown += RichTextBox_KeyDown;
            
            // Set up date/time picker events
            ReminderDatePicker.SelectedDateChanged += (s, e) => UpdateSelectedTimeDisplay();
            HourComboBox.SelectionChanged += (s, e) => UpdateSelectedTimeDisplay();
            MinuteComboBox.SelectionChanged += (s, e) => UpdateSelectedTimeDisplay();
            AmPmComboBox.SelectionChanged += (s, e) => UpdateSelectedTimeDisplay();
            
            // Initialize date picker to tomorrow
            ReminderDatePicker.SelectedDate = DateTime.Today.AddDays(1);
            
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
            else if (QuickOptionsRadio.IsChecked == true)
            {
                if (QuickReminderComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    var tag = selectedItem.Tag.ToString();
                    switch (tag)
                    {
                        case "tomorrow_9am":
                            ReminderTime = "Reminder set for tomorrow at 9:00 AM";
                            break;
                        case "3days_9am":
                            ReminderTime = "Reminder set for 3 days from now at 9:00 AM";
                            break;
                        case "1week_9am":
                            ReminderTime = "Reminder set for 1 week from now at 9:00 AM";
                            break;
                        case "30seconds":
                            ReminderTime = "Reminder set for 30 seconds from now (testing)";
                            break;
                        default:
                            ReminderTime = "Reminder set for 3 days from now at 9:00 AM";
                            break;
                    }
                }
                else
                {
                    ReminderTime = "Reminder set for 3 days from now at 9:00 AM";
                }
            }
            else if (ExactTimeRadio.IsChecked == true)
            {
                var selectedDateTime = GetSelectedReminderDateTime();
                if (selectedDateTime.HasValue)
                {
                    if (selectedDateTime.Value <= DateTime.Now)
                    {
                        MessageBox.Show("Please select a future date and time for the reminder.", "Invalid Time", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    ReminderTime = $"Reminder set for {selectedDateTime.Value:dddd, MMMM dd, yyyy 'at' h:mm tt}";
                }
                else
                {
                    MessageBox.Show("Please select a valid date and time for the reminder.", "Missing Information", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            // Create reminder using the new ReminderService if not "No reminder"
            if (NoReminderRadio.IsChecked != true)
            {
                try
                {
                    DateTime reminderDateTime;
                    
                    if (QuickOptionsRadio.IsChecked == true)
                    {
                        string reminderInput = "";
                        if (QuickReminderComboBox.SelectedItem is ComboBoxItem selectedItem)
                        {
                            var tag = selectedItem.Tag.ToString();
                            switch (tag)
                            {
                                case "tomorrow_9am":
                                    reminderInput = "tomorrow 9:00 AM";
                                    break;
                                case "3days_9am":
                                    reminderInput = "in 3 days 9:00 AM";
                                    break;
                                case "1week_9am":
                                    reminderInput = "in 1 week 9:00 AM";
                                    break;
                                case "30seconds":
                                    reminderInput = "in 30 seconds";
                                    break;
                                default:
                                    reminderInput = "in 3 days 9:00 AM";
                                    break;
                            }
                        }
                        else
                        {
                            reminderInput = "in 3 days 9:00 AM";
                        }
                        reminderDateTime = ReminderService.ParseReminderTime(reminderInput);
                    }
                    else if (ExactTimeRadio.IsChecked == true)
                    {
                        var selectedDateTime = GetSelectedReminderDateTime();
                        if (selectedDateTime.HasValue)
                        {
                            reminderDateTime = selectedDateTime.Value;
                        }
                        else
                        {
                            throw new Exception("No valid date/time selected");
                        }
                    }
                    else
                    {
                        // Fallback to default
                        reminderDateTime = ReminderService.ParseReminderTime("in 3 days 9:00 AM");
                    }
                    
                    ReminderService.AddReminder(TaskTitle, $"Time to work on: {TaskTitle}", reminderDateTime);
                    
                    MessageBox.Show(
                        $"✅ Task created successfully!\n\n" +
                        $"📝 Task: {TaskTitle}\n" +
                        $"⏰ Background reminder set for: {reminderDateTime:MMM dd, yyyy h:mm tt}\n\n" +
                        $"You will receive a modern popup notification when it's time to complete this task.", 
                        "Task Created", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"✅ Task created successfully!\n\n" +
                        $"⚠️ Note: Could not create background reminder: {ex.Message}\n" +
                        $"Your task has been saved without a reminder.", 
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
            else if (QuickOptionsRadio.IsChecked == true)
            {
                if (QuickReminderComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    var tag = selectedItem.Tag.ToString();
                    switch (tag)
                    {
                        case "tomorrow_9am":
                            reminderText = "Tomorrow at 9:00 AM";
                            break;
                        case "3days_9am":
                            reminderText = "In 3 days at 9:00 AM";
                            break;
                        case "1week_9am":
                            reminderText = "In 1 week at 9:00 AM";
                            break;
                        case "30seconds":
                            reminderText = "In 30 seconds (testing)";
                            break;
                        default:
                            reminderText = "In 3 days at 9:00 AM";
                            break;
                    }
                }
                else
                {
                    reminderText = "In 3 days at 9:00 AM";
                }
            }
            else if (ExactTimeRadio.IsChecked == true)
            {
                var selectedDateTime = GetSelectedReminderDateTime();
                if (selectedDateTime.HasValue)
                {
                    reminderText = selectedDateTime.Value.ToString("dddd, MMMM dd, yyyy 'at' h:mm tt");
                }
                else
                {
                    MessageBox.Show("Please select a valid date and time for the reminder.", "Missing Information", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
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

        private void UpdateSelectedTimeDisplay()
        {
            if (ReminderDatePicker.SelectedDate.HasValue && 
                HourComboBox.SelectedItem is ComboBoxItem hourItem &&
                MinuteComboBox.SelectedItem is ComboBoxItem minuteItem &&
                AmPmComboBox.SelectedItem is ComboBoxItem ampmItem)
            {
                var date = ReminderDatePicker.SelectedDate.Value;
                var hour = int.Parse(hourItem.Tag.ToString());
                var minute = int.Parse(minuteItem.Tag.ToString());
                var ampm = ampmItem.Tag.ToString();
                
                // Convert to 24-hour format
                if (ampm == "PM" && hour != 12)
                    hour += 12;
                else if (ampm == "AM" && hour == 12)
                    hour = 0;
                
                var reminderDateTime = new DateTime(date.Year, date.Month, date.Day, hour, minute, 0);
                SelectedTimeDisplay.Text = $"Selected: {reminderDateTime:dddd, MMMM dd, yyyy 'at' h:mm tt}";
            }
            else
            {
                SelectedTimeDisplay.Text = "Selected: (No date/time selected)";
            }
        }
        
        private DateTime? GetSelectedReminderDateTime()
        {
            if (ReminderDatePicker.SelectedDate.HasValue && 
                HourComboBox.SelectedItem is ComboBoxItem hourItem &&
                MinuteComboBox.SelectedItem is ComboBoxItem minuteItem &&
                AmPmComboBox.SelectedItem is ComboBoxItem ampmItem)
            {
                var date = ReminderDatePicker.SelectedDate.Value;
                var hour = int.Parse(hourItem.Tag.ToString());
                var minute = int.Parse(minuteItem.Tag.ToString());
                var ampm = ampmItem.Tag.ToString();
                
                // Convert to 24-hour format
                if (ampm == "PM" && hour != 12)
                    hour += 12;
                else if (ampm == "AM" && hour == 12)
                    hour = 0;
                
                return new DateTime(date.Year, date.Month, date.Day, hour, minute, 0);
            }
            return null;
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
