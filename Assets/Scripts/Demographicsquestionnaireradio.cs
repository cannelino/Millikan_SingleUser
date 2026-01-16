using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class DemographicsQuestionnaireRadio : MonoBehaviour
{
    [Header("UI References")]
    public Canvas demographicsCanvas;
    public Canvas quizCanvas; // Das ausgeblendete QuizCanvas

    [Header("Page Navigation")]
    public Button nextPageButton;
    public Button previousPageButton;
    public Button finishButton;
    public TextMeshProUGUI pageInfoText; // "Seite 1 von 3"

    [Header("Question Container")]
    public GameObject[] questionPages; // Array für verschiedene Seiten

    [Header("Page 1 - Demographics (Radio Buttons)")]
    public ToggleGroup ageToggleGroup;
    public Toggle[] ageToggles; // 15, 16, 17, 18, 19 Jahre

    public ToggleGroup genderToggleGroup;
    public Toggle[] genderToggles; // weiblich, männlich, divers, keine Angabe

    public ToggleGroup gradeToggleGroup;
    public Toggle[] gradeToggles; // 10., 11., 12. Klasse

    [Header("Page 2 - Physics Knowledge (Radio Buttons)")]
    public ToggleGroup millikansKnowledgeToggleGroup;
    public Toggle[] millikansKnowledgeToggles; // noch nie gehört bis sehr gut bekannt

    public ToggleGroup physicsInterestToggleGroup;
    public Toggle[] physicsInterestToggles; // gar nicht bis sehr stark

    public ToggleGroup electrodynamicsKnowledgeToggleGroup;
    public Toggle[] electrodynamicsKnowledgeToggles; // sehr gering bis sehr gut

    [Header("Page 3 - VR Experience (Radio Buttons)")]
    public ToggleGroup vrExperienceToggleGroup;
    public Toggle[] vrExperienceToggles; // ja, nein

    public ToggleGroup vrFrequencyToggleGroup;
    public Toggle[] vrFrequencyToggles; // nur einmal, gelegentlich, regelmäßig
    public GameObject vrFrequencySection; // Container für Häufigkeit

    private int currentPage = 0;
    private const int totalPages = 3;
    private bool hasLoggedDemographicsHeader = false;

    // Antworten-Storage
    private DemographicsData demographicsData = new DemographicsData();

    [System.Serializable]
    public class DemographicsData
    {
        public int age = -1; // Index: 0=15, 1=16, 2=17, 3=18, 4=19
        public int gender = -1; // 0=weiblich, 1=männlich, 2=divers, 3=keine Angabe
        public int grade = -1; // 0=10.Klasse, 1=11.Klasse, 2=12.Klasse
        public int millikansKnowledge = -1;
        public int physicsInterest = -1;
        public int electrodynamicsKnowledge = -1;
        public int vrExperience = -1; // 0=ja, 1=nein
        public int vrFrequency = -1;
    }

    void Start()
    {
        // Quiz Canvas ausblenden
        if (quizCanvas != null)
            quizCanvas.gameObject.SetActive(false);

        // Demographics Canvas aktivieren
        demographicsCanvas.gameObject.SetActive(true);

        // Button Events
        nextPageButton.onClick.AddListener(NextPage);
        previousPageButton.onClick.AddListener(PreviousPage);
        finishButton.onClick.AddListener(FinishDemographics);

        // Toggle Groups konfigurieren
        SetupToggleGroups();

        // Bedingte Anzeige für VR Frequency
        SetupConditionalToggles();

        // Header loggen
        LogDemographicsHeader();

        // Erste Seite anzeigen
        ShowCurrentPage();
    }

    void LogDemographicsHeader()
    {
        if (!hasLoggedDemographicsHeader)
        {
            Debug.Log("Demographics Questionnaire gestartet (Radio Button Version)");
            hasLoggedDemographicsHeader = true;
        }
    }

    void SetupToggleGroups()
    {
        // Alle Toggle Groups müssen "Allow Switch Off" = false haben
        // Das stellst du in Unity ein, nicht hier im Code

        // Optional: Alle Toggles initial deselektieren
        DeselectAllToggles(ageToggles);
        DeselectAllToggles(genderToggles);
        DeselectAllToggles(gradeToggles);
        DeselectAllToggles(millikansKnowledgeToggles);
        DeselectAllToggles(physicsInterestToggles);
        DeselectAllToggles(electrodynamicsKnowledgeToggles);
        DeselectAllToggles(vrExperienceToggles);
        DeselectAllToggles(vrFrequencyToggles);
    }

    void DeselectAllToggles(Toggle[] toggles)
    {
        foreach (var toggle in toggles)
        {
            if (toggle != null)
                toggle.isOn = false;
        }
    }

    void SetupConditionalToggles()
    {
        // VR-Häufigkeit nur anzeigen wenn VR-Erfahrung = "ja" (Index 0)
        if (vrExperienceToggles != null && vrExperienceToggles.Length >= 1 && vrExperienceToggles[0] != null)
        {
            vrExperienceToggles[0].onValueChanged.AddListener((isOn) => {
                if (vrFrequencySection != null && isOn)
                    vrFrequencySection.SetActive(true);
            });
        }

        if (vrExperienceToggles != null && vrExperienceToggles.Length >= 2 && vrExperienceToggles[1] != null)
        {
            vrExperienceToggles[1].onValueChanged.AddListener((isOn) => {
                if (vrFrequencySection != null && isOn)
                    vrFrequencySection.SetActive(false);
            });
        }

        // Initial ausblenden
        if (vrFrequencySection != null)
            vrFrequencySection.SetActive(false);
    }

    void ShowCurrentPage()
    {
        // Alle Seiten ausblenden
        for (int i = 0; i < questionPages.Length; i++)
        {
            questionPages[i].SetActive(i == currentPage);
        }

        // Seiten-Info aktualisieren
        pageInfoText.text = $"Seite {currentPage + 1} von {totalPages}";

        // Button-Status aktualisieren
        previousPageButton.interactable = currentPage > 0;
        previousPageButton.gameObject.SetActive(currentPage > 0); // Zurück-Button nur ab Seite 2 anzeigen

        bool isLastPage = currentPage >= totalPages - 1;
        nextPageButton.gameObject.SetActive(!isLastPage);
        finishButton.gameObject.SetActive(isLastPage);

        Debug.Log($"Demographics Seite {currentPage + 1} angezeigt");
    }

    void NextPage()
    {
        // Aktuelle Seite validieren und speichern
        if (ValidateCurrentPage())
        {
            SaveCurrentPageData();
            LogCurrentPageAnswers();

            currentPage++;
            if (currentPage < totalPages)
            {
                ShowCurrentPage();
            }
            else
            {
                FinishDemographics();
            }
        }
        else
        {
            Debug.LogWarning("Bitte alle Pflichtfelder ausfüllen!");
            // Optional: Zeige dem User eine Warnung in VR (z.B. Text färben oder Popup)
        }
    }

    void PreviousPage()
    {
        SaveCurrentPageData(); // Auch bei Zurück speichern

        if (currentPage > 0)
        {
            currentPage--;
            ShowCurrentPage();
        }
    }

    bool ValidateCurrentPage()
    {
        switch (currentPage)
        {
            case 0: // Demographics
                return ValidatePage1();
            case 1: // Physics Knowledge
                return ValidatePage2();
            case 2: // VR Experience
                return ValidatePage3();
            default:
                return true;
        }
    }

    bool ValidatePage1()
    {
        // Prüfe ob jeweils ein Toggle in jeder Gruppe ausgewählt ist
        bool ageSelected = IsAnyToggleSelected(ageToggles);
        bool genderSelected = IsAnyToggleSelected(genderToggles);
        bool gradeSelected = IsAnyToggleSelected(gradeToggles);

        if (!ageSelected) Debug.LogWarning("Bitte Alter auswählen!");
        if (!genderSelected) Debug.LogWarning("Bitte Geschlecht auswählen!");
        if (!gradeSelected) Debug.LogWarning("Bitte Klassenstufe auswählen!");

        return ageSelected && genderSelected && gradeSelected;
    }

    bool ValidatePage2()
    {
        bool millikansSelected = IsAnyToggleSelected(millikansKnowledgeToggles);
        bool physicsSelected = IsAnyToggleSelected(physicsInterestToggles);
        bool electroSelected = IsAnyToggleSelected(electrodynamicsKnowledgeToggles);

        if (!millikansSelected) Debug.LogWarning("Bitte Millikan-Kenntnisse auswählen!");
        if (!physicsSelected) Debug.LogWarning("Bitte Physik-Interesse auswählen!");
        if (!electroSelected) Debug.LogWarning("Bitte Elektrodynamik-Kenntnisse auswählen!");

        return millikansSelected && physicsSelected && electroSelected;
    }

    bool ValidatePage3()
    {
        bool vrExpSelected = IsAnyToggleSelected(vrExperienceToggles);

        if (!vrExpSelected)
        {
            Debug.LogWarning("Bitte VR-Erfahrung auswählen!");
            return false;
        }

        // Wenn "ja" ausgewählt (Index 0), muss auch Häufigkeit ausgewählt sein
        if (vrExperienceToggles[0].isOn)
        {
            bool vrFreqSelected = IsAnyToggleSelected(vrFrequencyToggles);
            if (!vrFreqSelected)
            {
                Debug.LogWarning("Bitte VR-Häufigkeit auswählen!");
                return false;
            }
        }

        return true;
    }

    bool IsAnyToggleSelected(Toggle[] toggles)
    {
        if (toggles == null || toggles.Length == 0)
            return false;

        foreach (var toggle in toggles)
        {
            if (toggle != null && toggle.isOn)
                return true;
        }
        return false;
    }

    int GetSelectedToggleIndex(Toggle[] toggles)
    {
        if (toggles == null || toggles.Length == 0)
            return -1;

        for (int i = 0; i < toggles.Length; i++)
        {
            if (toggles[i] != null && toggles[i].isOn)
                return i;
        }
        return -1;
    }

    void SaveCurrentPageData()
    {
        switch (currentPage)
        {
            case 0:
                demographicsData.age = GetSelectedToggleIndex(ageToggles);
                demographicsData.gender = GetSelectedToggleIndex(genderToggles);
                demographicsData.grade = GetSelectedToggleIndex(gradeToggles);
                break;

            case 1:
                demographicsData.millikansKnowledge = GetSelectedToggleIndex(millikansKnowledgeToggles);
                demographicsData.physicsInterest = GetSelectedToggleIndex(physicsInterestToggles);
                demographicsData.electrodynamicsKnowledge = GetSelectedToggleIndex(electrodynamicsKnowledgeToggles);
                break;

            case 2:
                demographicsData.vrExperience = GetSelectedToggleIndex(vrExperienceToggles);
                demographicsData.vrFrequency = GetSelectedToggleIndex(vrFrequencyToggles);
                break;
        }
    }

    void LogCurrentPageAnswers()
    {
        // Debug-Logs für Entwicklung
        switch (currentPage)
        {
            case 0:
                Debug.Log($"Demographics gespeichert: Age: {GetAgeText(demographicsData.age)}, Gender: {GetGenderText(demographicsData.gender)}, Grade: {GetGradeText(demographicsData.grade)}");
                break;

            case 1:
                Debug.Log($"Physics Knowledge gespeichert: Millikan: {GetMillikanKnowledgeText(demographicsData.millikansKnowledge)}, Interest: {GetPhysicsInterestText(demographicsData.physicsInterest)}, Electro: {GetElectrodynamicsText(demographicsData.electrodynamicsKnowledge)}");
                break;

            case 2:
                Debug.Log($"VR Experience gespeichert: Experience: {(demographicsData.vrExperience == 0 ? "ja" : "nein")}, Frequency: {GetVRFrequencyText(demographicsData.vrFrequency)}");

                // Am Ende alle Demographics Daten an CSV Logger senden
                if (CSVStudyLogger.Instance != null)
                {
                    CSVStudyLogger.Instance.LogDemographics(
                        demographicsData.age,
                        demographicsData.gender,
                        demographicsData.grade,
                        demographicsData.millikansKnowledge,
                        demographicsData.physicsInterest,
                        demographicsData.electrodynamicsKnowledge,
                        demographicsData.vrExperience,
                        demographicsData.vrFrequency
                    );
                    Debug.Log("Alle Demographics Daten an CSV Logger gesendet");
                }
                break;
        }
    }

    // Text-Mappings für Logging
    string GetAgeText(int index)
    {
        string[] options = { "15 Jahre", "16 Jahre", "17 Jahre", "18 Jahre", "19 Jahre" };
        return index >= 0 && index < options.Length ? options[index] : "unknown";
    }

    string GetGenderText(int index)
    {
        string[] options = { "weiblich", "männlich", "divers", "keine Angabe" };
        return index >= 0 && index < options.Length ? options[index] : "unknown";
    }

    string GetGradeText(int index)
    {
        string[] options = { "10. Klasse", "11. Klasse", "12. Klasse" };
        return index >= 0 && index < options.Length ? options[index] : "unknown";
    }

    string GetMillikanKnowledgeText(int index)
    {
        string[] options = { "noch nie gehört", "gehört, keine Details", "grundsätzlich bekannt", "gut bekannt", "sehr gut bekannt" };
        return index >= 0 && index < options.Length ? options[index] : "unknown";
    }

    string GetPhysicsInterestText(int index)
    {
        string[] options = { "gar nicht", "eher wenig", "neutral", "eher stark", "sehr stark" };
        return index >= 0 && index < options.Length ? options[index] : "unknown";
    }

    string GetElectrodynamicsText(int index)
    {
        string[] options = { "sehr gering", "gering", "durchschnittlich", "gut", "sehr gut" };
        return index >= 0 && index < options.Length ? options[index] : "unknown";
    }

    string GetVRFrequencyText(int index)
    {
        string[] options = { "nur einmal", "gelegentlich", "regelmäßig" };
        return index >= 0 && index < options.Length ? options[index] : "unknown";
    }

    void FinishDemographics()
    {
        SaveCurrentPageData();
        LogCurrentPageAnswers();

        Debug.Log("Demographics abgeschlossen. Wechsle zum Quiz.");

        // Wechsle zum Quiz
        demographicsCanvas.gameObject.SetActive(false);
        if (quizCanvas != null)
            quizCanvas.gameObject.SetActive(true);
    }

    // Debug-Methoden
    [ContextMenu("Show Current Data")]
    public void ShowCurrentData()
    {
        Debug.Log($"Age: {GetAgeText(demographicsData.age)}, Gender: {GetGenderText(demographicsData.gender)}, Grade: {GetGradeText(demographicsData.grade)}");
        Debug.Log($"Millikan: {GetMillikanKnowledgeText(demographicsData.millikansKnowledge)}, Physics: {GetPhysicsInterestText(demographicsData.physicsInterest)}, Electro: {GetElectrodynamicsText(demographicsData.electrodynamicsKnowledge)}");
        Debug.Log($"VR Experience: {(demographicsData.vrExperience == 0 ? "ja" : "nein")}, VR Frequency: {GetVRFrequencyText(demographicsData.vrFrequency)}");
    }
}