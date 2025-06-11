using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro hinzugefügt
using System.Collections;

public class SceneTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float timerDuration = 600f; // 10 Minuten in Sekunden
    [SerializeField] private float warningTime = 30f; // Letzte 30 Sekunden
    [SerializeField] private string nextSceneName = "NextScene"; // Name der nächsten Szene

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timerText; // Geändert zu TextMeshPro
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;

    // Position Settings entfernt - wird nicht mehr gebraucht
    // [Header("Position Settings")] - auskommentiert
    // [SerializeField] private Transform playerCamera; - auskommentiert  
    // [SerializeField] private float distanceFromPlayer = 2f; - auskommentiert
    // [SerializeField] private float heightOffset = -1f; - auskommentiert

    private float currentTime;
    private bool isWarning = false;
    // Canvas-Variablen entfernt - nicht mehr nötig

    void Start()
    {
        // Timer initialisieren
        currentTime = timerDuration;

        // Prüfen ob TextMeshPro zugewiesen ist
        if (timerText == null)
        {
            Debug.LogError("Timer Text (TextMeshProUGUI) ist nicht zugewiesen! Bitte im Inspector eintragen.");
            return;
        }

        // Timer starten
        StartCoroutine(CountdownTimer());

        Debug.Log("Scene Timer gestartet - 10 Minuten bis zum Szenenwechsel");
    }

    // Update-Methode nicht mehr nötig - Position wird nicht mehr verändert
    // void Update() - entfernt

    IEnumerator CountdownTimer()
    {
        while (currentTime > 0)
        {
            // Zeit reduzieren
            currentTime -= Time.deltaTime;

            // Warnung aktivieren bei letzten 30 Sekunden
            if (currentTime <= warningTime && !isWarning)
            {
                isWarning = true;
                timerText.color = warningColor;
                Debug.Log("Timer Warnung aktiviert - Letzte 30 Sekunden!");
            }

            // Timer Text aktualisieren
            UpdateTimerDisplay();

            yield return null;
        }

        // Zeit abgelaufen - Szene wechseln
        Debug.Log("Zeit abgelaufen! Wechsel zur nächsten Szene: " + nextSceneName);
        LoadNextScene();
    }

    void UpdateTimerDisplay()
    {
        // Zeit in Minuten:Sekunden formatieren
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);

        timerText.text = string.Format("Zeit: {0:00}:{1:00}", minutes, seconds);
    }

    // Alle UI-Erstellungs- und Positionierungs-Methoden entfernt
    // CreateTimerUI(), UpdateTimerPosition(), FindPlayerCamera() - nicht mehr nötig

    void LoadNextScene()
    {
        // Prüfen ob Szene existiert
        if (Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("Szene '" + nextSceneName + "' nicht gefunden! Prüfe den Szenenamen.");
            // Fallback: Erste Szene laden
            SceneManager.LoadScene(0);
        }
    }

    // Öffentliche Methoden für externe Kontrolle
    public void AddTime(float seconds)
    {
        currentTime += seconds;
        Debug.Log("Zeit hinzugefügt: " + seconds + " Sekunden");
    }

    public void ResetTimer()
    {
        currentTime = timerDuration;
        isWarning = false;
        timerText.color = normalColor;
        Debug.Log("Timer zurückgesetzt");
    }

    public float GetRemainingTime()
    {
        return currentTime;
    }
}