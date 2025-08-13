using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class DemographicsQuestionnaire : MonoBehaviour
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

    [Header("Page 1 - Demographics")]
    public TMP_Dropdown ageDropdown; // Alter als Dropdown
    public TMP_Dropdown genderDropdown;
    public TMP_Dropdown gradeDropdown; // Klassenstufe Dropdown
                                       // Education entfernt - alle sind Schüler*innen

    [Header("Page 2 - Physics Knowledge")]
    public TMP_Dropdown millikansKnowledgeDropdown;
    public TMP_Dropdown physicsInterestDropdown;
    public TMP_Dropdown electrodynamicsKnowledgeDropdown;

    [Header("Page 3 - VR Experience")]
    public TMP_Dropdown vrExperienceDropdown;
    public TMP_Dropdown vrFrequencyDropdown;
    public GameObject vrFrequencySection; // Container für Häufigkeit

    private int currentPage = 0;
    private const int totalPages = 3;
    private bool hasLoggedDemographicsHeader = false;

    // Antworten-Storage
    private DemographicsData demographicsData = new DemographicsData();

    [System.Serializable]
    public class DemographicsData
    {
        public int age = 0;
        public int gender = -1; // 0=weiblich, 1=männlich, 2=divers, 3=keine Angabe
        public int grade = 0; // Klassenstufe (alle sind Schüler*innen)
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

        // Dropdown Options setzen
        SetupDropdowns();

        // Dropdown Events für bedingte Anzeige
        SetupConditionalDropdowns();

        // Header loggen
        LogDemographicsHeader();

        // Erste Seite anzeigen
        ShowCurrentPage();
    }

    void LogDemographicsHeader()
    {
        if (!hasLoggedDemographicsHeader)
        {
            Debug.Log("Demographics Questionnaire gestartet");
            hasLoggedDemographicsHeader = true;
        }
    }

    void SetupDropdowns()
    {
        // Age Dropdown
        ageDropdown.options.Clear();
        ageDropdown.options.Add(new TMP_Dropdown.OptionData("Bitte wählen..."));
        for (int i = 10; i <= 20; i++)
        {
            ageDropdown.options.Add(new TMP_Dropdown.OptionData($"{i} Jahre"));
        }
        ageDropdown.options.Add(new TMP_Dropdown.OptionData("Älter"));
        ageDropdown.value = 0;

        // Gender Dropdown
        genderDropdown.options.Clear();
        genderDropdown.options.Add(new TMP_Dropdown.OptionData("Bitte wählen..."));
        genderDropdown.options.Add(new TMP_Dropdown.OptionData("weiblich"));
        genderDropdown.options.Add(new TMP_Dropdown.OptionData("männlich"));
        genderDropdown.options.Add(new TMP_Dropdown.OptionData("divers"));
        genderDropdown.options.Add(new TMP_Dropdown.OptionData("keine Angabe"));
        genderDropdown.value = 0;

        // Grade Dropdown (Klassenstufe) - alle sind Schüler*innen
        gradeDropdown.options.Clear();
        gradeDropdown.options.Add(new TMP_Dropdown.OptionData("Bitte wählen..."));
        gradeDropdown.options.Add(new TMP_Dropdown.OptionData("5. Klasse"));
        gradeDropdown.options.Add(new TMP_Dropdown.OptionData("6. Klasse"));
        gradeDropdown.options.Add(new TMP_Dropdown.OptionData("7. Klasse"));
        gradeDropdown.options.Add(new TMP_Dropdown.OptionData("8. Klasse"));
        gradeDropdown.options.Add(new TMP_Dropdown.OptionData("9. Klasse"));
        gradeDropdown.options.Add(new TMP_Dropdown.OptionData("10. Klasse"));
        gradeDropdown.options.Add(new TMP_Dropdown.OptionData("11. Klasse"));
        gradeDropdown.options.Add(new TMP_Dropdown.OptionData("12. Klasse"));
        gradeDropdown.options.Add(new TMP_Dropdown.OptionData("13. Klasse"));
        gradeDropdown.value = 0;

        // Millikan Knowledge Dropdown "noch nie gehört", "gehört, keine Details", "grundsätzlich bekannt", "gut bekannt", "sehr gut bekannt"
        millikansKnowledgeDropdown.options.Clear();
        millikansKnowledgeDropdown.options.Add(new TMP_Dropdown.OptionData("Bitte wählen..."));
        millikansKnowledgeDropdown.options.Add(new TMP_Dropdown.OptionData("noch nie gehört"));
        millikansKnowledgeDropdown.options.Add(new TMP_Dropdown.OptionData("gehört, keine Details"));
        millikansKnowledgeDropdown.options.Add(new TMP_Dropdown.OptionData("grundsätzlich bekannt"));
        millikansKnowledgeDropdown.options.Add(new TMP_Dropdown.OptionData("gut bekannt"));
        millikansKnowledgeDropdown.options.Add(new TMP_Dropdown.OptionData("sehr gut bekannt"));
        millikansKnowledgeDropdown.value = 0;

        // Physics Interest Dropdown
        physicsInterestDropdown.options.Clear();
        physicsInterestDropdown.options.Add(new TMP_Dropdown.OptionData("Bitte wählen..."));
        physicsInterestDropdown.options.Add(new TMP_Dropdown.OptionData("gar nicht"));
        physicsInterestDropdown.options.Add(new TMP_Dropdown.OptionData("eher wenig"));
        physicsInterestDropdown.options.Add(new TMP_Dropdown.OptionData("neutral"));
        physicsInterestDropdown.options.Add(new TMP_Dropdown.OptionData("eher stark"));
        physicsInterestDropdown.options.Add(new TMP_Dropdown.OptionData("sehr stark"));
        physicsInterestDropdown.value = 0;

        // Electrodynamics Knowledge Dropdown
        electrodynamicsKnowledgeDropdown.options.Clear();
        electrodynamicsKnowledgeDropdown.options.Add(new TMP_Dropdown.OptionData("Bitte wählen..."));
        electrodynamicsKnowledgeDropdown.options.Add(new TMP_Dropdown.OptionData("sehr gering"));
        electrodynamicsKnowledgeDropdown.options.Add(new TMP_Dropdown.OptionData("gering"));
        electrodynamicsKnowledgeDropdown.options.Add(new TMP_Dropdown.OptionData("durchschnittlich"));
        electrodynamicsKnowledgeDropdown.options.Add(new TMP_Dropdown.OptionData("gut"));
        electrodynamicsKnowledgeDropdown.options.Add(new TMP_Dropdown.OptionData("sehr gut"));
        electrodynamicsKnowledgeDropdown.value = 0;

        // VR Experience Dropdown
        vrExperienceDropdown.options.Clear();
        vrExperienceDropdown.options.Add(new TMP_Dropdown.OptionData("Bitte wählen..."));
        vrExperienceDropdown.options.Add(new TMP_Dropdown.OptionData("ja"));
        vrExperienceDropdown.options.Add(new TMP_Dropdown.OptionData("nein"));
        vrExperienceDropdown.value = 0;

        // VR Frequency Dropdown
        vrFrequencyDropdown.options.Clear();
        vrFrequencyDropdown.options.Add(new TMP_Dropdown.OptionData("Bitte wählen..."));
        vrFrequencyDropdown.options.Add(new TMP_Dropdown.OptionData("nur einmal"));
        vrFrequencyDropdown.options.Add(new TMP_Dropdown.OptionData("gelegentlich"));
        vrFrequencyDropdown.options.Add(new TMP_Dropdown.OptionData("regelmäßig"));
        vrFrequencyDropdown.value = 0;
    }

    void SetupConditionalDropdowns()
    {
        // VR-Häufigkeit nur anzeigen wenn VR-Erfahrung = "ja"
        vrExperienceDropdown.onValueChanged.AddListener((value) => {
            if (vrFrequencySection != null)
                vrFrequencySection.SetActive(value == 1); // Index 1 = ja
        });
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
        // Alter prüfen (0 = "Bitte wählen...")
        if (ageDropdown.value == 0)
            return false;

        // Geschlecht prüfen (0 = "Bitte wählen...")
        if (genderDropdown.value == 0)
            return false;

        // Klassenstufe prüfen (0 = "Bitte wählen...")
        if (gradeDropdown.value == 0)
            return false;

        return true;
    }

    bool ValidatePage2()
    {
        // Alle drei Dropdowns müssen einen Wert != 0 haben
        return millikansKnowledgeDropdown.value != 0 &&
               physicsInterestDropdown.value != 0 &&
               electrodynamicsKnowledgeDropdown.value != 0;
    }

    bool ValidatePage3()
    {
        // VR Erfahrung muss beantwortet sein (nicht "Bitte wählen...")
        if (vrExperienceDropdown.value == 0) return false;

        // Wenn ja (Index 1), dann muss auch Häufigkeit beantwortet sein
        if (vrExperienceDropdown.value == 1 && vrFrequencyDropdown.value == 0)
            return false;

        return true;
    }

    void SaveCurrentPageData()
    {
        switch (currentPage)
        {
            case 0:
                demographicsData.age = ageDropdown.value - 1; // -1 wegen "Bitte wählen...", dann 0=10Jahre, 1=11Jahre, etc.
                demographicsData.gender = genderDropdown.value - 1; // -1 wegen "Bitte wählen..."
                demographicsData.grade = gradeDropdown.value - 1; // -1 wegen "Bitte wählen...", dann 0=5.Klasse, 1=6.Klasse, etc.
                break;

            case 1:
                demographicsData.millikansKnowledge = millikansKnowledgeDropdown.value - 1;
                demographicsData.physicsInterest = physicsInterestDropdown.value - 1;
                demographicsData.electrodynamicsKnowledge = electrodynamicsKnowledgeDropdown.value - 1;
                break;

            case 2:
                demographicsData.vrExperience = vrExperienceDropdown.value - 1;
                demographicsData.vrFrequency = vrFrequencyDropdown.value - 1;
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

    // Text-Mappings für Logging (angepasst für Dropdown-Indizes)
    string GetAgeText(int index)
    {
        if (index >= 0 && index <= 10) // 0=10Jahre, 1=11Jahre, ..., 10=20Jahre
            return $"{index + 10} Jahre";
        else if (index == 11) // 11=Älter
            return "Älter";
        else
            return "unknown";
    }

    string GetGenderText(int index)
    {
        string[] options = { "weiblich", "männlich", "divers", "keine Angabe" };
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

    string GetGradeText(int index)
    {
        string[] options = { "5. Klasse", "6. Klasse", "7. Klasse", "8. Klasse", "9. Klasse", "10. Klasse", "11. Klasse", "12. Klasse", "13. Klasse" };
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