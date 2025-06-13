using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Oculus.Interaction;

public class QuestionnaireManager : MonoBehaviour
{
    [Header("VR Setup")]
    public int questionsPerPage = 3; // Anzahl Fragen pro Seite
    public bool useManualPositioning = true; // Canvas-Position manuell setzen

    [Header("Canvas References")]
    public Canvas questionnaireCanvas;
    public Canvas quizCanvas; // Referenz zu deinem bestehenden Quiz Canvas

    [Header("UI Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI pageInfoText; // "Seite 1 von 3"
    public Button nextPageButton;
    public Button previousPageButton;
    public Button finishButton; // Für letzte Seite

    [Header("Question Elements (3 per page)")]
    public QuestionUI[] questionElements = new QuestionUI[3];

    [System.Serializable]
    public class QuestionUI
    {
        public TextMeshProUGUI questionText;
        public Slider responseSlider;
        public TextMeshProUGUI sliderValueText;
        public TextMeshProUGUI leftScaleLabel;
        public TextMeshProUGUI rightScaleLabel;
        public GameObject questionContainer; // Parent GameObject für die ganze Frage
    }

    private int currentQuestionnaireIndex = 0; // 0 = PENS, 1 = NASA-TLX, 2 = Narrative Impact
    private int currentPageIndex = 0;
    private List<float> pensAnswers = new List<float>();
    private List<float> nasaTlxAnswers = new List<float>();
    private List<float> narrativeAnswers = new List<float>();

    private int totalPensPages;
    private int totalNasaTlxPages;
    private int totalNarrativePages;

    // Logging flags
    private bool hasLoggedPensHeader = false;
    private bool hasLoggedNasaTlxHeader = false;
    private bool hasLoggedNarrativeHeader = false;

    // PENS Fragebogen (21 Items)
    private string[] pensQuestions = {
        "Ich fühlte mich kompetent beim Spielen.",
        "Ich fühlte mich erfolgreich beim Erreichen der Ziele des Spiels.",
        "Ich fühlte mich beim Spielen sehr fähig und effektiv.",
        "Ich war in der Lage, wichtige Aspekte des Spiels zu meistern.",
        "Ich fühlte mich beim Spielen zufrieden mit meiner Leistung.",
        "Ich konnte beim Spielen Entscheidungen über das Vorgehen treffen.",
        "Ich fühlte mich bei meinen Handlungen im Spiel autonom.",
        "Meine Entscheidungen im Spiel spiegelten wider, was ich wirklich wollte.",
        "Ich fühlte mich beim Spielen frei, meine eigenen Entscheidungen zu treffen.",
        "Ich konnte beim Spielen meinem eigenen Interesse folgen.",
        "Ich hatte das Gefühl, mich beim Spielen nach meinen eigenen Wünschen zu verhalten.",
        "Ich verspürte beim Spielen Freude.",
        "Ich empfand das Spielen als sehr unterhaltsam.",
        "Ich fand das Spielen sehr interessant.",
        "Ich dachte, das Spiel war ziemlich angenehm.",
        "Während des Spielens dachte ich darüber nach, wie sehr mir das Spielen gefiel.",
        "Ich fand das Spielen langweilig.", // Reverse scored
        "Das Spiel war frustrierend für mich.", // Reverse scored
        "Ich fühlte mich beim Spielen angespannt.", // Reverse scored
        "Ich fühlte mich beim Spielen unter Druck gesetzt.", // Reverse scored
        "Ich fühlte mich beim Spielen ängstlich." // Reverse scored
    };

    // NASA-TLX Fragebogen (6 Dimensionen)
    private string[] nasaTlxQuestions = {
        "Wie hoch war die geistige Anforderung der Aufgabe?",
        "Wie hoch war die körperliche Anforderung der Aufgabe?",
        "Wie hoch war das Zeitdruck-Gefühl während der Aufgabe?",
        "Wie erfolgreich waren Sie bei der Bewältigung der Aufgabe?",
        "Wie anstrengend war es, die Aufgabe zu bewältigen?",
        "Wie frustriert waren Sie während der Aufgabe?"
    };

    private string[] nasaTlxLeftLabels = { "Sehr niedrig", "Sehr niedrig", "Sehr niedrig", "Perfekt", "Sehr niedrig", "Sehr niedrig" };
    private string[] nasaTlxRightLabels = { "Sehr hoch", "Sehr hoch", "Sehr hoch", "Misslungen", "Sehr hoch", "Sehr hoch" };

    // Narrative Impact Fragebogen (6 Items)
    private string[] narrativeQuestions = {
        "Die Geschichte hat mir geholfen, das Millikan-Experiment besser zu verstehen.",
        "Die Darstellung des Millikan-Experiments war klar und verständlich.",
        "Ich hätte mir mehr Entscheidungsmöglichkeiten innerhalb der Geschichte gewünscht.",
        "Die Geschichte hat mich motiviert, mich mit dem Thema auseinanderzusetzen.",
        "Ich hätte es hilfreich gefunden, wenn in der Anwendung noch weitere Personen anwesend gewesen wären (virtuell oder real).",
        "Ich fühlte mich während der Anwendung manchmal überfordert oder verwirrt."
    };

    void Start()
    {
        // Canvas-Position nicht automatisch setzen wenn manuell positioniert
        if (!useManualPositioning)
            SetupVRCanvas();

        InitializeQuestionnaire();
    }

    void SetupVRCanvas()
    {
        // Nur ausführen wenn automatische Positionierung gewünscht
        // (Dein Canvas ist ja bereits manuell positioniert)
    }

    void InitializeQuestionnaire()
    {
        // Quiz Canvas ausblenden
        if (quizCanvas != null)
            quizCanvas.gameObject.SetActive(false);

        // Questionnaire Canvas aktivieren
        questionnaireCanvas.gameObject.SetActive(true);

        // Seitenanzahl berechnen
        totalPensPages = Mathf.CeilToInt((float)pensQuestions.Length / questionsPerPage);
        totalNasaTlxPages = Mathf.CeilToInt((float)nasaTlxQuestions.Length / questionsPerPage);
        totalNarrativePages = Mathf.CeilToInt((float)narrativeQuestions.Length / questionsPerPage);

        // Antwort-Listen initialisieren
        for (int i = 0; i < pensQuestions.Length; i++)
            pensAnswers.Add(4f); // Default PENS value
        for (int i = 0; i < nasaTlxQuestions.Length; i++)
            nasaTlxAnswers.Add(11f); // Default NASA-TLX value
        for (int i = 0; i < narrativeQuestions.Length; i++)
            narrativeAnswers.Add(4f); // Default Narrative value (Likert 1-7, middle = 4)

        // Event Listeners
        nextPageButton.onClick.AddListener(NextPage);
        previousPageButton.onClick.AddListener(PreviousPage);
        finishButton.onClick.AddListener(FinishCurrentQuestionnaire);

        // Slider Event Listeners
        for (int i = 0; i < questionElements.Length; i++)
        {
            int index = i; // Closure fix
            questionElements[i].responseSlider.onValueChanged.AddListener((value) => OnSliderValueChanged(index, value));
        }

        // Erste Seite laden
        LoadCurrentPage();
    }

    void LoadCurrentPage()
    {
        string[] currentQuestions;
        List<float> currentAnswers;
        string[] leftLabels = null;
        string[] rightLabels = null;
        int totalPages;

        if (currentQuestionnaireIndex == 0) // PENS
        {
            // Logge PENS Header beim ersten Aufruf
            LogPensHeader();

            titleText.text = "PENS Fragebogen";
            instructionText.text = "Bewerten Sie auf einer Skala von 1 (stimme überhaupt nicht zu) bis 7 (stimme voll zu):";
            currentQuestions = pensQuestions;
            currentAnswers = pensAnswers;
            totalPages = totalPensPages;
        }
        else if (currentQuestionnaireIndex == 1) // NASA-TLX
        {
            // Logge NASA-TLX Header beim ersten Aufruf
            LogNasaTlxHeader();

            titleText.text = "NASA Task Load Index";
            instructionText.text = "Bewerten Sie auf einer Skala von 1 bis 21:";
            currentQuestions = nasaTlxQuestions;
            currentAnswers = nasaTlxAnswers;
            leftLabels = nasaTlxLeftLabels;
            rightLabels = nasaTlxRightLabels;
            totalPages = totalNasaTlxPages;
        }
        else // Narrative Impact
        {
            // Logge Narrative Header beim ersten Aufruf
            LogNarrativeHeader();

            titleText.text = "Wirkung der Narration";
            instructionText.text = "Bewerten Sie auf einer Skala von 1 (stimme überhaupt nicht zu) bis 7 (stimme voll zu):";
            currentQuestions = narrativeQuestions;
            currentAnswers = narrativeAnswers;
            totalPages = totalNarrativePages;
        }

        // Seiten-Info aktualisieren
        pageInfoText.text = $"Seite {currentPageIndex + 1} von {totalPages}";

        // Fragen für aktuelle Seite laden
        int startQuestionIndex = currentPageIndex * questionsPerPage;

        for (int i = 0; i < questionElements.Length; i++)
        {
            int questionIndex = startQuestionIndex + i;

            if (questionIndex < currentQuestions.Length)
            {
                // Frage anzeigen
                questionElements[i].questionContainer.SetActive(true);
                questionElements[i].questionText.text = $"{questionIndex + 1}. {currentQuestions[questionIndex]}";

                // Slider konfigurieren
                if (currentQuestionnaireIndex == 0) // PENS
                {
                    questionElements[i].responseSlider.minValue = 1;
                    questionElements[i].responseSlider.maxValue = 7;
                    questionElements[i].leftScaleLabel.text = "Stimme überhaupt nicht zu";
                    questionElements[i].rightScaleLabel.text = "Stimme voll zu";
                }
                else if (currentQuestionnaireIndex == 1) // NASA-TLX
                {
                    questionElements[i].responseSlider.minValue = 1;
                    questionElements[i].responseSlider.maxValue = 21;
                    questionElements[i].leftScaleLabel.text = leftLabels[questionIndex];
                    questionElements[i].rightScaleLabel.text = rightLabels[questionIndex];
                }
                else // Narrative Impact
                {
                    questionElements[i].responseSlider.minValue = 1;
                    questionElements[i].responseSlider.maxValue = 7;
                    questionElements[i].leftScaleLabel.text = "Stimme überhaupt nicht zu";
                    questionElements[i].rightScaleLabel.text = "Stimme voll zu";
                }

                // Gespeicherte Antwort laden
                questionElements[i].responseSlider.value = currentAnswers[questionIndex];
                OnSliderValueChanged(i, currentAnswers[questionIndex]);
            }
            else
            {
                // Frage ausblenden
                questionElements[i].questionContainer.SetActive(false);
            }
        }

        // Button-Status aktualisieren
        UpdateButtonStates(totalPages);
    }

    void LogPensHeader()
    {
        if (!hasLoggedPensHeader && StudyLogger.Instance != null)
        {
            StudyLogger.Instance.LogEvent("PENS_QUESTIONNAIRE_START", "#PENS_Questionnaire#");
            hasLoggedPensHeader = true;
            Debug.Log("PENS Questionnaire Header geloggt.");
        }
    }

    void LogNasaTlxHeader()
    {
        if (!hasLoggedNasaTlxHeader && StudyLogger.Instance != null)
        {
            StudyLogger.Instance.LogEvent("NASA_TLX_QUESTIONNAIRE_START", "#NASA_TLX_Questionnaire#");
            hasLoggedNasaTlxHeader = true;
            Debug.Log("NASA-TLX Questionnaire Header geloggt.");
        }
    }

    void LogNarrativeHeader()
    {
        if (!hasLoggedNarrativeHeader && StudyLogger.Instance != null)
        {
            StudyLogger.Instance.LogEvent("NARRATIVE_QUESTIONNAIRE_START", "#Narrative_Impact_Questionnaire#");
            hasLoggedNarrativeHeader = true;
            Debug.Log("Narrative Impact Questionnaire Header geloggt.");
        }
    }

    void UpdateButtonStates(int totalPages)
    {
        previousPageButton.interactable = currentPageIndex > 0 || currentQuestionnaireIndex > 0;

        bool isLastPage = currentPageIndex == totalPages - 1;
        bool isLastQuestionnaire = currentQuestionnaireIndex == 2; // Narrative Impact ist der letzte

        nextPageButton.gameObject.SetActive(!isLastPage || !isLastQuestionnaire);
        finishButton.gameObject.SetActive(isLastPage);
    }

    void SaveCurrentPageAnswers()
    {
        int startQuestionIndex = currentPageIndex * questionsPerPage;
        List<float> currentAnswers;

        if (currentQuestionnaireIndex == 0)
            currentAnswers = pensAnswers;
        else if (currentQuestionnaireIndex == 1)
            currentAnswers = nasaTlxAnswers;
        else
            currentAnswers = narrativeAnswers;

        for (int i = 0; i < questionElements.Length; i++)
        {
            int questionIndex = startQuestionIndex + i;
            if (questionIndex < currentAnswers.Count && questionElements[i].questionContainer.activeInHierarchy)
            {
                currentAnswers[questionIndex] = questionElements[i].responseSlider.value;
            }
        }
    }

    void LogCurrentPageAnswers()
    {
        if (StudyLogger.Instance == null)
        {
            Debug.LogWarning("StudyLogger.Instance ist null - Antworten konnten nicht geloggt werden!");
            return;
        }

        int startQuestionIndex = currentPageIndex * questionsPerPage;
        string[] currentQuestions;
        List<float> currentAnswers;
        string questionnaireType;

        if (currentQuestionnaireIndex == 0)
        {
            currentQuestions = pensQuestions;
            currentAnswers = pensAnswers;
            questionnaireType = "PENS";
        }
        else if (currentQuestionnaireIndex == 1)
        {
            currentQuestions = nasaTlxQuestions;
            currentAnswers = nasaTlxAnswers;
            questionnaireType = "NASA_TLX";
        }
        else
        {
            currentQuestions = narrativeQuestions;
            currentAnswers = narrativeAnswers;
            questionnaireType = "NARRATIVE_IMPACT";
        }

        for (int i = 0; i < questionElements.Length; i++)
        {
            int questionIndex = startQuestionIndex + i;
            if (questionIndex < currentAnswers.Count && questionElements[i].questionContainer.activeInHierarchy)
            {
                int questionNumber = questionIndex + 1;
                float rating = questionElements[i].responseSlider.value;
                string question = currentQuestions[questionIndex];

                string details = $"Questionnaire: {questionnaireType} | QuestionID: {questionNumber}";
                string additionalData = $"Question: {question} | Rating: {rating}";

                StudyLogger.Instance.LogEvent($"{questionnaireType}_ANSWER", details, additionalData);

                Debug.Log($"{questionnaireType} Q{questionNumber} geloggt: {rating}");
            }
        }
    }

    void NextPage()
    {
        SaveCurrentPageAnswers();

        // Logge die Antworten der aktuellen Seite
        LogCurrentPageAnswers();

        int totalPages;
        if (currentQuestionnaireIndex == 0)
            totalPages = totalPensPages;
        else if (currentQuestionnaireIndex == 1)
            totalPages = totalNasaTlxPages;
        else
            totalPages = totalNarrativePages;

        if (currentPageIndex < totalPages - 1)
        {
            currentPageIndex++;
            LoadCurrentPage();
        }
        else
        {
            // Zum nächsten Fragebogen wechseln
            FinishCurrentQuestionnaire();
        }
    }

    void PreviousPage()
    {
        SaveCurrentPageAnswers();

        // Logge auch bei Zurück-Navigation (für den Fall dass Werte geändert wurden)
        LogCurrentPageAnswers();

        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            LoadCurrentPage();
        }
        else if (currentQuestionnaireIndex > 0)
        {
            // Zurück zum vorherigen Fragebogen
            currentQuestionnaireIndex--;
            int previousTotalPages;
            if (currentQuestionnaireIndex == 0)
                previousTotalPages = totalPensPages;
            else if (currentQuestionnaireIndex == 1)
                previousTotalPages = totalNasaTlxPages;
            else
                previousTotalPages = totalNarrativePages;

            currentPageIndex = previousTotalPages - 1;
            LoadCurrentPage();
        }
    }

    void FinishCurrentQuestionnaire()
    {
        SaveCurrentPageAnswers();

        // Logge die Antworten der letzten Seite
        LogCurrentPageAnswers();

        if (currentQuestionnaireIndex == 0) // PENS fertig, zu NASA-TLX
        {
            // Logge Ende des PENS Fragebogens
            if (StudyLogger.Instance != null)
            {
                StudyLogger.Instance.LogEvent("PENS_QUESTIONNAIRE_END", "PENS Fragebogen abgeschlossen");
            }

            currentQuestionnaireIndex = 1;
            currentPageIndex = 0;
            LoadCurrentPage();
        }
        else if (currentQuestionnaireIndex == 1) // NASA-TLX fertig, zu Narrative Impact
        {
            // Logge Ende des NASA-TLX Fragebogens
            if (StudyLogger.Instance != null)
            {
                StudyLogger.Instance.LogEvent("NASA_TLX_QUESTIONNAIRE_END", "NASA-TLX Fragebogen abgeschlossen");
            }

            currentQuestionnaireIndex = 2;
            currentPageIndex = 0;
            LoadCurrentPage();
        }
        else // Narrative Impact fertig, alle Fragebögen abgeschlossen
        {
            // Logge Ende des Narrative Impact Fragebogens
            if (StudyLogger.Instance != null)
            {
                StudyLogger.Instance.LogEvent("NARRATIVE_QUESTIONNAIRE_END", "Narrative Impact Fragebogen abgeschlossen");
            }

            FinishQuestionnaires();
        }
    }

    void OnSliderValueChanged(int sliderIndex, float value)
    {
        questionElements[sliderIndex].sliderValueText.text = value.ToString("F0");

        // Optional: Logge Slider-Änderungen in Echtzeit (kann sehr viele Logs erzeugen)
        // LogSliderChange(sliderIndex, value);
    }

    // Optional: Echtzeit Slider-Logging (auskommentiert da es sehr viele Logs erzeugt)
    /*
    void LogSliderChange(int sliderIndex, float value)
    {
        if (StudyLogger.Instance != null)
        {
            int startQuestionIndex = currentPageIndex * questionsPerPage;
            int questionIndex = startQuestionIndex + sliderIndex;
            string questionnaireType = currentQuestionnaireIndex == 0 ? "PENS" : "NASA_TLX";
            
            string details = $"Questionnaire: {questionnaireType} | QuestionID: {questionIndex + 1} | SliderChange";
            string additionalData = $"NewValue: {value}";
            
            StudyLogger.Instance.LogEvent($"{questionnaireType}_SLIDER_CHANGE", details, additionalData);
        }
    }
    */

    void FinishQuestionnaires()
    {
        SaveCurrentPageAnswers();

        // Logge Ende aller Fragebögen
        if (StudyLogger.Instance != null)
        {
            StudyLogger.Instance.LogEvent("ALL_QUESTIONNAIRES_END", "Alle Fragebögen abgeschlossen");
        }

        // Ergebnisse speichern/verarbeiten
        SaveResults();

        // Zu Quiz wechseln
        questionnaireCanvas.gameObject.SetActive(false);
        if (quizCanvas != null)
            quizCanvas.gameObject.SetActive(true);

        // Falls du einen QuizManager hast, kannst du ihn hier starten
        // QuizManager.Instance.StartQuiz();
    }

    void SaveResults()
    {
        // Hier kannst du die Ergebnisse zusätzlich speichern (PlayerPrefs, File, etc.)
        Debug.Log("PENS Ergebnisse:");
        for (int i = 0; i < pensAnswers.Count; i++)
        {
            Debug.Log($"PENS Q{i + 1}: {pensAnswers[i]}");
        }

        Debug.Log("NASA-TLX Ergebnisse:");
        for (int i = 0; i < nasaTlxAnswers.Count; i++)
        {
            Debug.Log($"NASA-TLX Q{i + 1}: {nasaTlxAnswers[i]}");
        }

        Debug.Log("Narrative Impact Ergebnisse:");
        for (int i = 0; i < narrativeAnswers.Count; i++)
        {
            Debug.Log($"Narrative Impact Q{i + 1}: {narrativeAnswers[i]}");
        }

        // Beispiel für PlayerPrefs Speicherung:
        for (int i = 0; i < pensAnswers.Count; i++)
        {
            PlayerPrefs.SetFloat($"PENS_Q{i + 1}", pensAnswers[i]);
        }
        for (int i = 0; i < nasaTlxAnswers.Count; i++)
        {
            PlayerPrefs.SetFloat($"NASA_TLX_Q{i + 1}", nasaTlxAnswers[i]);
        }
        for (int i = 0; i < narrativeAnswers.Count; i++)
        {
            PlayerPrefs.SetFloat($"NARRATIVE_Q{i + 1}", narrativeAnswers[i]);
        }
        PlayerPrefs.Save();
    }

    // Debug-Methoden
    [ContextMenu("Force Log Current Page")]
    public void ForceLogCurrentPage()
    {
        SaveCurrentPageAnswers();
        LogCurrentPageAnswers();
    }

    [ContextMenu("Show Current State")]
    public void ShowCurrentState()
    {
        string[] questionnaireNames = { "PENS", "NASA-TLX", "Narrative Impact" };
        Debug.Log($"Current Questionnaire: {questionnaireNames[currentQuestionnaireIndex]}");
        Debug.Log($"Current Page: {currentPageIndex + 1}");
        Debug.Log($"PENS Header Logged: {hasLoggedPensHeader}");
        Debug.Log($"NASA-TLX Header Logged: {hasLoggedNasaTlxHeader}");
        Debug.Log($"Narrative Header Logged: {hasLoggedNarrativeHeader}");
    }
}