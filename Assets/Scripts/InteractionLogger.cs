using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InteractionLogger : MonoBehaviour
{
    [Header("Interaction Logging Settings")]
    [SerializeField] private bool logInteractionStart = true;
    [Tooltip("Loggt automatisch beim Start, dass die Interaction-Phase begonnen hat")]

    private bool hasLoggedInteractionHeader = false;

    void Start()
    {
        // Logge Interaction Header beim ersten Start
        if (logInteractionStart)
        {
            LogInteractionHeader();
        }

        // Finde alle Buttons in der Szene und registriere sie automatisch
        RegisterAllButtons();
    }

    void LogInteractionHeader()
    {
        if (!hasLoggedInteractionHeader && StudyLogger.Instance != null)
        {
            StudyLogger.Instance.LogEvent("INTERACTION_FRAGMENTS_START", "#Interaction_Fragments#");
            hasLoggedInteractionHeader = true;
            Debug.Log("Interaction Fragments Phase gestartet und geloggt.");
        }
        else if (StudyLogger.Instance == null)
        {
            Debug.LogWarning("StudyLogger.Instance ist null - Header konnte nicht geloggt werden!");
        }
    }

    /// <summary>
    /// Findet automatisch alle Buttons in der Szene und registriert sie für Logging
    /// </summary>
    void RegisterAllButtons()
    {
        // Finde alle Button-Komponenten in der Szene
        Button[] allButtons = FindObjectsOfType<Button>();

        Debug.Log($"Gefundene Buttons in Szene: {allButtons.Length}");

        foreach (Button button in allButtons)
        {
            RegisterButton(button);
        }
    }

    /// <summary>
    /// Registriert einen einzelnen Button für Interaction-Logging
    /// </summary>
    /// <param name="button">Der zu registrierende Button</param>
    /// <param name="customButtonID">Optionale custom ID für den Button</param>
    public void RegisterButton(Button button, string customButtonID = "")
    {
        if (button == null)
        {
            Debug.LogWarning("Button ist null - kann nicht registriert werden!");
            return;
        }

        // Bestimme Button-ID (Priorität: customButtonID > GameObject Name > Position)
        string buttonID = GetButtonID(button, customButtonID);

        // Entferne existierende Listener für diesen spezifischen Log-Call (falls vorhanden)
        // und füge neuen hinzu
        button.onClick.AddListener(() => LogButtonInteraction(buttonID, button));

        Debug.Log($"Button registriert für Logging: {buttonID}");
    }

    /// <summary>
    /// Bestimmt die ID für einen Button basierend auf verschiedenen Kriterien
    /// </summary>
    string GetButtonID(Button button, string customID = "")
    {
        if (!string.IsNullOrEmpty(customID))
        {
            return customID;
        }

        // Versuche verschiedene Methoden um eine sinnvolle ID zu finden
        string buttonID = button.gameObject.name;

        // Falls der Button Text hat, verwende diesen auch
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null && !string.IsNullOrEmpty(buttonText.text))
        {
            buttonID += $"_({buttonText.text})";
        }
        else
        {
            // Fallback: Verwende auch Text-Komponente
            Text legacyText = button.GetComponentInChildren<Text>();
            if (legacyText != null && !string.IsNullOrEmpty(legacyText.text))
            {
                buttonID += $"_({legacyText.text})";
            }
        }

        return buttonID;
    }

    /// <summary>
    /// Loggt die Button-Interaktion
    /// </summary>
    /// <param name="buttonID">ID des gedrückten Buttons</param>
    /// <param name="button">Button-Referenz für zusätzliche Daten</param>
    public void LogButtonInteraction(string buttonID, Button button = null)
    {
        if (StudyLogger.Instance != null)
        {
            string details = $"ButtonID: {buttonID}";
            string additionalData = "";

            // Sammle zusätzliche Button-Informationen falls verfügbar
            if (button != null)
            {
                // Position des Buttons
                Vector3 worldPos = button.transform.position;
                additionalData += $"Position: {worldPos}";

                // Button-Text falls vorhanden
                TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null && !string.IsNullOrEmpty(buttonText.text))
                {
                    additionalData += $" | Text: {buttonText.text}";
                }
                else
                {
                    Text legacyText = button.GetComponentInChildren<Text>();
                    if (legacyText != null && !string.IsNullOrEmpty(legacyText.text))
                    {
                        additionalData += $" | Text: {legacyText.text}";
                    }
                }

                // Parent-Objekt Info falls relevant
                if (button.transform.parent != null)
                {
                    additionalData += $" | Parent: {button.transform.parent.name}";
                }
            }

            StudyLogger.Instance.LogEvent("BUTTON_INTERACTION", details, additionalData);
            Debug.Log($"Button-Interaktion geloggt: {buttonID}");
        }
        else
        {
            Debug.LogWarning("StudyLogger.Instance ist null - Button-Interaktion konnte nicht geloggt werden!");
        }
    }

    /// <summary>
    /// Manuelle Registrierung eines Buttons mit custom ID
    /// Rufe diese Methode auf, wenn du spezifische Button-IDs haben möchtest
    /// </summary>
    /// <param name="button">Der Button</param>
    /// <param name="customID">Die gewünschte ID</param>
    public void RegisterButtonWithCustomID(Button button, string customID)
    {
        RegisterButton(button, customID);
    }

    /// <summary>
    /// Loggt das Ende der Interaction-Phase (optional)
    /// </summary>
    public void LogInteractionEnd()
    {
        if (StudyLogger.Instance != null)
        {
            StudyLogger.Instance.LogEvent("INTERACTION_FRAGMENTS_END", "Interaction Phase beendet");
            Debug.Log("Interaction Fragments Phase beendet und geloggt.");
        }
    }

    /// <summary>
    /// Hilfsmethode: Loggt eine custom Interaction ohne Button-Referenz
    /// </summary>
    /// <param name="interactionType">Art der Interaktion</param>
    /// <param name="objectID">ID des Objekts</param>
    /// <param name="additionalInfo">Zusätzliche Informationen</param>
    public void LogCustomInteraction(string interactionType, string objectID, string additionalInfo = "")
    {
        if (StudyLogger.Instance != null)
        {
            string details = $"Type: {interactionType} | ObjectID: {objectID}";
            StudyLogger.Instance.LogEvent("CUSTOM_INTERACTION", details, additionalInfo);
            Debug.Log($"Custom Interaktion geloggt: {interactionType} - {objectID}");
        }
    }

    // Debug-Methoden
    [ContextMenu("Log Interaction Header")]
    public void ForceLogInteractionHeader()
    {
        hasLoggedInteractionHeader = false; // Reset flag
        LogInteractionHeader();
    }

    [ContextMenu("Register All Buttons")]
    public void ForceRegisterAllButtons()
    {
        RegisterAllButtons();
    }

    [ContextMenu("Test Button Logging")]
    public void TestButtonLogging()
    {
        LogButtonInteraction("TEST_BUTTON_" + System.DateTime.Now.Ticks);
    }
}

// Zusätzliche Hilfsklasse für erweiterte Button-Informationen
[System.Serializable]
public class LoggableButton
{
    public Button button;
    public string customID;
    public string description;

    public LoggableButton(Button btn, string id = "", string desc = "")
    {
        button = btn;
        customID = id;
        description = desc;
    }
}
