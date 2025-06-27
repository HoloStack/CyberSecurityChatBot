using System;
using System.Collections.Generic;
using System.Linq;

namespace CybersecurityChatbot
{
    public class ConsoleTaskManager
    {
        private List<TaskData> tasks;
        private Action<string> logActivity;
        private Action<string> botResponse;

        public ConsoleTaskManager(List<TaskData> tasksList, Action<string> logActivityCallback, Action<string> botResponseCallback)
        {
            tasks = tasksList;
            logActivity = logActivityCallback;
            botResponse = botResponseCallback;
        }

        public bool ContainsTaskCreation(string input)
        {
            var creationPatterns = new string[]
            {
                "set a task", "set task", "create task", "add task", "new task",
                "remind me to", "set reminder", "schedule task", "task for",
                "set a reminder", "make a task", "create reminder"
            };
            
            return creationPatterns.Any(pattern => input.Contains(pattern));
        }

        public bool ContainsAdvancedTaskCreation(string input)
        {
            // Pattern 1: "create a task in X time to Y"
            if (input.Contains("create a task in") && input.Contains(" to "))
                return true;
                
            // Pattern 2: "remind me in X time to Y"
            if (input.Contains("remind me in") && input.Contains(" to "))
                return true;
                
            // Pattern 3: "set a task for X time to Y"
            if (input.Contains("set a task for") && input.Contains(" to "))
                return true;
                
            // Pattern 4: "create task in X to Y"
            if (input.Contains("create task in") && input.Contains(" to "))
                return true;
                
            // Pattern 5: "add task in X to Y"
            if (input.Contains("add task in") && input.Contains(" to "))
                return true;
                
            // Pattern 6: "schedule task in X to Y"
            if (input.Contains("schedule task in") && input.Contains(" to "))
                return true;
                
            // Pattern 7: "set reminder in X to Y"
            if (input.Contains("set reminder in") && input.Contains(" to "))
                return true;
                
            // Pattern 8: "make a task in X to Y"
            if (input.Contains("make a task in") && input.Contains(" to "))
                return true;
                
            // Pattern 9: "set a reminder to X in Y" (IMPORTANT FIX)
            if (input.Contains("set a reminder to") && input.Contains(" in "))
                return true;
                
            // Pattern 10: "set reminder to X in Y"
            if (input.Contains("set reminder to") && input.Contains(" in "))
                return true;
            
            return false;
        }

        public bool ContainsTaskCompletion(string input)
        {
            var completionPatterns = new string[]
            {
                "mark as done", "mark as complete", "set to done", "set to complete",
                "mark done", "mark complete", "finished", "completed",
                "done with", "complete the", "finish the", "set done",
                "mark the", "set the", "complete task"
            };
            
            return completionPatterns.Any(pattern => input.Contains(pattern));
        }

        public void HandleAdvancedTaskCreation(string input)
        {
            try
            {
                var (timeExpression, taskDescription) = ParseAdvancedTaskCreationInput(input);
                
                if (string.IsNullOrEmpty(taskDescription))
                {
                    botResponse("I understand you want to create a task, but I couldn't determine what the task should be. Please try saying something like 'create a task in 10 minutes to enable 2FA' or 'remind me in 1 day to secure my network'.");
                    return;
                }
                
                if (string.IsNullOrEmpty(timeExpression))
                {
                    botResponse($"I understand you want to create a task '{taskDescription}', but when would you like to be reminded? You can say something like 'in 30 minutes', 'in 2 hours', 'tomorrow', or 'in 1 day and 3 hours'.");
                    return;
                }
                
                // Parse the time expression
                var reminderTime = ReminderService.ParseReminderTime(timeExpression);
                
                // Create the task
                var task = new TaskData
                {
                    Title = taskDescription,
                    Description = $"Cybersecurity task: {taskDescription}",
                    ReminderText = $"Reminder set for: {reminderTime:MMM dd, yyyy h:mm tt}",
                    CreatedDate = DateTime.Now,
                    Status = "Pending",
                    Priority = "Medium",
                    Category = "Cybersecurity",
                    DueDate = reminderTime
                };
                
                tasks.Add(task);
                
                // Add to reminder service
                ReminderService.AddReminder(taskDescription, $"Time to work on: {taskDescription}", reminderTime);
                
                botResponse($"✅ Perfect! I've created a task '{taskDescription}' with a reminder set for {reminderTime:MMM dd, yyyy} at {reminderTime:h:mm tt}.");
                botResponse($"📱 You'll receive both a popup notification and a Windows notification when it's time to complete this task.");
                logActivity($"Created task via natural language: '{taskDescription}' for {reminderTime}");
            }
            catch (Exception ex)
            {
                logActivity($"Error creating task via natural language: {ex.Message}");
                botResponse($"I had trouble creating that task: {ex.Message}. Please try rephrasing your request or use the basic 'add task [name]' command.");
            }
        }

