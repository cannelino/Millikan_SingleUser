using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TestManager_Post : MonoBehaviour
{
    [Header("Basic Quiz Settings")]
    public TestData testData;
    public TextMeshProUGUI questionText;
    public Button[] answerButtons;

    [Header("Progress & Navigation")]
    public TextMeshProUGUI progressText; // "x/10" Anzeige
    public Button nextButton; // "Weiter" Button
    public Button finishButton; // "Zum Ende" Button
    public TextMeshProUGUI completionText; // Abschlusstext

    [Header("Feedback Settings")]
    public bool enableFeedback = false; // Toggle für Feedback ein/aus
    public Color correctColor = Color.green;
    public Color incorrectColor = Color.red;
    public Color defaultButtonColor = Color.white;

    [Header("Quiz Elements to Hide on Completion")]
    public GameObject[] quizElements; // Array für alle Quiz-UI-Elemente die versteckt werden sollen

    private int currentIndex = 0;
    private bool questionAnswered = false;
    private Color[] originalButtonColors; // Speichere die ursprünglichen Button-Farben
    private bool hasLoggedPostTestHeader = false; // Flag um Header nur einmal zu loggen

    void Start()
    {
        // Validierung der Test-Daten
        if (testData == null)
        {
            Debug.LogError("TestData ist null! Bitte TestData im Inspector zuweisen.");
            return;
        }

        if (testData.questions == null || testData.questions.Count == 0)
        {
            Debug.LogError("Keine Fragen in TestData gefunden!");
            return;
        }

        // Validierung der Answer Buttons
        if (answerButtons == null || answerButtons.Length == 0)
        {
            Debug.LogError("Keine Answer Buttons zugewiesen!");
            return;
        }

        // Speichere die ursprünglichen Button-Farben
        SaveOriginalButtonColors();

        // Setup Navigation Buttons
        SetupNavigationButtons();

        // Setup Completion Text (initially hidden)
        if (completionText != null)
            completionText.gameObject.SetActive(false);

        // Logge Post-Wissenstest Header beim ersten Start
        LogPostTestHeader();

        // Starte Post-Test Logging
        if (CSVStudyLogger.Instance != null)
        {
            CSVStudyLogger.Instance.LogPostTestStart();
        }

        DisplayQuestion();
    }

    void LogPostTestHeader()
    {
        if (!hasLoggedPostTestHeader)
        {
            Debug.Log("Post-Wissenstest gestartet");
            hasLoggedPostTestHeader = true;
        }
    }

    void SaveOriginalButtonColors()
    {
        originalButtonColors = new Color[answerButtons.Length];
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] != null)
            {
                Image buttonImage = answerButtons[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    originalButtonColors[i] = buttonImage.color;
                }
            }
        }
    }

    void SetupNavigationButtons()
    {
        // Setup Next Button
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(ProceedToNextQuestion);
            nextButton.interactable = false; // Deaktiviert bis Antwort gegeben
        }

        // Setup Finish Button
        if (finishButton != null)
        {
            finishButton.onClick.RemoveAllListeners();
            finishButton.onClick.AddListener(CompleteTest);
            finishButton.gameObject.SetActive(false); // Versteckt bis letzte Frage
        }
    }

    void DisplayQuestion()
    {
        // Sicherheitscheck: Verhindere Index-Überschreitung
        if (currentIndex >= testData.questions.Count)
        {
            Debug.Log($"Alle Fragen beantwortet. CurrentIndex: {currentIndex}, Anzahl Fragen: {testData.questions.Count}");
            CompleteTest();
            return;
        }

        TestQuestion q = testData.questions[currentIndex];
        if (q == null)
        {
            Debug.LogError($"Frage bei Index {currentIndex} ist null!");
            CompleteTest();
            return;
        }

        questionText.text = q.questionText;

        // Update Progress Display
        UpdateProgressDisplay();

        // Reset question answered state
        questionAnswered = false;

        // Sicherheitscheck für Antworten
        if (q.answers == null || q.answers.Length == 0)
        {
            Debug.LogError($"Keine Antworten für Frage {currentIndex} gefunden!");
            CompleteTest();
            return;
        }

        // Setze nur so viele Buttons wie Antworten vorhanden sind
        int maxButtons = Mathf.Min(answerButtons.Length, q.answers.Length);

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] == null) continue;

            if (i < maxButtons)
            {
                // Button aktivieren und Text setzen
                answerButtons[i].gameObject.SetActive(true);

                TextMeshProUGUI buttonText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = q.answers[i];
                }

                // Event Listener hinzufügen
                int index = i; // Capture für Closure
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => Answer(index));
                answerButtons[i].interactable = true;

                // Setze Button-Farbe zurück
                Image buttonImage = answerButtons[i].GetComponent<Image>();
                if (buttonImage != null && i < originalButtonColors.Length)
                {
                    buttonImage.color = originalButtonColors[i];
                }
            }
            else
            {
                // Deaktiviere überschüssige Buttons
                answerButtons[i].gameObject.SetActive(false);
            }
        }

        // Update Navigation Buttons
        UpdateNavigationButtons();

        Debug.Log($"Post-Test Frage {currentIndex + 1} von {testData.questions.Count} angezeigt: {q.questionText}");
    }

    void UpdateProgressDisplay()
    {
        if (progressText != null)
        {
            int totalQuestions = testData.questions.Count;
            progressText.text = $"{currentIndex + 1}/{totalQuestions}";
        }
    }

    void UpdateNavigationButtons()
    {
        bool isLastQuestion = (currentIndex >= testData.questions.Count - 1);

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(!isLastQuestion);
            nextButton.interactable = questionAnswered;
        }

        if (finishButton != null)
        {
            finishButton.gameObject.SetActive(isLastQuestion);
        }
    }

    void Answer(int index)
    {
        if (questionAnswered) return; // Verhindere mehrfache Eingaben

        // Sicherheitscheck: Verhindere Index-Überschreitung
        if (currentIndex >= testData.questions.Count)
        {
            Debug.LogError($"CurrentIndex ({currentIndex}) überschreitet Anzahl Fragen ({testData.questions.Count})!");
            return;
        }

        TestQuestion currentQuestion = testData.questions[currentIndex];
        if (currentQuestion == null || currentQuestion.answers == null)
        {
            Debug.LogError($"Aktuelle Frage oder Antworten sind null! CurrentIndex: {currentIndex}");
            return;
        }

        if (index >= currentQuestion.answers.Length)
        {
            Debug.LogError($"Answer Index ({index}) überschreitet Anzahl Antworten ({currentQuestion.answers.Length})!");
            return;
        }

        questionAnswered = true;
        Debug.Log($"Post-Test Antwort gewählt: {index} - {currentQuestion.answers[index]}");

        // Logge die Antwort sofort beim Klicken
        LogQuizAnswer(index);

        // Deaktiviere Answer-Buttons nach Antwort
        SetAnswerButtonsInteractable(false);

        if (enableFeedback)
        {
            ShowFeedback(index);
        }

        // Aktiviere Navigation Button sofort
        UpdateNavigationButtons();
    }

    void LogQuizAnswer(int selectedAnswerIndex)
    {
        if (CSVStudyLogger.Instance != null)
        {
            // Sicherheitscheck vor Zugriff auf Arrays
            if (currentIndex >= testData.questions.Count)
            {
                Debug.LogError("Kann nicht loggen - currentIndex überschreitet Fragenanzahl!");
                return;
            }

            TestQuestion currentQuestion = testData.questions[currentIndex];
            if (currentQuestion == null || currentQuestion.answers == null)
            {
                Debug.LogError("Kann nicht loggen - aktuelle Frage oder Antworten sind null!");
                return;
            }

            if (selectedAnswerIndex >= currentQuestion.answers.Length)
            {
                Debug.LogError("Kann nicht loggen - selectedAnswerIndex überschreitet Antwortanzahl!");
                return;
            }

            // Frage ID ist currentIndex + 1 (da wir bei 1 anfangen wollen)
            int questionId = currentIndex + 1;

            // Antwort ID ist selectedAnswerIndex + 1 (da wir bei 1 anfangen wollen für die IDs)
            int answerId = selectedAnswerIndex + 1;

            CSVStudyLogger.Instance.LogPostTestAnswer(questionId, answerId);

            Debug.Log($"Post-Test Antwort geloggt: Q{questionId}, A{answerId}");
        }
        else
        {
            Debug.LogWarning("CSVStudyLogger.Instance ist null - Antwort konnte nicht geloggt werden!");
        }
    }

    void ShowFeedback(int selectedIndex)
    {
        // Sicherheitscheck
        if (currentIndex >= testData.questions.Count)
        {
            Debug.LogError("ShowFeedback: currentIndex überschreitet Fragenanzahl!");
            return;
        }

        TestQuestion currentQuestion = testData.questions[currentIndex];
        if (currentQuestion == null)
        {
            Debug.LogError("ShowFeedback: currentQuestion ist null!");
            return;
        }

        int correctIndex = currentQuestion.correctAnswerIndex;

        // Ändere Button-Farben für Feedback (mit Sicherheitscheck)
        if (selectedIndex < answerButtons.Length && answerButtons[selectedIndex] != null)
        {
            Image selectedButtonImage = answerButtons[selectedIndex].GetComponent<Image>();
            if (selectedButtonImage != null)
            {
                if (selectedIndex == correctIndex)
                {
                    // Richtige Antwort - Button wird grün
                    selectedButtonImage.color = correctColor;
                    Debug.Log("Richtige Antwort!");
                }
                else
                {
                    // Falsche Antwort - gewählter Button wird rot
                    selectedButtonImage.color = incorrectColor;
                    Debug.Log($"Falsche Antwort! Korrekt wäre: {correctIndex}");
                }
            }
        }

        // Korrekten Button grün färben (wenn verschieden vom gewählten)
        if (selectedIndex != correctIndex && correctIndex < answerButtons.Length && answerButtons[correctIndex] != null)
        {
            Image correctButtonImage = answerButtons[correctIndex].GetComponent<Image>();
            if (correctButtonImage != null)
            {
                correctButtonImage.color = correctColor;
            }
        }
    }

    void ProceedToNextQuestion()
    {
        currentIndex++;
        Debug.Log($"Gehe zu Post-Test Frage {currentIndex + 1} von {testData.questions.Count}");

        if (currentIndex < testData.questions.Count)
        {
            DisplayQuestion();
        }
        else
        {
            Debug.Log("Alle Post-Test Fragen beantwortet, Test wird beendet.");
            CompleteTest();
        }
    }

    void CompleteTest()
    {
        // Logge das Ende des Post-Wissenstests
        if (CSVStudyLogger.Instance != null)
        {
            CSVStudyLogger.Instance.LogPostTestEnd();
            Debug.Log("Post-Wissenstest Ende geloggt.");
        }

        Debug.Log("Post-Test abgeschlossen!");

        // Verstecke alle Quiz-Elemente
        HideQuizElements();

        // Zeige Abschlusstext
        ShowCompletionMessage();

        // Logge das Ende der gesamten Studie und erstelle CSV
        if (CSVStudyLogger.Instance != null)
        {
            CSVStudyLogger.Instance.EndSession();
            Debug.Log("CSV-Datei wurde erstellt - Studie abgeschlossen!");
        }
    }

    void HideQuizElements()
    {
        // Verstecke Progress Text
        if (progressText != null)
            progressText.gameObject.SetActive(false);

        // Verstecke Navigation Buttons
        if (nextButton != null)
            nextButton.gameObject.SetActive(false);
        if (finishButton != null)
            finishButton.gameObject.SetActive(false);

        // Verstecke alle spezifizierten Quiz-Elemente
        if (quizElements != null)
        {
            foreach (GameObject element in quizElements)
            {
                if (element != null)
                    element.SetActive(false);
            }
        }

        // Fallback: Verstecke Standard-Quiz-Elemente
        if (questionText != null)
            questionText.gameObject.SetActive(false);

        foreach (Button btn in answerButtons)
        {
            if (btn != null)
                btn.gameObject.SetActive(false);
        }
    }

    void ShowCompletionMessage()
    {
        if (completionText != null)
        {
            completionText.gameObject.SetActive(true);
            // Text wird nicht mehr automatisch gesetzt - kann im Inspector konfiguriert werden
        }
    }

    void SetAnswerButtonsInteractable(bool interactable)
    {
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] != null)
            {
                answerButtons[i].interactable = interactable;
            }
        }
    }

    // Öffentliche Methoden für zusätzliche Kontrolle
    public void EnableFeedback(bool enable)
    {
        enableFeedback = enable;
    }

    // Methode zum manuellen Fortfahren (falls in VR gewünscht)
    public void ForceNextQuestion()
    {
        if (!questionAnswered) return; // Nur wenn bereits geantwortet wurde
        ProceedToNextQuestion();
    }

    // Debug-Methoden für Testing
    public void SetCurrentQuestionIndex(int index)
    {
        if (index >= 0 && index < testData.questions.Count)
        {
            currentIndex = index;
            DisplayQuestion();
        }
    }

    public int GetCurrentQuestionIndex()
    {
        return currentIndex;
    }

    public int getTotalQuestions()
    {
        return testData.questions.Count;
    }

    // Debug-Methoden
    [ContextMenu("Debug Info")]
    public void DebugInfo()
    {
        Debug.Log($"Current Index: {currentIndex}");
        Debug.Log($"Total Questions: {(testData != null && testData.questions != null ? testData.questions.Count : 0)}");
        Debug.Log($"Answer Buttons: {answerButtons?.Length ?? 0}");
        Debug.Log($"Question Answered: {questionAnswered}");
        Debug.Log($"Header Logged: {hasLoggedPostTestHeader}");
    }
}