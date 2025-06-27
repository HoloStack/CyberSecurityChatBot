# 🛡️ Cybersecurity Awareness Chatbot

A sophisticated AI-powered cybersecurity education platform featuring advanced Natural Language Processing (NLP) capabilities, intelligent task management, and interactive learning modules.

### 🔐 **Important Note on API Key Configuration**

The chatbot includes ChatGPT integration, which relies on an OpenAI API key.
For security best practices, the API key is stored in a separate configuration file that is **not** tracked by Git.

**To enable ChatGPT functionality:**

1. **Copy the example configuration file:**
   ```bash
   cp config.example.json config.json
   ```

2. **Edit `config.json` and replace the placeholder:**
   ```json
   {
     "OpenAI": {
       "ApiKey": "your-actual-openai-api-key-here"
     }
   }
   ```

3. **The `config.json` file is automatically ignored by Git** (listed in `.gitignore`)

4. **For submission:** Include the API key in a separate secure document, **never commit it to version control**

**Security Features:**
- ✅ API key stored in separate config file
- ✅ Config file ignored by Git (.gitignore)
- ✅ Template file provided for easy setup
- ✅ Graceful degradation when API key is missing
- ✅ Activity logging of API key loading status

Without the API key, ChatGPT features will be disabled, but all other chatbot features remain fully functional.

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 or later
- Windows OS (for notification features and audio greeting)
- Audio output device (speakers/headphones) for greeting sound
- Visual Studio 2022 or VS Code (optional)

### Installation & Running
```bash
# Clone and navigate to the project
cd ChatBot

# Build the application
dotnet build

# Run GUI version (default) - plays greeting.wav on startup
dotnet run

# Run console version - also plays greeting.wav on startup
dotnet run --console
```

### 🔊 **Audio Features**
- **Greeting Sound**: Both GUI and console versions play `Audio/greetings.wav` on startup
- **Fallback Audio**: If greetings.wav is missing, system sounds are used as fallback
- **Audio Location**: All audio files are organized in the `Audio/` folder
- **Custom Audio**: Replace `greetings.wav` with your own welcome message

### ⏰ **Advanced Reminder System**

This application features a sophisticated background reminder service that runs even when the app is closed:

#### **Key Features:**
- **🔄 Background Service**: Runs continuously to monitor reminders
- **🚀 Auto-Start**: Automatically starts with Windows (configurable)
- **💬 Natural Language**: Parse human-friendly time inputs
- **📅 Precise Date/Time Selection**: Advanced GUI controls for exact scheduling
- **🎨 Modern Popups**: Beautiful, non-intrusive reminder windows
- **⏰ Smart Scheduling**: Handles complex time patterns and scheduling
- **🔔 Fallback Notifications**: Windows system notifications as backup

#### **Natural Language Time Parsing:**
```bash
# Relative times
"in 5 minutes", "in 2 hours", "in 3 days", "in 1 week"

# Specific days
"tomorrow", "monday", "friday", "next week"

# Time modifiers
"tomorrow morning", "tomorrow evening", "later today"

# Absolute times (when combined)
"3:30 PM tomorrow", "next Monday at 9 AM"
```

#### **Reminder Modes:**
```bash
# GUI Mode (default) - Full interface + background service
dotnet run

# Console Mode - Terminal interface + background service  
dotnet run --console

# Background Service Only - No interface, just reminder monitoring
dotnet run --background
```

#### **Modern Reminder Popup Features:**
- **📍 Smart Positioning**: Appears in bottom-right corner
- **⚡ Smooth Animations**: Slide-in/slide-out effects
- **🎨 Professional Design**: Modern, non-intrusive appearance
- **⏰ Auto-Close**: Automatically closes after 30 seconds
- **🔄 Action Buttons**: Snooze (10 min), Mark Complete, Dismiss
- **🔊 Audio Alert**: System notification sound on appearance
- **🚫 Non-Stealing Focus**: Doesn't interrupt current work

## 🧠 **LLM & NLP FEATURES - TESTING GUIDE FOR LECTURERS**

This section demonstrates the advanced Language Learning Model and Natural Language Processing capabilities implemented in the chatbot.

### 1. **Natural Language Task Management** 📝

The chatbot uses sophisticated NLP to understand various ways users might phrase task-related requests:

#### **Creating Tasks with Natural Language:**
```
User Input Examples:
• "Can you remind me to update my password?"
• "Add a task to enable two-factor authentication"
• "Create a reminder for checking my privacy settings"
• "Set up a task to install antivirus software"
• "I need to remember to update my browser"
```

**Expected Behavior:** The chatbot recognizes intent through keyword detection and responds by guiding users to the task creation interface.

