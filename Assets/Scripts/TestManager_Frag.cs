using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TestManager_Frag : MonoBehaviour
{
    [Header("Basic Quiz Settings")]
    public TestData testData;
    public TextMeshProUGUI questionText;
    public Button[] answerButtons;

    [Header("Feedback Settings")]
    public bool enableFeedback = false; // Toggle für Feedback ein/aus
    public Color correctColor = Color.green;
    public Color incorrectColor = Color.red;
    public Color defaultButtonColor = Color.white;
    public float feedbackDuration = 3f;

    [Header("Optional Feedback UI")]
    public GameObject feedbackPanel; // Optional: Panel für "Nächste Frage in X Sekunden"
    public TextMeshProUGUI feedbackText; // Optional: Countdown-Text

    [Header("Scene Management")]
    [SerializeField] private string nextSceneName = "Millikan_ExplanationRoom_Fragmented";
    [SerializeField] private int nextSceneIndex = 2; // Fallback falls Name nicht funktioniert

    private int currentIndex = 0;
    private bool isShowingFeedback = false;
    private Color[] originalButtonColors; // Speichere die ursprünglichen Button-Farben
    private bool hasLoggedPreTestHeader = false; // Flag um Header nur einmal zu loggen

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

        // Logge Pre-Wissenstest Header beim ersten Start
        LogPreTestHeader();

        // Starte Pre-Test Logging
        if (CSVStudyLogger.Instance != null)
        {
            CSVStudyLogger.Instance.LogPreTestStart();
        }

        DisplayQuestion();
    }

    void LogPreTestHeader()
    {
        if (!hasLoggedPreTestHeader)
        {
            Debug.Log("Pre-Wissenstest (Fragment) gestartet");
            hasLoggedPreTestHeader = true;
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

        // Verstecke Feedback-Panel falls vorhanden
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }

        isShowingFeedback = false;

        Debug.Log($"Frage {currentIndex + 1} von {testData.questions.Count} angezeigt: {q.questionText}");
    }

    void Answer(int index)
    {
        if (isShowingFeedback) return; // Verhindere mehrfache Eingaben während Feedback

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

        Debug.Log($"Antwort gewählt: {index}");

        // Logge die Antwort mit CSV Logger
        LogQuizAnswer(index);

        if (enableFeedback)
        {
            // Deaktiviere Buttons während Feedback
            SetButtonsInteractable(false);
            StartCoroutine(ShowFeedbackAndContinue(index));
        }
        else
        {
            // Ursprüngliches Verhalten - direkt zur nächsten Frage
            ProceedToNextQuestion();
        }
    }

    void LogQuizAnswer(int selectedAnswerIndex)
    {
        // CSV Logging
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

            CSVStudyLogger.Instance.LogPreTestAnswer(questionId, answerId);

            Debug.Log($"Pre-Test (Fragment) Antwort geloggt: Q{questionId}, A{answerId}");
        }
        else
        {
            Debug.LogWarning("CSVStudyLogger.Instance ist null - Antwort konnte nicht geloggt werden!");
        }
    }

    IEnumerator ShowFeedbackAndContinue(int selectedIndex)
    {
        isShowingFeedback = true;

        // Sicherheitscheck
        if (currentIndex >= testData.questions.Count)
        {
            Debug.LogError("ShowFeedbackAndContinue: currentIndex überschreitet Fragenanzahl!");
            CompleteTest();
            yield break;
        }

        TestQuestion currentQuestion = testData.questions[currentIndex];
        if (currentQuestion == null)
        {
            Debug.LogError("ShowFeedbackAndContinue: currentQuestion ist null!");
            CompleteTest();
            yield break;
        }

        int correctIndex = currentQuestion.correctAnswerIndex;
        bool isLastQuestion = (currentIndex >= testData.questions.Count - 1);

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

        // Angepasste Wartezeit für letzte Frage
        float waitTime = isLastQuestion ? feedbackDuration + 1f : feedbackDuration;
        string countdownText = isLastQuestion ? "Test wird beendet in {0} Sekunden..." : "Nächste Frage in {0} Sekunden...";

        // Optional: Zeige Countdown-Text
        if (feedbackPanel != null && feedbackText != null)
        {
            feedbackPanel.SetActive(true);

            for (int i = (int)waitTime; i > 0; i--)
            {
                feedbackText.text = string.Format(countdownText, i);
                yield return new WaitForSeconds(1f);
            }

            feedbackPanel.SetActive(false);
        }
        else
        {
            // Warte die angepasste Feedback-Dauer ab
            yield return new WaitForSeconds(waitTime);
        }

        // Gehe zur nächsten Frage oder beende Test
        if (isLastQuestion)
        {
            CompleteTest();
        }
        else
        {
            ProceedToNextQuestion();
        }
    }

    void ProceedToNextQuestion()
    {
        currentIndex++;
        Debug.Log($"Gehe zu Frage {currentIndex + 1} von {testData.questions.Count}");

        if (currentIndex < testData.questions.Count)
        {
            DisplayQuestion();
        }
        else
        {
            Debug.Log("Alle Fragen beantwortet, Test wird beendet.");
            CompleteTest();
        }
    }

    void CompleteTest()
    {
        // Logge das Ende des Pre-Wissenstests
        if (CSVStudyLogger.Instance != null)
        {
            CSVStudyLogger.Instance.LogPreTestEnd();
            Debug.Log("Pre-Wissenstest (Fragment) Ende geloggt.");
        }

        Debug.Log("Test abgeschlossen. Szene wird geladen...");

        try
        {
            // Versuche zuerst mit Szenen-Namen zu laden
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                Debug.Log($"Lade Szene: {nextSceneName}");
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                // Fallback: Lade mit Index
                Debug.Log($"Lade Szene mit Index: {nextSceneIndex}");
                SceneManager.LoadScene(nextSceneIndex);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Fehler beim Laden der Szene: {e.Message}");

            // Versuche Fallback-Methode
            try
            {
                Debug.Log($"Versuche Fallback - Lade Szene mit Index: {nextSceneIndex}");
                SceneManager.LoadScene(nextSceneIndex);
            }
            catch (System.Exception fallbackError)
            {
                Debug.LogError($"Auch Fallback fehlgeschlagen: {fallbackError.Message}");
            }
        }
    }

    void SetButtonsInteractable(bool interactable)
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

    public void SetFeedbackDuration(float duration)
    {
        feedbackDuration = duration;
    }

    // Methode zum manuellen Fortfahren (falls in VR gewünscht)
    public void ForceNextQuestion()
    {
        if (isShowingFeedback)
        {
            StopAllCoroutines();
            ProceedToNextQuestion();
        }
    }

    // Debug-Methoden
    [ContextMenu("Debug Info")]
    public void DebugInfo()
    {
        Debug.Log($"Current Index: {currentIndex}");
        Debug.Log($"Total Questions: {(testData != null && testData.questions != null ? testData.questions.Count : 0)}");
        Debug.Log($"Answer Buttons: {answerButtons?.Length ?? 0}");
        Debug.Log($"Header Logged: {hasLoggedPreTestHeader}");
    }

    // Setze Szenen-Informationen zur Laufzeit
    public void SetNextScene(string sceneName, int sceneIndex = -1)
    {
        nextSceneName = sceneName;
        if (sceneIndex >= 0)
        {
            nextSceneIndex = sceneIndex;
        }
    }
}