        public (string title, string timing) ParseTaskCreationInput(string input)
        {
            string title = "";
            string timing = "";
            
            // Handle "set a task for X time to do Y" pattern
            if (input.Contains("set a task for") || input.Contains("set task for"))
            {
                var parts = input.Split(new[] { "to ", " to do ", " for " }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    // Extract timing (between "for" and "to")
                    var forIndex = input.IndexOf(" for ");
                    var toIndex = input.IndexOf(" to ", forIndex);
                    if (toIndex == -1) toIndex = input.IndexOf(" to do ", forIndex);
                    
                    if (forIndex != -1 && toIndex != -1)
                    {
                        timing = input.Substring(forIndex + 5, toIndex - forIndex - 5).Trim();
                        title = input.Substring(toIndex + 4).Trim();
                        if (title.StartsWith("do ")) title = title.Substring(3).Trim();
                    }
                }
            }
            // Handle "remind me to X in Y" pattern
            else if (input.Contains("remind me to"))
            {
                var reminderIndex = input.IndexOf("remind me to");
                var remainingText = input.Substring(reminderIndex + 12).Trim();
                
                var timingKeywords = new[] { " in ", " for ", " at ", " on ", " tomorrow", " today", " next " };
                foreach (var keyword in timingKeywords)
                {
                    var timingIndex = remainingText.IndexOf(keyword);
                    if (timingIndex != -1)
                    {
                        title = remainingText.Substring(0, timingIndex).Trim();
                        timing = remainingText.Substring(timingIndex + 1).Trim();
                        break;
                    }
                }
                
                if (string.IsNullOrEmpty(title))
                {
                    title = remainingText;
                    timing = "tomorrow";
                }
            }
            // Handle standard patterns
            else
            {
                foreach (var pattern in new[] { "add task", "create task", "new task", "set task" })
                {
                    if (input.Contains(pattern))
                    {
                        title = input.Replace(pattern, "").Trim();
                        break;
                    }
                }
            }
            
            return (title, timing);
        }

        private (string timeExpression, string taskDescription) ParseAdvancedTaskCreationInput(string input)
        {
            var lowerInput = input.ToLower();
            
            // Handle "set a reminder to X in Y" pattern specifically
            if (lowerInput.Contains("set a reminder to") || lowerInput.Contains("set reminder to"))
            {
                var reminderToIndex = lowerInput.IndexOf("reminder to");
                if (reminderToIndex != -1)
                {
                    var afterReminderTo = input.Substring(reminderToIndex + 11).Trim(); // "reminder to".Length = 11
                    var reminderInIndex = afterReminderTo.IndexOf(" in ");
                    
                    if (reminderInIndex != -1)
                    {
                        var reminderTaskDescription = afterReminderTo.Substring(0, reminderInIndex).Trim();
                        var reminderTimeExpression = afterReminderTo.Substring(reminderInIndex + 4).Trim(); // " in ".Length = 4
                        return (reminderTimeExpression, reminderTaskDescription);
                    }
                }
            }
            
            // Handle standard "X in Y to Z" patterns
            var inIndex = -1;
            var toIndex = -1;
            
            // Look for various patterns
            var inPatterns = new[] { " in ", "task in ", "reminder in " };
            var toPatterns = new[] { " to ", " to do ", " for " };
            
            foreach (var pattern in inPatterns)
            {
                var index = lowerInput.IndexOf(pattern);
                if (index != -1)
                {
                    inIndex = index + pattern.Length;
                    break;
                }
            }
            
            foreach (var pattern in toPatterns)
            {
                var index = lowerInput.IndexOf(pattern, inIndex > 0 ? inIndex : 0);
                if (index != -1)
                {
                    toIndex = index;
                    break;
                }
            }
            
            if (inIndex == -1 || toIndex == -1 || toIndex <= inIndex)
            {
                return ("", "");
            }
            
            // Extract time expression (between "in" and "to")
            var timeExpression = input.Substring(inIndex, toIndex - inIndex).Trim();
            
            // Extract task description (after "to")
            var taskStart = toIndex;
            while (taskStart < input.Length && (input[taskStart] == ' ' || input[taskStart] == 't' || input[taskStart] == 'o'))
            {
                taskStart++;
            }
            
            var taskDescription = "";
            if (taskStart < input.Length)
            {
                taskDescription = input.Substring(taskStart).Trim();
                
                // Clean up common prefixes
                if (taskDescription.StartsWith("do "))
                    taskDescription = taskDescription.Substring(3).Trim();
            }
            
            return (timeExpression, taskDescription);
        }

        public void AddTaskWithTiming(string title, string timing)
        {
            var task = new TaskData
            {
                Title = title,
                Description = $"Cybersecurity task: {title}",
                CreatedDate = DateTime.Now
            };
            
            // Set up timing if provided
            if (!string.IsNullOrEmpty(timing))
            {
                try
                {
                    var reminderTime = ReminderService.ParseReminderTime(timing);
                    task.ReminderText = $"Reminder set for: {reminderTime:MMM dd, yyyy h:mm tt}";
                    task.DueDate = reminderTime;
                    
                    // Add to the reminder service
                    ReminderService.AddReminder(title, $"Time to work on: {title}", reminderTime);
                    
                    botResponse($"✅ Task '{title}' created with reminder set for {reminderTime:MMM dd, yyyy} at {reminderTime:h:mm tt}!");
                    logActivity($"Added task with timing: {title} - {reminderTime}");
                }
                catch (Exception ex)
                {
                    task.ReminderText = "No reminder set";
                    botResponse($"✅ Task '{title}' created, but couldn't set reminder: {ex.Message}");
                    logActivity($"Added task without reminder: {title} - timing parse failed");
                }
            }
            else
            {
                task.ReminderText = "No reminder set";
                botResponse($"✅ Task '{title}' created successfully!");
                logActivity($"Added task: {title}");
            }
            
            tasks.Add(task);
        }

