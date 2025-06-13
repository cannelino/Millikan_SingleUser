using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AdvancedInteractionLogger : MonoBehaviour
{
    [Header("Manual Button Registration")]
    [SerializeField] private List<LoggableButton> specificButtons = new List<LoggableButton>();
    [Tooltip("Trage hier spezifische Buttons mit custom IDs ein")]

    [Header("Auto-Registration Settings")]
    [SerializeField] private bool autoRegisterAllButtons = true;
    [SerializeField] private bool logInteractionStart = true;
    [SerializeField] private string[] excludeButtonNames = { "CloseButton", "BackButton" };
    [Tooltip("Button-Namen die NICHT automatisch registriert werden sollen")]

    private bool hasLoggedInteractionHeader = false;
    private HashSet<Button> registeredButtons = new HashSet<Button>();

    void Start()
    {
        // Logge Interaction Header beim ersten Start
        if (logInteractionStart)
        {
            LogInteractionHeader();
        }

        // Registriere spezifische Buttons zuerst
        RegisterSpecificButtons();

        // Dann automatische Registrierung falls gewünscht
        if (autoRegisterAllButtons)
        {
            RegisterAllButtons();
        }
    }

    void LogInteractionHeader()
    {
        if (!hasLoggedInteractionHeader && StudyLogger.Instance != null)
        {
            StudyLogger.Instance.LogEvent("INTERACTION_FRAGMENTS_START", "#Interaction_Fragments#");
            hasLoggedInteractionHeader = true;
            Debug.Log("Interaction Fragments Phase gestartet und geloggt.");
        }
    }

    void RegisterSpecificButtons()
    {
        foreach (LoggableButton loggableBtn in specificButtons)
        {
            if (loggableBtn.button != null)
            {
                RegisterButton(loggableBtn.button, loggableBtn.customID);
            }
        }
    }

    void RegisterAllButtons()
    {
        Button[] allButtons = FindObjectsOfType<Button>();
        Debug.Log($"Gefundene Buttons in Szene: {allButtons.Length}");

        foreach (Button button in allButtons)
        {
            // Überspringe bereits registrierte Buttons
            if (registeredButtons.Contains(button))
                continue;

            // Überspringe ausgeschlossene Buttons
            if (ShouldExcludeButton(button))
            {
                Debug.Log($"Button ausgeschlossen: {button.name}");
                continue;
            }

            RegisterButton(button);
        }
    }

    bool ShouldExcludeButton(Button button)
    {
        string buttonName = button.gameObject.name;

        foreach (string excludeName in excludeButtonNames)
        {
            if (buttonName.Contains(excludeName))
                return true;
        }

        return false;
    }

    public void RegisterButton(Button button, string customButtonID = "")
    {
        if (button == null || registeredButtons.Contains(button))
            return;

        string buttonID = GetButtonID(button, customButtonID);

        button.onClick.AddListener(() => LogButtonInteraction(buttonID, button));
        registeredButtons.Add(button);

        Debug.Log($"Button registriert für Logging: {buttonID}");
    }

    string GetButtonID(Button button, string customID = "")
    {
        if (!string.IsNullOrEmpty(customID))
            return customID;

        string buttonID = button.gameObject.name;

        // Text hinzufügen falls vorhanden
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null && !string.IsNullOrEmpty(buttonText.text))
        {
            buttonID += $"_({buttonText.text.Replace(" ", "_")})";
        }
        else
        {
            Text legacyText = button.GetComponentInChildren<Text>();
            if (legacyText != null && !string.IsNullOrEmpty(legacyText.text))
            {
                buttonID += $"_({legacyText.text.Replace(" ", "_")})";
            }
        }

        return buttonID;
    }

    public void LogButtonInteraction(string buttonID, Button button = null)
    {
        if (StudyLogger.Instance != null)
        {
            string details = $"ButtonID: {buttonID}";
            string additionalData = "";

            if (button != null)
            {
                Vector3 worldPos = button.transform.position;
                additionalData += $"Position: ({worldPos.x:F2}, {worldPos.y:F2}, {worldPos.z:F2})";

                // Button-Text
                TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null && !string.IsNullOrEmpty(buttonText.text))
                {
                    additionalData += $" | Text: '{buttonText.text}'";
                }

                // Szenen-Info
                additionalData += $" | Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}";
            }

            StudyLogger.Instance.LogEvent("BUTTON_INTERACTION", details, additionalData);
            Debug.Log($"Button-Interaktion geloggt: {buttonID}");
        }
    }

    // Öffentliche Methoden für externe Verwendung
    public void LogInteractionEnd()
    {
        if (StudyLogger.Instance != null)
        {
            StudyLogger.Instance.LogEvent("INTERACTION_FRAGMENTS_END", "Interaction Phase beendet");
        }
    }

    public void AddButtonToTracking(Button button, string customID = "")
    {
        RegisterButton(button, customID);
    }

    public void RemoveButtonFromTracking(Button button)
    {
        if (button != null && registeredButtons.Contains(button))
        {
            registeredButtons.Remove(button);
            // Note: onClick Listener können nicht einfach entfernt werden ohne Referenz
            Debug.Log($"Button aus Tracking entfernt: {button.name}");
        }
    }

    // Utility Methode für manuelle Logs
    public void LogCustomButtonClick(string buttonID, string additionalInfo = "")
    {
        if (StudyLogger.Instance != null)
        {
            StudyLogger.Instance.LogEvent("BUTTON_INTERACTION", $"ButtonID: {buttonID}", additionalInfo);
        }
    }

    // Debug Methods
    [ContextMenu("Show Registered Buttons")]
    public void ShowRegisteredButtons()
    {
        Debug.Log($"Anzahl registrierter Buttons: {registeredButtons.Count}");
        foreach (Button btn in registeredButtons)
        {
            if (btn != null)
                Debug.Log($"- {btn.name}");
        }
    }
}