using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CybersecurityChatbot
{
    public partial class ActivityLogWindow : Window
    {
        public ActivityLogWindow(List<ActivityLogEntry> logs)
        {
            InitializeComponent();
            
            // Show recent logs (last 10) for clarity
            var recentLogs = logs.TakeLast(10).ToList();
            
            Title = $"Activity Log - {recentLogs.Count} Recent Actions";
            
            // Display logs in reverse chronological order (newest first)
            foreach (var log in recentLogs.AsEnumerable().Reverse())
            {
                LogListBox.Items.Add($"[{log.Timestamp}] {log.Action}");
            }
            
            // If there are more than 10 logs, show a note
            if (logs.Count > 10)
            {
                LogListBox.Items.Insert(0, $"--- Showing last 10 of {logs.Count} total activities ---");
            }
            
            // Add summary information
            if (recentLogs.Count > 0)
            {
                LogListBox.Items.Add("");
                LogListBox.Items.Add("📊 Summary:");
                
                var taskActions = recentLogs.Count(l => l.Action.Contains("task", System.StringComparison.OrdinalIgnoreCase));
                var quizActions = recentLogs.Count(l => l.Action.Contains("quiz", System.StringComparison.OrdinalIgnoreCase));
                var nlpActions = recentLogs.Count(l => l.Action.Contains("NLP", System.StringComparison.OrdinalIgnoreCase));
                
                if (taskActions > 0) LogListBox.Items.Add($"   📝 Task-related actions: {taskActions}");
                if (quizActions > 0) LogListBox.Items.Add($"   🎯 Quiz-related actions: {quizActions}");
                if (nlpActions > 0) LogListBox.Items.Add($"   🧠 NLP interactions: {nlpActions}");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
