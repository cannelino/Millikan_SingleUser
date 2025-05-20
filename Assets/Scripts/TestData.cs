using System.Collections.Generic;
using UnityEngine;

public class TestData : MonoBehaviour
{
    public List<TestQuestion> questions = new List<TestQuestion>();

    void Awake()
    {
        questions.Add(new TestQuestion {
            questionText = "Was misst das Millikan-Experiment?",
            answers = new string[] { "Masse eines Elektrons", "Elementarladung", "Spannung" },
            correctAnswerIndex = 1
        });
        questions.Add(new TestQuestion {
            questionText = "Warum schweben Öltröpfchen im Experiment?",
            answers = new string[] { "Luftwiderstand", "Gleichgewicht der Kräfte", "Hitze" },
            correctAnswerIndex = 1
        });
    }
}