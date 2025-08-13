using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestionnaireManager : MonoBehaviour
{
    [Header("VR Setup")]
    public int questionsPerPage = 3;
    public bool useManualPositioning = true;

    [Header("Canvas References")]
    public Canvas questionnaireCanvas;
    public Canvas quizCanvas;

    [Header("UI Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI pageInfoText;
    public Button nextPageButton;
    public Button previousPageButton;
    public Button finishButton;

    [Header("Question Elements")]
    public QuestionUI[] questionElements = new QuestionUI[3];

    [System.Serializable]
    public class QuestionUI
    {
        public TextMeshProUGUI questionText;
        public Slider responseSlider;
        public TextMeshProUGUI sliderValueText;
        public TextMeshProUGUI leftScaleLabel;
        public TextMeshProUGUI rightScaleLabel;
        public GameObject questionContainer;
    }

    private int currentQuestionnaireIndex = 0;
    private int currentPageIndex = 0;
    private List<float> pensAnswers = new List<float>();
    private List<float> nasaTlxAnswers = new List<float>();
    private List<float> narrativeAnswers = new List<float>();

    private int totalPensPages;
    private int totalNasaTlxPages;
    private int totalNarrativePages;

    private bool hasLoggedPensHeader = false;
    private bool hasLoggedNasaTlxHeader = false;
    private bool hasLoggedNarrativeHeader = false;

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
        "Ich fand das Spielen langweilig.",
        "Das Spiel war frustrierend für mich.",
        "Ich fühlte mich beim Spielen angespannt.",
        "Ich fühlte mich beim Spielen unter Druck gesetzt.",
        "Ich fühlte mich beim Spielen ängstlich."
    };

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
        if (!useManualPositioning)
            SetupVRCanvas();

        InitializeQuestionnaire();
    }

    void SetupVRCanvas()
    {
        // Nur ausführen wenn automatische Positionierung gewünscht
    }

    void InitializeQuestionnaire()
    {
        if (quizCanvas != null)
            quizCanvas.gameObject.SetActive(false);

        questionnaireCanvas.gameObject.SetActive(true);

        totalPensPages = Mathf.CeilToInt((float)pensQuestions.Length / questionsPerPage);
        totalNasaTlxPages = Mathf.CeilToInt((float)nasaTlxQuestions.Length / questionsPerPage);
        totalNarrativePages = Mathf.CeilToInt((float)narrativeQuestions.Length / questionsPerPage);

        for (int i = 0; i < pensQuestions.Length; i++)
            pensAnswers.Add(4f);
        for (int i = 0; i < nasaTlxQuestions.Length; i++)
            nasaTlxAnswers.Add(11f);
        for (int i = 0; i < narrativeQuestions.Length; i++)
            narrativeAnswers.Add(4f);

        nextPageButton.onClick.AddListener(NextPage);
        previousPageButton.onClick.AddListener(PreviousPage);
        finishButton.onClick.AddListener(FinishCurrentQuestionnaire);

        for (int i = 0; i < questionElements.Length; i++)
        {
            int index = i;
            questionElements[i].responseSlider.onValueChanged.AddListener((value) => OnSliderValueChanged(index, value));
        }

        LoadCurrentPage();
    }

    void LoadCurrentPage()
    {
        string[] currentQuestions;
        List<float> currentAnswers;
        string[] leftLabels = null;
        string[] rightLabels = null;
        int totalPages;

        if (currentQuestionnaireIndex == 0)
        {
            LogPensHeader();
            titleText.text = "PENS Fragebogen";
            instructionText.text = "Bewerten Sie auf einer Skala von 1 (stimme überhaupt nicht zu) bis 7 (stimme voll zu):";
            currentQuestions = pensQuestions;
            currentAnswers = pensAnswers;
            totalPages = totalPensPages;
        }
        else if (currentQuestionnaireIndex == 1)
        {
            LogNasaTlxHeader();
            titleText.text = "NASA Task Load Index";
            instructionText.text = "Bewerten Sie auf einer Skala von 1 bis 21:";
            currentQuestions = nasaTlxQuestions;
            currentAnswers = nasaTlxAnswers;
            leftLabels = nasaTlxLeftLabels;
            rightLabels = nasaTlxRightLabels;
            totalPages = totalNasaTlxPages;
        }
        else
        {
            LogNarrativeHeader();
            titleText.text = "Wirkung der Narration";
            instructionText.text = "Bewerten Sie auf einer Skala von 1 (stimme überhaupt nicht zu) bis 7 (stimme voll zu):";
            currentQuestions = narrativeQuestions;
            currentAnswers = narrativeAnswers;
            totalPages = totalNarrativePages;
        }

        pageInfoText.text = $"Seite {currentPageIndex + 1} von {totalPages}";

        int startQuestionIndex = currentPageIndex * questionsPerPage;

        for (int i = 0; i < questionElements.Length; i++)
        {
            int questionIndex = startQuestionIndex + i;

            if (questionIndex < currentQuestions.Length)
            {
                questionElements[i].questionContainer.SetActive(true);
                questionElements[i].questionText.text = $"{questionIndex + 1}. {currentQuestions[questionIndex]}";

                if (currentQuestionnaireIndex == 0)
                {
                    questionElements[i].responseSlider.minValue = 1;
                    questionElements[i].responseSlider.maxValue = 7;
                    questionElements[i].leftScaleLabel.text = "Stimme überhaupt nicht zu";
                    questionElements[i].rightScaleLabel.text = "Stimme voll zu";
                }
                else if (currentQuestionnaireIndex == 1)
                {
                    questionElements[i].responseSlider.minValue = 1;
                    questionElements[i].responseSlider.maxValue = 21;
                    questionElements[i].leftScaleLabel.text = leftLabels[questionIndex];
                    questionElements[i].rightScaleLabel.text = rightLabels[questionIndex];
                }
                else
                {
                    questionElements[i].responseSlider.minValue = 1;
                    questionElements[i].responseSlider.maxValue = 7;
                    questionElements[i].leftScaleLabel.text = "Stimme überhaupt nicht zu";
                    questionElements[i].rightScaleLabel.text = "Stimme voll zu";
                }

                questionElements[i].responseSlider.value = currentAnswers[questionIndex];
                OnSliderValueChanged(i, currentAnswers[questionIndex]);
            }
            else
            {
                questionElements[i].questionContainer.SetActive(false);
            }
        }

        UpdateButtonStates(totalPages);
    }

    void LogPensHeader()
    {
        if (!hasLoggedPensHeader)
        {
            Debug.Log("PENS Questionnaire gestartet");
            hasLoggedPensHeader = true;
        }
    }

    void LogNasaTlxHeader()
    {
        if (!hasLoggedNasaTlxHeader)
        {
            Debug.Log("NASA-TLX Questionnaire gestartet");
            hasLoggedNasaTlxHeader = true;
        }
    }

    void LogNarrativeHeader()
    {
        if (!hasLoggedNarrativeHeader)
        {
            Debug.Log("Narrative Impact Questionnaire gestartet");
            hasLoggedNarrativeHeader = true;
        }
    }

    void UpdateButtonStates(int totalPages)
    {
        previousPageButton.interactable = currentPageIndex > 0 || currentQuestionnaireIndex > 0;

        bool isLastPage = currentPageIndex == totalPages - 1;
        bool isLastQuestionnaire = currentQuestionnaireIndex == 2;

        nextPageButton.gameObject.SetActive(!isLastPage || !isLastQuestionnaire);
        finishButton.gameObject.SetActive(isLastPage);
    }

    void SaveCurrentPageData()
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
        if (CSVStudyLogger.Instance == null) return;

        int startQuestionIndex = currentPageIndex * questionsPerPage;

        for (int i = 0; i < questionElements.Length; i++)
        {
            int questionIndex = startQuestionIndex + i;
            if (questionIndex < GetCurrentQuestionCount() && questionElements[i].questionContainer.activeInHierarchy)
            {
                int questionNumber = questionIndex + 1;
                int rating = Mathf.RoundToInt(questionElements[i].responseSlider.value);

                if (currentQuestionnaireIndex == 0)
                {
                    CSVStudyLogger.Instance.LogPensAnswer(questionNumber, rating);
                }
                else if (currentQuestionnaireIndex == 1)
                {
                    CSVStudyLogger.Instance.LogNasaTlxAnswer(questionNumber, rating);
                }
                else
                {
                    CSVStudyLogger.Instance.LogNarrativeAnswer(questionNumber, rating);
                }

                Debug.Log($"{GetCurrentQuestionnaireType()} Q{questionNumber} geloggt: {rating}");
            }
        }
    }

    int GetCurrentQuestionCount()
    {
        if (currentQuestionnaireIndex == 0)
            return pensQuestions.Length;
        else if (currentQuestionnaireIndex == 1)
            return nasaTlxQuestions.Length;
        else
            return narrativeQuestions.Length;
    }

    string GetCurrentQuestionnaireType()
    {
        if (currentQuestionnaireIndex == 0)
            return "PENS";
        else if (currentQuestionnaireIndex == 1)
            return "NASA-TLX";
        else
            return "NARRATIVE";
    }

    void NextPage()
    {
        SaveCurrentPageData();
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
            FinishCurrentQuestionnaire();
        }
    }

    void PreviousPage()
    {
        SaveCurrentPageData();

        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            LoadCurrentPage();
        }
        else if (currentQuestionnaireIndex > 0)
        {
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
        SaveCurrentPageData();
        LogCurrentPageAnswers();

        if (currentQuestionnaireIndex == 0)
        {
            Debug.Log("PENS Fragebogen abgeschlossen");
            currentQuestionnaireIndex = 1;
            currentPageIndex = 0;
            LoadCurrentPage();
        }
        else if (currentQuestionnaireIndex == 1)
        {
            Debug.Log("NASA-TLX Fragebogen abgeschlossen");
            currentQuestionnaireIndex = 2;
            currentPageIndex = 0;
            LoadCurrentPage();
        }
        else
        {
            Debug.Log("Narrative Impact Fragebogen abgeschlossen");
            FinishQuestionnaires();
        }
    }

    void OnSliderValueChanged(int sliderIndex, float value)
    {
        questionElements[sliderIndex].sliderValueText.text = value.ToString("F0");
    }

    void FinishQuestionnaires()
    {
        SaveCurrentPageData();
        Debug.Log("Alle Fragebögen abgeschlossen");

        questionnaireCanvas.gameObject.SetActive(false);
        if (quizCanvas != null)
            quizCanvas.gameObject.SetActive(true);
    }
}