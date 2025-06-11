using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TestManager : MonoBehaviour
{
    public TestData testData;
    public TextMeshProUGUI questionText;
    public Button[] answerButtons;

    private int currentIndex = 0;

    void Start()
    {
        DisplayQuestion();
    }

    void DisplayQuestion()
    {
        TestQuestion q = testData.questions[currentIndex];
        questionText.text = q.questionText;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i;
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = q.answers[i];
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => Answer(index));
        }
    }

    void Answer(int index)
    {
        Debug.Log($"Antwort gewählt: {index}");
        currentIndex++;

        if (currentIndex < testData.questions.Count)
        {
            DisplayQuestion();
        }
        else
        {
            Debug.Log("Test abgeschlossen. Szene wird geladen...");
            SceneManager.LoadScene("Millikan_ExplanationRoom_Complete");
        }
    }
}