using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace CybersecurityChatbot
{
    public class ReminderService
    {
        private static readonly string ReminderFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CybersecurityChatbot", "reminders.json");
        private static readonly string LogFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CybersecurityChatbot", "reminder_service.log");
        private static Timer? _reminderTimer;
        private static bool _isRunning = false;

        public static void StartService()
        {
            if (_isRunning) return;

            try
            {
                EnsureDirectoryExists();
                LogMessage("Reminder service starting...");

                // Check every minute for due reminders
                _reminderTimer = new Timer(CheckReminders, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
                _isRunning = true;

                LogMessage("Reminder service started successfully");
            }
            catch (Exception ex)
            {
                LogMessage($"Error starting reminder service: {ex.Message}");
            }
        }

        public static void StopService()
        {
            try
            {
                _reminderTimer?.Dispose();
                _isRunning = false;
                LogMessage("Reminder service stopped");
            }
            catch (Exception ex)
            {
                LogMessage($"Error stopping reminder service: {ex.Message}");
            }
        }

        public static void AddReminder(string taskTitle, string reminderText, DateTime reminderTime)
        {
            try
            {
                var reminders = LoadReminders();
                var reminder = new ReminderData
                {
                    Id = Guid.NewGuid().ToString(),
                    TaskTitle = taskTitle,
                    ReminderText = reminderText,
                    ReminderTime = reminderTime,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                reminders.Add(reminder);
                SaveReminders(reminders);

                LogMessage($"Added reminder: {taskTitle} for {reminderTime}");

                // Also add to Windows Task Scheduler for redundancy
                ScheduleWindowsReminder(reminder);
            }
            catch (Exception ex)
            {
                LogMessage($"Error adding reminder: {ex.Message}");
            }
        }

        public static DateTime ParseReminderTime(string reminderText)
        {
            var baseTime = DateTime.Now;
            string lowerText = reminderText.ToLower().Trim();

            try
            {
                // Handle specific times like "3:30 PM tomorrow"
                if (lowerText.Contains("tomorrow"))
                {
                    var tomorrow = baseTime.AddDays(1);
                    if (lowerText.Contains("morning")) return tomorrow.Date.AddHours(9);
                    if (lowerText.Contains("afternoon")) return tomorrow.Date.AddHours(14);
                    if (lowerText.Contains("evening")) return tomorrow.Date.AddHours(18);
                    return tomorrow.Date.AddHours(9); // Default to 9 AM
                }

                // Handle complex time expressions like "1 day and 2 hours and 30 minutes"
                if (lowerText.StartsWith("in ") || lowerText.Contains(" and "))
                {
                    return ParseComplexTimeExpression(lowerText, baseTime);
                }

                // Handle time expressions - check in order of specificity
                if (lowerText.Contains("second"))
                {
                    var seconds = ExtractNumber(lowerText);
                    return baseTime.AddSeconds(seconds > 0 ? seconds : 30);
                }
                if (lowerText.Contains("minute"))
                {
                    var minutes = ExtractNumber(lowerText);
                    return baseTime.AddMinutes(minutes > 0 ? minutes : 5);
                }
                if (lowerText.Contains("hour"))
                {
                    var hours = ExtractNumber(lowerText);
                    return baseTime.AddHours(hours > 0 ? hours : 1);
                }
                if (lowerText.Contains(" day") || lowerText.StartsWith("day") || lowerText.EndsWith(" day") || lowerText == "day")
                {
                    var days = ExtractNumber(lowerText);
                    return baseTime.AddDays(days > 0 ? days : 1).Date.AddHours(9);
                }
                if (lowerText.Contains(" week") || lowerText.StartsWith("week") || lowerText.EndsWith(" week") || lowerText == "week")
                {
                    var weeks = ExtractNumber(lowerText);
                    return baseTime.AddDays((weeks > 0 ? weeks : 1) * 7).Date.AddHours(9);
                }
                
                // Handle "in X" patterns as fallback
                if (lowerText.StartsWith("in "))
                {
                    // This is a fallback for "in X" patterns that didn't match above
                    // Try to extract any numbers and assume it's minutes as default
                    var number = ExtractNumber(lowerText);
                    if (number > 0)
                    {
                        return baseTime.AddMinutes(number);
                    }
                }

                // Handle day names
                if (lowerText.Contains("monday")) return GetNextWeekday(DayOfWeek.Monday, baseTime);
                if (lowerText.Contains("tuesday")) return GetNextWeekday(DayOfWeek.Tuesday, baseTime);
                if (lowerText.Contains("wednesday")) return GetNextWeekday(DayOfWeek.Wednesday, baseTime);
                if (lowerText.Contains("thursday")) return GetNextWeekday(DayOfWeek.Thursday, baseTime);
                if (lowerText.Contains("friday")) return GetNextWeekday(DayOfWeek.Friday, baseTime);
                if (lowerText.Contains("saturday")) return GetNextWeekday(DayOfWeek.Saturday, baseTime);
                if (lowerText.Contains("sunday")) return GetNextWeekday(DayOfWeek.Sunday, baseTime);

                // Handle relative terms
                if (lowerText.Contains("next week")) return baseTime.AddDays(7).Date.AddHours(9);
                if (lowerText.Contains("tonight")) return baseTime.Date.AddHours(20);
                if (lowerText.Contains("later today")) return baseTime.AddHours(4);

                // Try to parse as absolute datetime
                if (DateTime.TryParse(reminderText, out DateTime parsedTime))
                {
                    return parsedTime;
                }

                // Default: 1 day from now at 9 AM
                return baseTime.AddDays(1).Date.AddHours(9);
            }
            catch
            {
                // Fallback: 1 hour from now
                return baseTime.AddHours(1);
            }
        }

        private static int ExtractNumber(string text)
        {
            var words = text.Split(' ');
            foreach (var word in words)
            {
                if (int.TryParse(word, out int number))
                    return number;
                
                // Handle written numbers
                switch (word.ToLower())
                {
                    case "one": return 1;
                    case "two": return 2;
                    case "three": return 3;
                    case "four": return 4;
                    case "five": return 5;
                    case "six": return 6;
                    case "seven": return 7;
                    case "eight": return 8;
                    case "nine": return 9;
                    case "ten": return 10;
                    case "fifteen": return 15;
                    case "twenty": return 20;
                    case "thirty": return 30;
                    default: continue;
                }
            }
            return 0;
        }

        private static DateTime ParseComplexTimeExpression(string lowerText, DateTime baseTime)
        {
            // Remove "in " from the beginning if present
            if (lowerText.StartsWith("in "))
                lowerText = lowerText.Substring(3).Trim();
            
            // Initialize time components
            int totalSeconds = 0;
            int totalMinutes = 0;
            int totalHours = 0;
            int totalDays = 0;
            int totalWeeks = 0;
            
            // Split by "and" to handle complex expressions
            var parts = lowerText.Split(new[] { " and ", ", ", "," }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var part in parts)
            {
                var trimmedPart = part.Trim();
                
                // Extract number and unit from each part
                var words = trimmedPart.Split(' ');
                for (int i = 0; i < words.Length - 1; i++)
                {
                    if (int.TryParse(words[i], out int number) || TryParseWrittenNumber(words[i], out number))
                    {
                        var unit = words[i + 1].ToLower();
                        
                        if (unit.StartsWith("second"))
                            totalSeconds += number;
                        else if (unit.StartsWith("minute"))
                            totalMinutes += number;
                        else if (unit.StartsWith("hour"))
                            totalHours += number;
                        else if (unit.StartsWith("day"))
                            totalDays += number;
                        else if (unit.StartsWith("week"))
                            totalWeeks += number;
                    }
                }
            }
            
            // Apply all time additions
            var result = baseTime;
            result = result.AddSeconds(totalSeconds);
            result = result.AddMinutes(totalMinutes);
            result = result.AddHours(totalHours);
            result = result.AddDays(totalDays);
            result = result.AddDays(totalWeeks * 7);
            
            return result;
        }
        
        private static bool TryParseWrittenNumber(string word, out int number)
        {
            number = 0;
            switch (word.ToLower())
            {
                case "one": number = 1; return true;
                case "two": number = 2; return true;
                case "three": number = 3; return true;
                case "four": number = 4; return true;
                case "five": number = 5; return true;
                case "six": number = 6; return true;
                case "seven": number = 7; return true;
                case "eight": number = 8; return true;
                case "nine": number = 9; return true;
                case "ten": number = 10; return true;
                case "eleven": number = 11; return true;
                case "twelve": number = 12; return true;
                case "fifteen": number = 15; return true;
                case "twenty": number = 20; return true;
                case "thirty": number = 30; return true;
                case "forty": number = 40; return true;
                case "fifty": number = 50; return true;
                case "sixty": number = 60; return true;
                default: return false;
            }
        }
        
        private static DateTime GetNextWeekday(DayOfWeek targetDay, DateTime baseTime)
        {
            int daysUntilTarget = ((int)targetDay - (int)baseTime.DayOfWeek + 7) % 7;
            if (daysUntilTarget == 0) daysUntilTarget = 7; // Next week if it's the same day
            return baseTime.AddDays(daysUntilTarget).Date.AddHours(9);
        }

        private static void CheckReminders(object state)
        {
            try
            {
                var reminders = LoadReminders();
                var dueReminders = reminders.Where(r => r.IsActive && r.ReminderTime <= DateTime.Now).ToList();

                foreach (var reminder in dueReminders)
                {
                    ShowReminderPopup(reminder);
                    
                    // Mark as inactive
                    reminder.IsActive = false;
                    LogMessage($"Triggered reminder: {reminder.TaskTitle}");
                }

                if (dueReminders.Any())
                {
                    SaveReminders(reminders);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error checking reminders: {ex.Message}");
            }
        }

        private static void ShowReminderPopup(ReminderData reminder)
        {
            try
            {
                // Show popup on UI thread
                Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    var popup = new ReminderPopupWindow(reminder);
                    popup.Show();
                }));
            }
            catch (Exception ex)
            {
                LogMessage($"Error showing reminder popup: {ex.Message}");
                
                // Fallback: Windows notification
                WindowsReminderService.ShowNotification(
                    "🛡️ Cybersecurity Task Reminder",
                    $"Task: {reminder.TaskTitle}\n{reminder.ReminderText}"
                );
            }
        }

        private static void ScheduleWindowsReminder(ReminderData reminder)
        {
            try
            {
                // Create a simple scheduled task using Windows notifications as backup
                var delay = reminder.ReminderTime - DateTime.Now;
                if (delay.TotalMilliseconds > 0)
                {
                    Task.Delay(delay).ContinueWith(_ =>
                    {
                        WindowsReminderService.ShowNotification(
                            "🛡️ Cybersecurity Task Reminder",
                            $"Task: {reminder.TaskTitle}\n{reminder.ReminderText}"
                        );
                    });
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error scheduling Windows reminder: {ex.Message}");
            }
        }

        public static void SetupAutoStart()
        {
            try
            {
                var executablePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                
                if (key != null)
                {
                    key.SetValue("CybersecurityChatbotReminder", $"\"{executablePath}\" --background");
                    key.Close();
                    LogMessage("Auto-start configured successfully");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error setting up auto-start: {ex.Message}");
            }
        }

        public static void RemoveAutoStart()
        {
            try
            {
                var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (key != null)
                {
                    key.DeleteValue("CybersecurityChatbotReminder", false);
                    key.Close();
                    LogMessage("Auto-start removed successfully");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error removing auto-start: {ex.Message}");
            }
        }

        private static List<ReminderData> LoadReminders()
        {
            try
            {
                if (!File.Exists(ReminderFilePath))
                    return new List<ReminderData>();

                var json = File.ReadAllText(ReminderFilePath);
                return JsonSerializer.Deserialize<List<ReminderData>>(json) ?? new List<ReminderData>();
            }
            catch (Exception ex)
            {
                LogMessage($"Error loading reminders: {ex.Message}");
                return new List<ReminderData>();
            }
        }

        private static void SaveReminders(List<ReminderData> reminders)
        {
            try
            {
                EnsureDirectoryExists();
                var json = JsonSerializer.Serialize(reminders, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ReminderFilePath, json);
            }
            catch (Exception ex)
            {
                LogMessage($"Error saving reminders: {ex.Message}");
            }
        }

        private static void EnsureDirectoryExists()
        {
            var directory = Path.GetDirectoryName(ReminderFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static void LogMessage(string message)
        {
            try
            {
                EnsureDirectoryExists();
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
                File.AppendAllText(LogFilePath, logEntry);
            }
            catch
            {
                // Ignore logging errors
            }
        }

        public static bool IsRunning => _isRunning;
    }

    public class ReminderData
    {
        public string Id { get; set; } = "";
        public string TaskTitle { get; set; } = "";
        public string ReminderText { get; set; } = "";
        public DateTime ReminderTime { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