#### **Marking Tasks as Complete with Natural Language:**
```
User Input Examples:
• "Mark the 2FA task as done"
• "Set password reminder to finished"
• "Complete the antivirus task"
• "I finished the browser update"
• "Mark privacy settings as completed"
```

**Expected Behavior:** The chatbot searches for matching tasks using fuzzy keyword matching and automatically marks them as complete.

### 2. **Intelligent Keyword Recognition & Context Understanding** 🎯

The system implements advanced keyword detection with contextual understanding:

#### **Test Cybersecurity Topics:**
```
Password Security:
• "How do I create a strong password?"
• "What makes passwords secure?"
• "Tell me about password managers"

Phishing Protection:
• "What is phishing?"
• "How do I spot phishing emails?"
• "Protect me from phishing attacks"

Malware Defense:
• "What is malware?"
• "How to remove viruses?"
• "Malware protection tips"

Two-Factor Authentication:
• "What is 2FA?"
• "Enable two-factor authentication"
• "Benefits of multi-factor authentication"
```

**Expected Behavior:** Context-aware responses that provide relevant cybersecurity advice based on detected keywords and user intent.

### 3. **Sentiment Analysis & Emotional Intelligence** 😊

The chatbot detects user emotional states and responds appropriately:

#### **Test Emotional States:**
```
Worried/Anxious:
• "I'm worried about my online security"
• "I'm scared my account was hacked"
• "This cybersecurity stuff makes me nervous"

Frustrated:
• "I'm frustrated with all these security requirements"
• "This is too complicated and annoying"
• "I'm fed up with password requirements"

Positive/Enthusiastic:
• "I'm excited to learn about cybersecurity"
• "I'm interested in improving my security"
• "I'm keen to protect my data better"
```

**Expected Behavior:** Empathetic responses that acknowledge the user's emotional state and provide appropriate encouragement or reassurance.

### 4. **ChatGPT Integration** 🤖

Advanced AI-powered responses for complex queries:

#### **Testing ChatGPT Features:**
1. Click the **"ChatGPT"** button instead of "Send"
2. Try complex cybersecurity questions:
   ```
   • "Explain zero-day vulnerabilities in simple terms"
   • "What's the difference between encryption and hashing?"
   • "How do VPNs protect my privacy?"
   • "What are the latest cybersecurity threats in 2024?"
   ```

**Expected Behavior:** More detailed, context-aware responses powered by OpenAI's language model (requires API key).

### 5. **Comprehensive Activity Logging System** 📊

Advanced activity tracking with detailed logging of all user interactions:

#### **Activity Log Features:**
- **Real-time Logging:** All actions are logged with timestamps
- **Categorized Tracking:** Task operations, quiz activities, NLP interactions
- **Smart Summaries:** Intelligent categorization and counting of activities
- **Limited Display:** Shows last 10 activities for clarity with "show more" functionality

#### **Test Activity Logging:**

**View Activity Log:**
```
• "Show activity log"
• "What have you done for me?"
• Click "View Activity" button
• Type "activity log" or "show log"
```

**Actions That Get Logged:**
- **Task Management:** Task creation, updates, completion, deletion
- **Quiz Activities:** Quiz starts, completions, score achievements
- **NLP Interactions:** Natural language processing, intent recognition
- **Reminders:** Reminder creation and scheduling
- **ChatGPT Usage:** AI query processing
- **System Events:** Profile updates, data loading

**Expected Log Entries:**
```
[2024-06-27 15:30:45] Created new task: 'Enable 2FA' with reminder: Tomorrow
[2024-06-27 15:31:20] Started cybersecurity quiz
[2024-06-27 15:33:15] Completed quiz - Score: 8/10 (80%)
[2024-06-27 15:33:16] Achieved excellent quiz score (80%+)
[2024-06-27 15:34:02] NLP: Recognized task creation intent
[2024-06-27 15:35:10] Completed task: 'Enable 2FA'
[2024-06-27 15:36:00] Provided tip about: passwords
```

**Expected Behavior:** Comprehensive tracking with intelligent categorization and easy access through multiple interfaces.

### 6. **Interactive Quiz with Intelligent Feedback** 🎯

Advanced quiz system with contextual explanations:

#### **Testing Quiz Intelligence:**
1. Start a quiz using: `"start quiz"` or click **"Start Quiz"**
2. **Deliberately answer questions incorrectly**
3. **Observe the intelligent feedback system:**
   - **Basic Explanation:** What the correct answer is
   - **"Why" Explanation:** Deep reasoning for why the answer is correct
   - **Educational Context:** How this applies to real-world cybersecurity

