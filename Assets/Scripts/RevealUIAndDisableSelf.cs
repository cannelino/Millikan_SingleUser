using UnityEngine;

public class RevealUIAndDisableSelf : MonoBehaviour
{
    [Tooltip("UI-Elemente (Text, Image etc.), die aktiviert werden sollen")]
    public GameObject[] elementsToEnable;

    [Tooltip("Weitere UI-Elemente, die nach dem Klick zusätzlich aktiviert werden sollen (z. B. nächste Buttons)")]
    public GameObject[] elementsToUnhideAfterClick;

    public void Reveal()
    {
        // Hauptinhalte aktivieren
        foreach (GameObject element in elementsToEnable)
        {
            if (element != null)
                element.SetActive(true);
        }

        // Zusätzliche Elemente (z. B. nachfolgender Button) aktivieren
        foreach (GameObject next in elementsToUnhideAfterClick)
        {
            if (next != null)
                next.SetActive(true);
        }

        // Sich selbst deaktivieren
        gameObject.SetActive(false);
    }
}
