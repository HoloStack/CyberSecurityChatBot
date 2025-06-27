using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CybersecurityChatbot
{
    public static class WindowsReminderService
    {
        public static bool CreateWindowsReminder(string taskTitle, string description, string reminderTime, string userName)
        {
            try
            {
                var delayMinutes = ParseReminderTime(reminderTime);
                if (delayMinutes <= 0)
                {
                    return false;
                }

                var reminderMessage = $"🛡️ CYBERSECURITY REMINDER for {userName}!\n\n" +
                                    $"Task: {taskTitle}\n" +
                                    $"Description: {description}\n\n" +
                                    $"Time to take action on your cybersecurity task!";

                // Create a PowerShell script to show the reminder
                var script = CreateReminderScript(reminderMessage, delayMinutes);
                
                // Execute the script with better error handling
                var scriptPath = Path.Combine(Path.GetTempPath(), $"reminder_{DateTime.Now.Ticks}.ps1");
                
                // Ensure we can write to temp directory
                if (!Directory.Exists(Path.GetTempPath()))
                {
                    return false;
                }
                
                File.WriteAllText(scriptPath, script, Encoding.UTF8);

                var processInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-WindowStyle Hidden -ExecutionPolicy Bypass -File \"{scriptPath}\"",
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false
                };

                using (var process = Process.Start(processInfo))
                {
                    // Don't wait for the process to complete since it has a delay
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating Windows reminder: {ex.Message}");
                return false;
            }
        }

        private static string CreateReminderScript(string message, int delayMinutes)
        {
            var script = new StringBuilder();
            script.AppendLine("# Cybersecurity Task Reminder Script");
            script.AppendLine($"Start-Sleep -Seconds {delayMinutes * 60}");
            script.AppendLine();
            script.AppendLine("# Show notification");
            script.AppendLine("Add-Type -AssemblyName System.Windows.Forms");
            script.AppendLine("Add-Type -AssemblyName System.Drawing");
            script.AppendLine();
            script.AppendLine("$notification = New-Object System.Windows.Forms.NotifyIcon");
            script.AppendLine("$notification.Icon = [System.Drawing.SystemIcons]::Information");
            script.AppendLine("$notification.BalloonTipIcon = [System.Windows.Forms.ToolTipIcon]::Info");
            script.AppendLine("$notification.BalloonTipTitle = \"🛡️ Cybersecurity Task Reminder\"");
            script.AppendLine($"$notification.BalloonTipText = @\"\n{message.Replace("\"", "`\"")}\n\"@");
            script.AppendLine("$notification.Visible = $true");
            script.AppendLine("$notification.ShowBalloonTip(10000)");
            script.AppendLine();
            script.AppendLine("# Also show message box");
            script.AppendLine("Add-Type -AssemblyName PresentationFramework");
            script.AppendLine($"[System.Windows.MessageBox]::Show(@\"\n{message.Replace("\"", "`\"")}\n\"@, \"🛡️ Cybersecurity Task Reminder\", \"OK\", \"Information\")");
            script.AppendLine();
            script.AppendLine("# Cleanup");
            script.AppendLine("$notification.Dispose()");
            script.AppendLine($"Remove-Item \"{Path.Combine(Path.GetTempPath(), $"reminder_*.ps1")}\" -Force -ErrorAction SilentlyContinue");

            return script.ToString();
        }

        private static int ParseReminderTime(string reminderTime)
        {
            if (string.IsNullOrWhiteSpace(reminderTime))
                return 0;

            reminderTime = reminderTime.ToLower().Trim();

            // Handle predefined options
            if (reminderTime.Contains("tomorrow"))
                return 24 * 60; // 24 hours in minutes
            if (reminderTime.Contains("3 days"))
                return 3 * 24 * 60; // 3 days in minutes
            if (reminderTime.Contains("1 week") || reminderTime.Contains("week"))
                return 7 * 24 * 60; // 1 week in minutes

            // Parse custom time formats
            if (reminderTime.Contains("minute"))
            {
                var parts = reminderTime.Split(' ');
                foreach (var part in parts)
                {
                    if (int.TryParse(part, out int minutes))
                        return minutes;
                }
                return 5; // default 5 minutes
            }

            if (reminderTime.Contains("hour"))
            {
                var parts = reminderTime.Split(' ');
                foreach (var part in parts)
                {
                    if (int.TryParse(part, out int hours))
                        return hours * 60;
                }
                return 60; // default 1 hour
            }

            if (reminderTime.Contains("day"))
            {
                var parts = reminderTime.Split(' ');
                foreach (var part in parts)
                {
                    if (int.TryParse(part, out int days))
                        return days * 24 * 60;
                }
                return 24 * 60; // default 1 day
            }

            // Try to extract any number and assume minutes
            var words = reminderTime.Split(' ');
            foreach (var word in words)
            {
                if (int.TryParse(word, out int value))
                {
                    // If no unit specified, assume minutes for small numbers, hours for larger
                    return value <= 60 ? value : value * 60;
                }
            }

            // Default to 1 hour if nothing else matches
            return 60;
        }

        public static string GetReminderTimeDescription(string reminderTime)
        {
            var minutes = ParseReminderTime(reminderTime);
            
            if (minutes <= 0)
                return "Invalid time";
            
            if (minutes < 60)
                return $"{minutes} minute(s)";
            
            if (minutes < 24 * 60)
            {
                var hours = minutes / 60;
                var remainingMinutes = minutes % 60;
                return remainingMinutes > 0 ? $"{hours} hour(s) and {remainingMinutes} minute(s)" : $"{hours} hour(s)";
            }
            
            var days = minutes / (24 * 60);
            var remainingHours = (minutes % (24 * 60)) / 60;
            return remainingHours > 0 ? $"{days} day(s) and {remainingHours} hour(s)" : $"{days} day(s)";
        }
    }
}