**Example of Intelligent Feedback:**
```
Question: "What is the most effective way to create a strong password?"
Wrong Answer Selected: "Use your birthday and name"
Response:
✅ Correct Answer: "Use a long, random combination of letters, numbers, and symbols"
📖 Explanation: "Strong passwords should be long (12+ characters), unique, and contain a mix of uppercase, lowercase, numbers, and symbols."
💡 Why this is correct: "Long, complex passwords are exponentially harder for attackers to crack using brute force methods. Each additional character and character type dramatically increases the time needed to break the password."
```

### 7. **Personalized User Experience** 👤

The system adapts responses based on user profiles:

#### **Testing Personalization:**
1. Set up a user profile with favorite subjects
2. Ask questions related to that subject
3. **Expected Behavior:** Personalized greetings and specialized responses based on user preferences

### 8. **Command Flexibility & Understanding** 💬

The chatbot understands various command phrasings:

#### **Test Command Variations:**
```
Task Commands:
• "show tasks" / "list tasks" / "view tasks"
• "add task" / "create task" / "new task"

Quiz Commands:
• "start quiz" / "begin quiz" / "take quiz"

Help Commands:
• "help" / "commands" / "what can you do?"

Activity Commands:
• "activity log" / "show activity" / "show log"
```

## 🎯 **SPECIFIC TESTING SCENARIOS FOR ASSESSMENT**

### Scenario 1: Natural Language Task Creation
1. Type: `"Can you remind me to enable 2FA on my social media accounts?"`
2. Follow the guided task creation process
3. **Verify:** Task is created with appropriate title and description

### Scenario 2: Intelligent Task Completion
1. Create a task with "2FA" in the title
2. Type: `"Mark the 2FA task as finished"`
3. **Verify:** System finds and completes the correct task

### Scenario 3: Contextual Help Response
1. Type: `"I'm worried about phishing emails"`
2. **Verify:** System detects worried sentiment AND phishing topic
3. **Expected:** Empathetic response with relevant phishing advice

### Scenario 4: Quiz Intelligence
1. Start quiz and intentionally answer incorrectly
2. **Verify:** Detailed explanations with "why" reasoning appear
3. **Check:** Educational value of feedback

### Scenario 5: Activity Intelligence
1. Complete several actions (create tasks, take quiz, etc.)
2. Ask: `"What have you done for me recently?"`
3. **Verify:** Intelligent summary of recent activities

## 🔧 **TECHNICAL IMPLEMENTATION DETAILS**

### NLP Techniques Used:
- **Keyword Detection:** Advanced pattern matching with synonyms
- **Intent Recognition:** Context-aware command interpretation
- **Fuzzy Matching:** Approximate string matching for task identification
- **Sentiment Analysis:** Emotional state detection and response
- **Response Personalization:** User-profile driven adaptations

### AI Integration:
- **Local NLP:** Custom implementation for real-time responses
- **External LLM:** ChatGPT API integration for complex queries
- **Hybrid Approach:** Combining rule-based and AI-powered responses

## 📁 **Project Structure**

```
ChatBot/
├── Data/                          # JSON data storage
│   ├── quiz_questions.json        # Quiz with "why" explanations
│   ├── tasks.json                 # User tasks
│   ├── responses.json             # NLP response templates
│   └── conversation_history.json  # Chat history
├── Models/                        # Data models
├── Services/                      # Business logic
├── Windows/                       # WPF GUI components
└── Core/                         # NLP engine
```

## 🌟 **KEY FEATURES FOR ASSESSMENT**

1. ✅ **Advanced NLP Processing**
2. ✅ **Sentiment Analysis & Emotional Intelligence**
3. ✅ **Context-Aware Response Generation**
4. ✅ **Intelligent Task Management**
5. ✅ **Educational Quiz with Deep Explanations**
6. ✅ **ChatGPT Integration**
7. ✅ **Activity Intelligence & Summarization**
8. ✅ **Personalized User Experience**
9. ✅ **Multi-Modal Interface (GUI + Console)**
10. ✅ **Real-time Notifications & Reminders**

## 📋 **Evaluation Checklist for Lecturers**

- [ ] Natural language task creation works
- [ ] Task completion via natural language works
- [ ] Sentiment detection responds appropriately
- [ ] Quiz provides intelligent "why" explanations
- [ ] ChatGPT integration functions (if API key provided)
- [ ] Activity summarization is intelligent
- [ ] Keyword detection covers cybersecurity topics
- [ ] User personalization adapts responses
- [ ] Multiple command phrasings understood
- [ ] Notifications and reminders function

## 🎓 **Educational Value**

This chatbot demonstrates practical application of:
- Natural Language Processing
- Machine Learning Integration
- Human-Computer Interaction
- Cybersecurity Education
- Software Engineering Best Practices

---

**Developed for Cybersecurity Education with Advanced AI/ML Integration**
