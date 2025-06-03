using System.Collections.Generic;
using UnityEngine;

public class TestData : MonoBehaviour
{
    public List<TestQuestion> questions = new List<TestQuestion>();

    void Awake()
    {
        questions.Add(new TestQuestion {
            questionText = "Welche Körper untersuchte MILLIKAN in seinem Versuch?",
            answers = new string[] { "Staubteilchen", "Öltröpfchen", "Elektronen" },
            correctAnswerIndex = 2
        });
        questions.Add(new TestQuestion {
            questionText = "Welche typischen Eigenschaften hatten die Öltröpchen, die MILLIKAN in seinem Versuch beobachtete?",
            answers = new string[] { "Form einer Kugel", "Sehr hohe Dichte", "Form eines Zylinders" },
            correctAnswerIndex = 1
        });
        questions.Add(new TestQuestion
        {
            questionText = "Welches Feld ließ MILLIKAN unter anderem auf die Öltröpfchen wirken?",
            answers = new string[] { "Gravitationsfeld", "Hydrodynamisches Feld", "Magnetisches Feld" },
            correctAnswerIndex = 1
        });
        questions.Add(new TestQuestion
        {
            questionText = "Welche Kräfte wirkten unter anderem auf die Öltröpfchen während der verschiedenen Versuchsphasen?",
            answers = new string[] { "Magnetische Kraft", "STOKESsche Reibungskraft", "Kohäsionskraft" },
            correctAnswerIndex = 2
        });
        questions.Add(new TestQuestion
        {
            questionText = "Welche Richtungen hatten die Kräfte, die auf die Öltröpfchen wirkten?",
            answers = new string[] { "Die STOKESsche Reibungskraft wirkte stets nach oben.", "Die Elektrische Kraft wirkte stets nach unten.", "Die STOKESsche Reibungskraft wirkte stets entgegen der Bewegungsrichtung." },
            correctAnswerIndex = 3
        });
        questions.Add(new TestQuestion
        {
            questionText = "Welche Einflussmöglichkeiten hatte MILLIKAN auf das Elektrische Feld?",
            answers = new string[] { "Er konnte die Frequenz des Elektrischen Feldes verändern.", "Er konnte die Geometrie des Elektrischen Feldes verändern.", "Er konnte die Richtung des Elektrischen Feldes verändern." },
            correctAnswerIndex = 3
        });
        questions.Add(new TestQuestion
        {
            questionText = "Welche physikalische Bedingung konnte MILLIKAN unter anderem mit seiner Gleichfeldmethode herstellen?",
            answers = new string[] { "Schweben mit elektrischem Feld", "Steigen ohne elektrisches Feld", "Schweben ohne elektrisches Feld" },
            correctAnswerIndex = 1
        });
        questions.Add(new TestQuestion
        {
            questionText = "Welche physikalische Größe konnte im Versuch direkt gemessen werden?",
            answers = new string[] { "Tröpfchenladung", "Angelegte Spannung", "Elektrische Feldstärke" },
            correctAnswerIndex = 2
        });
        questions.Add(new TestQuestion
        {
            questionText = "Welche physikalische Größe konnte Tabellenwerken entnommen werden?",
            answers = new string[] { "Dichte von Öl", "Tröpfchenmasse", "Tröpfchenladung" },
            correctAnswerIndex = 1
        });
        questions.Add(new TestQuestion
        {
            questionText = "Von den beiden Methoden ist die Methode 'Fallen und Steigen mit Feld' der Methode 'Schweben und Fallen ohne Feld' vorzuziehen. Warum?",
            answers = new string[] { "Beim Fallen ohne Feld herrscht kein elektrisches Feld. Dieses ist aber unbedingt nötig, um das Experiment auswerten zu können.", "Bei der Schwebemethode ist wegen der BROWNschen Molekularbewegung und der damit verbundenen Zitterbewegung des Öltröpfchens der Schwebezustand schwer zu messen.", "Bei der Schwebemethode wirkt die STOKESsche Reibungskraft nicht. Diese ist aber unbedingt nötig, um das Experiment auswerten zu können." },
            correctAnswerIndex = 2
        });
    }
}