using System;

namespace CybersecurityChatbot
{
    public class TaskData
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string ReminderText { get; set; } = "";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? CompletedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; } = "Pending";
        public string Priority { get; set; } = "Medium";
        public string Category { get; set; } = "Cybersecurity";
        public bool IsCompleted { get; set; } = false;
    }
}
