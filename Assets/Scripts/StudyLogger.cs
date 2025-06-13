using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

public class StudyLogger : MonoBehaviour
{
    [Header("Teilnehmer Konfiguration")]
    [SerializeField] private string participantID = "";
    [Tooltip("ID des Teilnehmers - MUSS vor Play gesetzt werden!")]

    [Header("Logging Konfiguration")]
    [SerializeField] private string logFileName = "VR_Study_Log.txt";
    [SerializeField] private bool enableDebugLogs = true;

    [Header("Custom Log Path (Optional)")]
    [SerializeField] private string customLogPath = "";
    [Tooltip("Wenn leer, wird Standard-Pfad verwendet. Beispiel: C:\\Users\\janin\\Documents\\GitHub\\Millikan_SingleUser\\Logs")]

    // Singleton Pattern
    public static StudyLogger Instance { get; private set; }

    private string logFilePath;
    private StreamWriter logWriter;
    private DateTime sessionStartTime;

    // Event System für andere Scripts
    public static event Action<string> OnLogEntry;

    private void Awake()
    {
        // Singleton Setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeLogging();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeLogging()
    {
        // Validierung der Participant ID
        if (string.IsNullOrEmpty(participantID))
        {
            Debug.LogError("FEHLER: Participant ID ist leer! Bitte ID im Inspector setzen vor Play!");
            enabled = false;
            return;
        }

        sessionStartTime = DateTime.Now;

        // Log-Datei Pfad erstellen - Verbesserter Ansatz
        string timestamp = sessionStartTime.ToString("yyyyMMdd_HHmmss");
        string fileName = $"{participantID}_{timestamp}_{logFileName}";

        // Verwende benutzerdefinierten Pfad oder Standard-Pfad
        string logDirectory = !string.IsNullOrEmpty(customLogPath) ? customLogPath : GetLogDirectory();
        logFilePath = Path.Combine(logDirectory, fileName);

        try
        {
            // Stelle sicher, dass das Verzeichnis existiert
            Directory.CreateDirectory(logDirectory);

            // Log-Datei erstellen und Header schreiben
            logWriter = new StreamWriter(logFilePath, true);
            WriteLogHeader();

            // Session Start loggen
            LogEvent("SESSION_START", $"Participant ID: {participantID}");

            if (enableDebugLogs)
            {
                Debug.Log($"Logging initialisiert: {logFilePath}");
                Debug.Log($"Participant ID: {participantID}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Fehler beim Erstellen der Log-Datei: {e.Message}");
            Debug.LogError($"Versuchter Pfad: {logFilePath}");
        }
    }

    private string GetLogDirectory()
    {
        // Wähle sicheren Pfad basierend auf Platform
#if UNITY_EDITOR
        // Im Editor: Projekt-Ordner/Logs
        return Path.Combine(Application.dataPath, "..", "Logs");
#elif UNITY_ANDROID
            // Auf Android/Quest: Persistent Data Path
            return Path.Combine(Application.persistentDataPath, "Logs");
#elif UNITY_STANDALONE_WIN
            // Auf Windows: Dokumente/AppName/Logs
            string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            return Path.Combine(documentsPath, Application.productName, "Logs");
#else
            // Fallback: Persistent Data Path
            return Path.Combine(Application.persistentDataPath, "Logs");
#endif
    }

    private void WriteLogHeader()
    {
        logWriter.WriteLine("=== VR STUDY LOG ===");
        logWriter.WriteLine($"Participant ID: {participantID}");
        logWriter.WriteLine($"Session Start: {sessionStartTime:yyyy-MM-dd HH:mm:ss}");
        logWriter.WriteLine($"Unity Version: {Application.unityVersion}");
        logWriter.WriteLine($"Platform: {Application.platform}");
        logWriter.WriteLine($"Log File Path: {logFilePath}");
        logWriter.WriteLine("=====================================");
        logWriter.WriteLine();
        logWriter.Flush();
    }

    /// <summary>
    /// Allgemeine Event-Logging Funktion
    /// </summary>
    public void LogEvent(string eventType, string details = "", string additionalData = "")
    {
        if (logWriter == null)
        {
            Debug.LogWarning("LogWriter ist null - Event konnte nicht geloggt werden!");
            return;
        }

        try
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string timeFromStart = (DateTime.Now - sessionStartTime).ToString(@"hh\:mm\:ss\.fff");

            string logEntry = $"[{timestamp}] [{timeFromStart}] {eventType}";

            if (!string.IsNullOrEmpty(details))
                logEntry += $" | {details}";

            if (!string.IsNullOrEmpty(additionalData))
                logEntry += $" | Data: {additionalData}";

            logWriter.WriteLine(logEntry);
            logWriter.Flush();

            if (enableDebugLogs)
                Debug.Log($"LOG: {logEntry}");

            // Event für andere Scripts
            OnLogEntry?.Invoke(logEntry);
        }
        catch (Exception e)
        {
            Debug.LogError($"Fehler beim Schreiben ins Log: {e.Message}");
        }
    }

    /// <summary>
    /// Quiz-Antworten loggen
    /// </summary>
    public void LogQuizAnswer(string quizType, int questionNumber, string question, string selectedAnswer, bool isCorrect)
    {
        string details = $"Quiz: {quizType} | Q{questionNumber}: {question}";
        string data = $"Answer: {selectedAnswer} | Correct: {isCorrect}";
        LogEvent("QUIZ_ANSWER", details, data);
    }

    /// <summary>
    /// PENS Questionnaire Antworten loggen
    /// </summary>
    public void LogPENSAnswer(string dimension, int questionNumber, string question, int rating)
    {
        string details = $"PENS | Dimension: {dimension} | Q{questionNumber}: {question}";
        string data = $"Rating: {rating}";
        LogEvent("PENS_ANSWER", details, data);
    }

    /// <summary>
    /// NASA-TLX Antworten loggen
    /// </summary>
    public void LogNASATLXAnswer(string dimension, int rating, string description = "")
    {
        string details = $"NASA-TLX | Dimension: {dimension}";
        string data = $"Rating: {rating}";
        if (!string.IsNullOrEmpty(description))
            data += $" | Description: {description}";
        LogEvent("NASA_TLX_ANSWER", details, data);
    }

    /// <summary>
    /// Informationsblock-Interaktionen loggen
    /// </summary>
    public void LogInformationBlockInteraction(string blockID, string blockTitle, bool wasExpanded)
    {
        string action = wasExpanded ? "EXPANDED" : "COLLAPSED";
        string details = $"Info Block: {blockID} | Title: {blockTitle}";
        string data = $"Action: {action}";
        LogEvent("INFO_BLOCK_INTERACTION", details, data);
    }

    /// <summary>
    /// Szenen-Wechsel loggen
    /// </summary>
    public void LogSceneChange(string fromScene, string toScene)
    {
        string details = $"From: {fromScene} | To: {toScene}";
        LogEvent("SCENE_CHANGE", details);
    }

    /// <summary>
    /// VR-spezifische Ereignisse loggen
    /// </summary>
    public void LogVREvent(string eventType, string details = "")
    {
        LogEvent($"VR_{eventType}", details);
    }

    /// <summary>
    /// Benutzer-Interaktionen loggen
    /// </summary>
    public void LogUserInteraction(string interactionType, string objectName, Vector3 position)
    {
        string details = $"Interaction: {interactionType} | Object: {objectName}";
        string data = $"Position: {position}";
        LogEvent("USER_INTERACTION", details, data);
    }

    /// <summary>
    /// Session beenden und Log schließen
    /// </summary>
    public void EndSession()
    {
        if (logWriter != null)
        {
            DateTime endTime = DateTime.Now;
            TimeSpan sessionDuration = endTime - sessionStartTime;

            LogEvent("SESSION_END", $"Duration: {sessionDuration:hh\\:mm\\:ss}");

            logWriter.WriteLine();
            logWriter.WriteLine("=== SESSION ENDED ===");
            logWriter.WriteLine($"End Time: {endTime:yyyy-MM-dd HH:mm:ss}");
            logWriter.WriteLine($"Total Duration: {sessionDuration:hh\\:mm\\:ss}");
            logWriter.WriteLine("=====================");

            logWriter.Close();
            logWriter = null;

            if (enableDebugLogs)
                Debug.Log($"Logging beendet. Datei gespeichert: {logFilePath}");
        }
    }

    /// <summary>
    /// Participant ID abrufen
    /// </summary>
    public string GetParticipantID()
    {
        return participantID;
    }

    /// <summary>
    /// Log-Datei Pfad abrufen
    /// </summary>
    public string GetLogFilePath()
    {
        return logFilePath;
    }

    /// <summary>
    /// Custom Log Path zur Laufzeit setzen
    /// </summary>
    public void SetCustomLogPath(string path)
    {
        customLogPath = path;
    }

    private void OnApplicationQuit()
    {
        EndSession();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            LogEvent("APPLICATION_PAUSED");
        else
            LogEvent("APPLICATION_RESUMED");
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            LogEvent("APPLICATION_FOCUS_GAINED");
        else
            LogEvent("APPLICATION_FOCUS_LOST");
    }

    // Debug-Methode um aktuellen Pfad zu zeigen
    [ContextMenu("Show Log Path")]
    public void ShowLogPath()
    {
        string logDir = !string.IsNullOrEmpty(customLogPath) ? customLogPath : GetLogDirectory();
        Debug.Log($"Current Log Directory: {logDir}");
        if (!string.IsNullOrEmpty(logFilePath))
        {
            Debug.Log($"Current Log File Path: {logFilePath}");
            Debug.Log($"File exists: {File.Exists(logFilePath)}");
        }
        else
        {
            Debug.Log("Log file path is not set yet!");
        }
    }
}