using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text;

public class CSVStudyLogger : MonoBehaviour
{
    [Header("Teilnehmer Konfiguration")]
    [SerializeField] private string participantID = "";
    [Tooltip("ID des Teilnehmers - MUSS vor Play gesetzt werden!")]

    [Header("Custom Log Path (Optional)")]
    [SerializeField] private string customLogPath = "";
    [Tooltip("Wenn leer, wird Standard-Pfad verwendet")]

    // Singleton Pattern
    public static CSVStudyLogger Instance { get; private set; }

    private string csvFilePath;
    private DateTime sessionStartTime;
    private DateTime sessionEndTime;

    // Data Storage für CSV
    private CSVData csvData = new CSVData();

    [System.Serializable]
    public class CSVData
    {
        // Basic Info
        public string participantID;
        public string sceneName;
        public DateTime sessionStart;
        public DateTime sessionEnd;
        public string duration;
        public string time;

        // Demographics
        public int age = -1;
        public int gender = -1; // 0=weiblich, 1=männlich, 2=divers, 3=keine Angabe
        public int grade = -1;
        public int millikansKnowledge = -1;
        public int physicsInterest = -1;
        public int electrodynamicsKnowledge = -1;
        public int vrExperience = -1;
        public int vrFrequency = -1;

        // Pre Test
        public DateTime preTestStart;
        public DateTime preTestEnd;
        public string preTestDuration;
        public int[] preTestAnswers = new int[10] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

        // Button Interactions (7 Buttons)
        public DateTime[] buttonClicks = new DateTime[7];

        // PENS (21 Questions)
        public int[] pensAnswers = new int[21];

        // NASA-TLX (6 Questions)
        public int[] nasaTlxAnswers = new int[6];

        // Narrative (6 Questions)
        public int[] narrativeAnswers = new int[6];

        // Post Test
        public DateTime postTestStart;
        public DateTime postTestEnd;
        public string postTestDuration;
        public int[] postTestAnswers = new int[10] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

        public CSVData()
        {
            // Initialize arrays with default values
            for (int i = 0; i < 21; i++) pensAnswers[i] = -1;
            for (int i = 0; i < 6; i++) nasaTlxAnswers[i] = -1;
            for (int i = 0; i < 6; i++) narrativeAnswers[i] = -1;
            for (int i = 0; i < 7; i++) buttonClicks[i] = DateTime.MinValue;
        }
    }

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

        // Basic Data setzen
        csvData.participantID = participantID;
        csvData.sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        csvData.sessionStart = sessionStartTime;
        csvData.time = sessionStartTime.ToString("HH:mm:ss");

        // CSV-Datei Pfad erstellen
        string timestamp = sessionStartTime.ToString("yyyyMMdd_HHmmss");
        string fileName = $"{participantID}_{timestamp}_StudyData.csv";

        string logDirectory = !string.IsNullOrEmpty(customLogPath) ? customLogPath : GetLogDirectory();
        csvFilePath = Path.Combine(logDirectory, fileName);

