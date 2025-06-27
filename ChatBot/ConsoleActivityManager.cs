using System;
using System.Collections.Generic;
using System.Linq;

namespace CybersecurityChatbot
{
    public class ConsoleActivityManager
    {
        private List<ActivityLogEntry> activityLog;
        private Action<string> botResponse;

        public ConsoleActivityManager(List<ActivityLogEntry> activityLogList, Action<string> botResponseCallback)
        {
            activityLog = activityLogList;
            botResponse = botResponseCallback;
        }

        public void LogActivity(string action)
        {
            activityLog.Add(new ActivityLogEntry
            {
                Action = action,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        public void ShowActivityLog()
        {
            if (activityLog.Count == 0)
            {
                botResponse("📊 No activity recorded yet. Start using features to see your activity log!");
                return;
            }
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n📊 RECENT ACTIVITY LOG:");
            Console.WriteLine("═══════════════════════");
            Console.ResetColor();
            
            var recentLogs = activityLog.TakeLast(10).ToList();
            foreach (var log in recentLogs)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"[{log.Timestamp}] {log.Action}");
            }
            Console.ResetColor();
            
            LogActivity("Viewed activity log");
        }

        public void ClearActivityLog()
        {
            activityLog.Clear();
            botResponse("🗑️ Activity log cleared.");
            LogActivity("Activity log cleared");
        }

        public int GetLogCount()
        {
            return activityLog.Count;
        }

        public List<ActivityLogEntry> GetRecentLogs(int count = 10)
        {
            return activityLog.TakeLast(count).ToList();
        }
        
        public bool ShouldProcessActivityLog(string input)
        {
            return input.Contains("activity log") || input.Contains("show activity") || input.Contains("show log") ||
                   input.Contains("view activity") || input.Contains("view log") || input.Contains("my activity") ||
                   input == "activity" || input == "log" || input == "history" ||
                   input.Contains("show history") || input.Contains("view history");
        }
        
        public bool ShouldClearLog(string input)
        {
            return input.Contains("clear log");
        }
    }

    public class ActivityLogEntry
    {
        public string Action { get; set; } = "";
        public string Timestamp { get; set; } = "";
    }
}
