using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CybersecurityChatbot
{
    public partial class TasksWindow : Window
    {
        public event Action TasksUpdated = delegate { };
        private ObservableCollection<TaskData> taskCollection;
        private List<TaskData> originalTasks;
        private ChatbotEngine engine;

        public TasksWindow(List<TaskData> tasks, ChatbotEngine engine)
        {
            InitializeComponent();
            this.originalTasks = tasks;
            this.engine = engine;
            
            // Create observable collection for data binding
            taskCollection = new ObservableCollection<TaskData>(tasks);
            TaskListView.ItemsSource = taskCollection;
            
            // Update button states
            UpdateButtonStates();
            
            // Handle selection changes
            TaskListView.SelectionChanged += TaskListView_SelectionChanged;
        }
        
        private void TaskListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtonStates();
        }
        
        private void UpdateButtonStates()
        {
            bool hasSelection = TaskListView.SelectedItem != null;
            EditTaskButton.IsEnabled = hasSelection;
            CompleteTaskButton.IsEnabled = hasSelection && 
                ((TaskData)TaskListView.SelectedItem)?.Status != "Completed";
            DeleteTaskButton.IsEnabled = hasSelection;
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            var taskDialog = new TaskDialog();
            taskDialog.Owner = this;
            
            if (taskDialog.ShowDialog() == true)
            {
                var newTask = new TaskData
                {
                    Title = taskDialog.TaskTitle,
                    Description = taskDialog.TaskDescription,
                    ReminderText = taskDialog.ReminderTime,
                    CreatedDate = DateTime.Now,
                    Status = "Pending",
                    Priority = "Medium",
                    Category = "Cybersecurity"
                };
                
                taskCollection.Add(newTask);
                originalTasks.Add(newTask);
                
                // Save to JSON
                SaveTasksToJson();
                
                MessageBox.Show($"Task '{newTask.Title}' has been added successfully!", 
                    "Task Added", MessageBoxButton.OK, MessageBoxImage.Information);
                
                TasksUpdated?.Invoke();
            }
        }

        private void EditTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListView.SelectedItem is TaskData selectedTask)
            {
                var taskDialog = new TaskDialog();
                taskDialog.Owner = this;
                
                // Pre-populate the dialog with existing task data
                taskDialog.PopulateTaskData(selectedTask.Title, selectedTask.Description);
                
                if (taskDialog.ShowDialog() == true)
                {
                    selectedTask.Title = taskDialog.TaskTitle;
                    selectedTask.Description = taskDialog.TaskDescription;
                    selectedTask.ReminderText = taskDialog.ReminderTime;
                    
                    // Refresh the ListView
                    TaskListView.Items.Refresh();
                    
                    // Save to JSON
                    SaveTasksToJson();
                    
                    MessageBox.Show($"Task '{selectedTask.Title}' has been updated successfully!", 
                        "Task Updated", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    TasksUpdated?.Invoke();
                }
            }
        }

        private void CompleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListView.SelectedItem is TaskData selectedTask)
            {
                var result = MessageBox.Show(
                    $"Mark task '{selectedTask.Title}' as completed?\n\n" +
                    "This will change the task status to 'Completed' and set the completion date.", 
                    "Complete Task", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    selectedTask.Status = "Completed";
                    selectedTask.CompletedDate = DateTime.Now;
                    
                    // Refresh the ListView
                    TaskListView.Items.Refresh();
                    UpdateButtonStates();
                    
                    // Save to JSON
                    SaveTasksToJson();
                    
                    MessageBox.Show(
                        $"🎉 Congratulations! Task '{selectedTask.Title}' completed!\n\n" +
                        "Great job on improving your cybersecurity posture!", 
                        "Task Completed", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                    
                    TasksUpdated?.Invoke();
                }
            }
        }

        private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListView.SelectedItem is TaskData selectedTask)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the task '{selectedTask.Title}'?\n\n" +
                    "This action cannot be undone.", 
                    "Delete Task", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    taskCollection.Remove(selectedTask);
                    originalTasks.Remove(selectedTask);
                    
                    // Save to JSON
                    SaveTasksToJson();
                    
                    MessageBox.Show(
                        $"Task '{selectedTask.Title}' has been deleted.", 
                        "Task Deleted", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                    
                    TasksUpdated?.Invoke();
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Save any pending changes
            SaveTasksToJson();
            
            // Trigger the event to update task count in main window
            TasksUpdated?.Invoke();
            Close();
        }
        
        private void SaveTasksToJson()
        {
            try
            {
                // Use JsonDataService to save tasks
                JsonDataService.SaveTasks(originalTasks);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving tasks: {ex.Message}", 
                    "Save Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        // Method to refresh task display (can be called externally)
        public void RefreshTasks()
        {
            taskCollection.Clear();
            foreach (var task in originalTasks)
            {
                taskCollection.Add(task);
            }
        }
        
        // Method to get task statistics
        public (int total, int pending, int completed) GetTaskStatistics()
        {
            int total = originalTasks.Count;
            int completed = originalTasks.Count(t => t.Status == "Completed");
            int pending = total - completed;
            
            return (total, pending, completed);
        }
    }
}