        try
        {
            Directory.CreateDirectory(logDirectory);
            Debug.Log($"CSV Logging initialisiert: {csvFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Fehler beim Erstellen des Log-Verzeichnisses: {e.Message}");
        }
    }

    private string GetLogDirectory()
    {
#if UNITY_EDITOR
        return Path.Combine(Application.dataPath, "..", "Logs");
#elif UNITY_ANDROID
        return "/storage/emulated/0/Download";
#elif UNITY_STANDALONE_WIN
        string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        return Path.Combine(documentsPath, Application.productName, "Logs");
#else
        return Path.Combine(Application.persistentDataPath, "Logs");
#endif
    }

    // Demographics Logging
    public void LogDemographics(int age, int gender, int grade, int millikan, int physics, int electro, int vrExp, int vrFreq)
    {
        csvData.age = age;
        csvData.gender = gender;
        csvData.grade = grade;
        csvData.millikansKnowledge = millikan;
        csvData.physicsInterest = physics;
        csvData.electrodynamicsKnowledge = electro;
        csvData.vrExperience = vrExp;
        csvData.vrFrequency = vrFreq;

        Debug.Log("Demographics data saved to CSV structure");
    }

    // Pre Test Logging
    public void LogPreTestStart()
    {
        csvData.preTestStart = DateTime.Now;
        Debug.Log("Pre-Test gestartet: " + csvData.preTestStart.ToString("HH:mm:ss"));
    }

    public void LogPreTestAnswer(int questionID, int answerID)
    {
        if (questionID >= 1 && questionID <= 10)
        {
            csvData.preTestAnswers[questionID - 1] = answerID;
            Debug.Log($"Pre-Test Antwort Q{questionID}: {answerID}");
        }
    }

    public void LogPreTestEnd()
    {
        csvData.preTestEnd = DateTime.Now;
        TimeSpan duration = csvData.preTestEnd - csvData.preTestStart;
        csvData.preTestDuration = duration.ToString(@"hh\:mm\:ss");
        Debug.Log("Pre-Test beendet: " + csvData.preTestEnd.ToString("HH:mm:ss"));
    }

    // Button Interaction Logging
    public void LogButtonClick(int buttonNumber)
    {
        if (buttonNumber >= 1 && buttonNumber <= 7)
        {
            csvData.buttonClicks[buttonNumber - 1] = DateTime.Now;
            Debug.Log($"Button {buttonNumber} geklickt: {DateTime.Now.ToString("HH:mm:ss")}");
        }
    }

    // PENS Logging
    public void LogPensAnswer(int questionID, int rating)
    {
        if (questionID >= 1 && questionID <= 21)
        {
            csvData.pensAnswers[questionID - 1] = rating;
            Debug.Log($"PENS Q{questionID}: {rating}");
        }
    }

    // NASA-TLX Logging
    public void LogNasaTlxAnswer(int questionID, int rating)
    {
        if (questionID >= 1 && questionID <= 6)
        {
            csvData.nasaTlxAnswers[questionID - 1] = rating;
            Debug.Log($"NASA-TLX Q{questionID}: {rating}");
        }
    }

    // Narrative Logging
    public void LogNarrativeAnswer(int questionID, int rating)
    {
        if (questionID >= 1 && questionID <= 6)
        {
            csvData.narrativeAnswers[questionID - 1] = rating;
            Debug.Log($"Narrative Q{questionID}: {rating}");
        }
    }

    // Post Test Logging
    public void LogPostTestStart()
    {
        csvData.postTestStart = DateTime.Now;
        Debug.Log("Post-Test gestartet: " + csvData.postTestStart.ToString("HH:mm:ss"));
    }

    public void LogPostTestAnswer(int questionID, int answerID)
    {
        if (questionID >= 1 && questionID <= 10)
        {
            csvData.postTestAnswers[questionID - 1] = answerID;
            Debug.Log($"Post-Test Antwort Q{questionID}: {answerID}");
        }
    }

    public void LogPostTestEnd()
    {
        csvData.postTestEnd = DateTime.Now;
        TimeSpan duration = csvData.postTestEnd - csvData.postTestStart;
        csvData.postTestDuration = duration.ToString(@"hh\:mm\:ss");
        Debug.Log("Post-Test beendet: " + csvData.postTestEnd.ToString("HH:mm:ss"));
    }

    // Session Ende und CSV Export
    public void EndSession()
    {
        sessionEndTime = DateTime.Now;
        csvData.sessionEnd = sessionEndTime;
        TimeSpan totalDuration = sessionEndTime - sessionStartTime;
        csvData.duration = totalDuration.ToString(@"hh\:mm\:ss");

        WriteCSVFile();
        Debug.Log($"Session beendet. CSV gespeichert: {csvFilePath}");
    }

    private void WriteCSVFile()
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(csvFilePath, false, Encoding.UTF8))
            {
                // Header schreiben
                writer.WriteLine(GetCSVHeader());

                // Data schreiben
                writer.WriteLine(GetCSVData());
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Fehler beim Schreiben der CSV: {e.Message}");
        }
    }

    private string GetCSVHeader()
    {
        StringBuilder header = new StringBuilder();

        // Basic Info
        header.Append("Participant_ID,Scene_name,Session_Start,Session_End,Duration,Time,");

        // Demographics
        header.Append("Age,Gender,Grade,Millikan_Knowledge,Physics_Interest,Electrodynamics_Knowledge,VR_Experience,VR_Frequency,");

        // Pre Test
        header.Append("PreTestTimestampStart,");
        for (int i = 1; i <= 10; i++)
        {
            header.Append($"PreQuestion{i},");
        }
        header.Append("PreTestTimestampEnd,PreTestDuration,");

        // Button Interactions
        for (int i = 1; i <= 7; i++)
        {
            header.Append($"Button_{i:D2},");
        }

        // PENS
        for (int i = 1; i <= 21; i++)
        {
            header.Append($"PENS_Question{i},");
        }

        // NASA-TLX
        for (int i = 1; i <= 6; i++)
        {
            header.Append($"NASA_TLX_Question{i},");
        }

        // Narrative
        for (int i = 1; i <= 6; i++)
        {
            header.Append($"Narrative_Question{i},");
        }

        // Post Test
        header.Append("PostTestTimestampStart,");
        for (int i = 1; i <= 10; i++)
        {
            header.Append($"PostQuestion{i},");
        }
        header.Append("PostTestTimestampEnd,PostTestDuration");

        return header.ToString();
    }

    private string GetCSVData()
    {
        StringBuilder data = new StringBuilder();

        // Basic Info
        data.Append($"{csvData.participantID},");
        data.Append($"{csvData.sceneName},");
        data.Append($"{csvData.sessionStart:yyyy-MM-dd HH:mm:ss},");
        data.Append($"{csvData.sessionEnd:yyyy-MM-dd HH:mm:ss},");
        data.Append($"{csvData.duration},");
        data.Append($"{csvData.time},");

        // Demographics
        data.Append($"{(csvData.age >= 0 ? csvData.age.ToString() : "NULL")},");
        data.Append($"{(csvData.gender >= 0 ? csvData.gender.ToString() : "NULL")},");
        data.Append($"{(csvData.grade >= 0 ? csvData.grade.ToString() : "NULL")},");
        data.Append($"{(csvData.millikansKnowledge >= 0 ? csvData.millikansKnowledge.ToString() : "NULL")},");
        data.Append($"{(csvData.physicsInterest >= 0 ? csvData.physicsInterest.ToString() : "NULL")},");
        data.Append($"{(csvData.electrodynamicsKnowledge >= 0 ? csvData.electrodynamicsKnowledge.ToString() : "NULL")},");
        data.Append($"{(csvData.vrExperience >= 0 ? csvData.vrExperience.ToString() : "NULL")},");
        data.Append($"{(csvData.vrFrequency >= 0 ? csvData.vrFrequency.ToString() : "NULL")},");

        // Pre Test
        data.Append($"{(csvData.preTestStart != DateTime.MinValue ? csvData.preTestStart.ToString("HH:mm:ss") : "NULL")},");
        for (int i = 0; i < 10; i++)
        {
            data.Append($"{(csvData.preTestAnswers[i] >= 0 ? csvData.preTestAnswers[i].ToString() : "NULL")},");
        }
        data.Append($"{(csvData.preTestEnd != DateTime.MinValue ? csvData.preTestEnd.ToString("HH:mm:ss") : "NULL")},");
        data.Append($"{(string.IsNullOrEmpty(csvData.preTestDuration) ? "NULL" : csvData.preTestDuration)},");

        // Button Interactions
        for (int i = 0; i < 7; i++)
        {
            data.Append($"{(csvData.buttonClicks[i] != DateTime.MinValue ? csvData.buttonClicks[i].ToString("HH:mm:ss") : "NULL")},");
        }

        // PENS
        for (int i = 0; i < 21; i++)
        {
            data.Append($"{(csvData.pensAnswers[i] >= 0 ? csvData.pensAnswers[i].ToString() : "NULL")},");
        }

        // NASA-TLX
        for (int i = 0; i < 6; i++)
        {
            data.Append($"{(csvData.nasaTlxAnswers[i] >= 0 ? csvData.nasaTlxAnswers[i].ToString() : "NULL")},");
        }

        // Narrative
        for (int i = 0; i < 6; i++)
        {
            data.Append($"{(csvData.narrativeAnswers[i] >= 0 ? csvData.narrativeAnswers[i].ToString() : "NULL")},");
        }

        // Post Test
        data.Append($"{(csvData.postTestStart != DateTime.MinValue ? csvData.postTestStart.ToString("HH:mm:ss") : "NULL")},");
        for (int i = 0; i < 10; i++)
        {
            data.Append($"{(csvData.postTestAnswers[i] >= 0 ? csvData.postTestAnswers[i].ToString() : "NULL")},");
        }
        data.Append($"{(csvData.postTestEnd != DateTime.MinValue ? csvData.postTestEnd.ToString("HH:mm:ss") : "NULL")},");
        data.Append($"{(string.IsNullOrEmpty(csvData.postTestDuration) ? "NULL" : csvData.postTestDuration)}");

        return data.ToString();
    }

    // Getter
    public string GetParticipantID() => participantID;
    public string GetCSVFilePath() => csvFilePath;

    private void OnApplicationQuit()
    {
        EndSession();
    }

    // Debug Methods
    [ContextMenu("Export CSV Now")]
    public void ForceExportCSV()
    {
        EndSession();
    }

    [ContextMenu("Show CSV Path")]
    public void ShowCSVPath()
    {
        Debug.Log($"CSV will be saved to: {csvFilePath}");
    }
}
