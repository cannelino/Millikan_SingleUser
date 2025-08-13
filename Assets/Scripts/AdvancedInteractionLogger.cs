using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AdvancedInteractionLogger : MonoBehaviour
{
    [Header("Manual Button Registration")]
    [SerializeField] private List<LoggableButton> specificButtons = new List<LoggableButton>();

    [Header("Auto-Registration Settings")]
    [SerializeField] private bool autoRegisterAllButtons = true;
    [SerializeField] private bool logInteractionStart = true;
    [SerializeField] private string[] excludeButtonNames = { "CloseButton", "BackButton" };

    private bool hasLoggedInteractionHeader = false;
    private HashSet<Button> registeredButtons = new HashSet<Button>();
    private int buttonCounter = 1;

    void Start()
    {
        if (logInteractionStart)
        {
            LogInteractionHeader();
        }

        RegisterSpecificButtons();

        if (autoRegisterAllButtons)
        {
            RegisterAllButtons();
        }
    }

    void LogInteractionHeader()
    {
        if (!hasLoggedInteractionHeader)
        {
            Debug.Log("Interaction Fragments Phase gestartet");
            hasLoggedInteractionHeader = true;
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
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        Debug.Log($"Gefundene Buttons in Szene: {allButtons.Length}");

        foreach (Button button in allButtons)
        {
            if (registeredButtons.Contains(button))
                continue;

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

        Debug.Log($"Button registriert für CSV-Logging: {buttonID}");
    }

    string GetButtonID(Button button, string customID = "")
    {
        if (!string.IsNullOrEmpty(customID))
            return customID;

        string buttonID = button.gameObject.name;

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
        if (CSVStudyLogger.Instance != null)
        {
            CSVStudyLogger.Instance.LogButtonClick(buttonCounter);
            Debug.Log($"Button {buttonCounter} geklickt und an CSV geloggt: {buttonID}");

            buttonCounter++;

            if (buttonCounter > 7)
            {
                Debug.LogWarning("Mehr als 7 Button-Clicks! Weitere Clicks werden nicht geloggt.");
            }
        }
        else
        {
            Debug.LogWarning("CSVStudyLogger.Instance ist null - Button-Click konnte nicht geloggt werden!");
        }

        if (button != null)
        {
            Vector3 worldPos = button.transform.position;
            Debug.Log($"Button Details - ID: {buttonID}, Position: ({worldPos.x:F2}, {worldPos.y:F2}, {worldPos.z:F2})");
        }
    }

    public void LogCustomButtonClick(string buttonID, string additionalInfo = "")
    {
        if (CSVStudyLogger.Instance != null)
        {
            CSVStudyLogger.Instance.LogButtonClick(buttonCounter);
            Debug.Log($"Custom Button {buttonCounter} geklickt: {buttonID} - {additionalInfo}");
            buttonCounter++;
        }
    }

    [ContextMenu("Show Registered Buttons")]
    public void ShowRegisteredButtons()
    {
        Debug.Log($"Anzahl registrierter Buttons: {registeredButtons.Count}");
        foreach (Button btn in registeredButtons)
        {
            if (btn != null)
                Debug.Log($"- {btn.name}");
        }
        Debug.Log($"Nächste Button-Nummer: {buttonCounter}");
    }
}

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