        public void HandleTaskCompletion(string input)
        {
            if (tasks.Count == 0)
            {
                botResponse("You don't have any tasks to complete.");
                return;
            }
            
            // Try to extract task keywords from the input
            var taskKeywords = ExtractTaskKeywords(input);
            
            if (taskKeywords.Count > 0)
            {
                var matchedTask = FindTaskByKeywords(taskKeywords);
                if (matchedTask != null)
                {
                    matchedTask.Status = "Completed";
                    matchedTask.IsCompleted = true;
                    matchedTask.CompletedDate = DateTime.Now;
                    
                    botResponse($"✅ Great job! Task '{matchedTask.Title}' has been marked as completed!");
                    logActivity($"Completed task via natural language: {matchedTask.Title}");
                    return;
                }
                else
                {
                    botResponse($"I couldn't find a task matching '{string.Join(" ", taskKeywords)}'. Here are your current tasks:");
                    ShowTasks();
                    return;
                }
            }
            
            // Fall back to standard completion method
            CompleteTask(input);
        }

        private List<string> ExtractTaskKeywords(string input)
        {
            var keywords = new List<string>();
            var taskKeywords = new[] { "2fa", "two-factor", "password", "backup", "update", "install", "enable", "disable", "configure", "setup", "vpn", "firewall", "antivirus", "encrypt", "scan", "patch" };
            
            foreach (var keyword in taskKeywords)
            {
                if (input.Contains(keyword))
                {
                    keywords.Add(keyword);
                }
            }
            
            return keywords;
        }

        private TaskData? FindTaskByKeywords(List<string> keywords)
        {
            foreach (var task in tasks)
            {
                var taskText = (task.Title + " " + task.Description).ToLower();
                if (keywords.Any(keyword => taskText.Contains(keyword.ToLower())))
                {
                    return task;
                }
            }
            return null;
        }

        public void AddTask(string title)
        {
            var task = new TaskData
            {
                Title = title,
                Description = $"Cybersecurity task: {title}",
                ReminderText = "No reminder set",
                CreatedDate = DateTime.Now
            };
            
            tasks.Add(task);
            botResponse($"✅ Task added: '{title}'. Would you like to set a reminder? (Type 'yes' or 'no')");
            logActivity($"Added task: {title}");
            
            // Simple reminder logic would be handled by the calling code
        }

        public void ShowTasks()
        {
            if (tasks.Count == 0)
            {
                botResponse("📝 You don't have any tasks yet. Use 'add task [description]' to create one!");
                return;
            }
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n📝 YOUR CYBERSECURITY TASKS:");
            Console.WriteLine("════════════════════════════");
            Console.ResetColor();
            
            for (int i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{i + 1}. {task.Title}");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"   Description: {task.Description}");
                Console.WriteLine($"   Created: {task.CreatedDate:yyyy-MM-dd HH:mm}");
                Console.WriteLine($"   {task.ReminderText}");
                Console.WriteLine();
            }
            Console.ResetColor();
            logActivity("Viewed task list");
        }

        public void CompleteTask(string input)
        {
            if (tasks.Count == 0)
            {
                botResponse("You don't have any tasks to complete.");
                return;
            }
            
            ShowTasks();
            botResponse("Which task number would you like to mark as complete?");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("👤 You: ");
            Console.ResetColor();
            
            if (int.TryParse(Console.ReadLine(), out int taskIndex) && taskIndex > 0 && taskIndex <= tasks.Count)
            {
                var completedTask = tasks[taskIndex - 1];
                tasks.RemoveAt(taskIndex - 1);
                botResponse($"✅ Completed task: '{completedTask.Title}'. Great work on improving your cybersecurity!");
                logActivity($"Completed task: {completedTask.Title}");
            }
            else
            {
                botResponse("Invalid task number. Please try again.");
            }
        }

        public void DeleteTask(string input)
        {
            if (tasks.Count == 0)
            {
                botResponse("You don't have any tasks to delete.");
                return;
            }
            
            ShowTasks();
            botResponse("Which task number would you like to delete?");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("👤 You: ");
            Console.ResetColor();
            
            if (int.TryParse(Console.ReadLine(), out int taskIndex) && taskIndex > 0 && taskIndex <= tasks.Count)
            {
                var deletedTask = tasks[taskIndex - 1];
                tasks.RemoveAt(taskIndex - 1);
                botResponse($"🗑️ Deleted task: '{deletedTask.Title}'.");
                logActivity($"Deleted task: {deletedTask.Title}");
            }
            else
            {
                botResponse("Invalid task number. Please try again.");
            }
        }
    }
}
