using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class DifficultyStage
{
    public float spawnRate;
    public float noteSpeed;
    public int scoreForNextStage;
    public bool pause = false;
    public float pauseTime;
}

public class NoteSpawner : MonoBehaviour
{
    [SerializeField] private GameObject notePrefab;

    [SerializeField] private DifficultyStage[] stages;
    [SerializeField] private int currentStage = 0;

    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private int[] stageThresholds;

    [Header("Win / Lose Conditions")]
    [SerializeField] private int winScore = 1000;
    [SerializeField] private int maxMisses = 5;
    [SerializeField] private float restartDelay = 3f;

    [SerializeField] private TMP_Text missesText;

    [Header("Events")]
    public UnityEvent onWin;

    // UI built in code
    private GameObject winPanel, losePanel;
    private TextMeshProUGUI winSubtitleText;

    private int score = 0;
    private int misses = 0;
    private bool gameOver = false;

    void Awake()
    {
        BuildUI();
    }

    void Start()
    {
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        if (winSubtitleText != null)
            winSubtitleText.text = $"Returning to Museum in {restartDelay:0.#} seconds...";
        UpdateMissesUI();
    }

    public void StartGame()
    {
        StartCoroutine(SpawnLoop());
    }

    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            NextStage();
        }
    }

    IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(1f);

        while (!gameOver)
        {
            var stage = stages[currentStage];

            if (!stage.pause)
            {
                SpawnNote();
                yield return new WaitForSeconds(stage.spawnRate);
                if (score >= GetCumulativeThreshold(currentStage))
                {
                    NextStage();
                }
            }
            else
            {
                yield return new WaitForSeconds(stage.pauseTime);
                NextStage();
            }
        }
    }

    private void SpawnNote()
    {
        GameObject noteObj = Instantiate(notePrefab, transform.position, Quaternion.identity);
        Note note = noteObj.GetComponent<Note>();

        note.direction = (Direction)UnityEngine.Random.Range(0, 4);
        note.noteMod = (NoteMod)UnityEngine.Random.Range(0, Enum.GetNames(typeof(NoteMod)).Length);

        note.speed = stages[currentStage].noteSpeed;
    }

    private int GetCumulativeThreshold(int stageIndex)
    {
        int total = 0;
        for (int i = 0; i <= stageIndex; i++)
            total += stages[i].scoreForNextStage;
        return total;
    }

    public void NextStage()
    {
        currentStage = Mathf.Min(currentStage + 1, stages.Length - 1);
    }

    public void AddScore(int amount)
    {
        if (gameOver) return;
        score += amount;
        UpdateScoreUI();

        if (score >= winScore)
            Win();
    }

    public void AddMiss()
    {
        if (gameOver) return;
        misses++;
        UpdateMissesUI();

        if (misses >= maxMisses)
            Lose();
    }

    private void Win()
    {
        gameOver = true;
        PlayerPrefs.SetInt("DDRCompleted", 1);
        PlayerPrefs.SetInt("DDRJustWon", 1);   // one-shot flag for museum cutscene
        PlayerPrefs.Save();
        winPanel.SetActive(true);
        onWin.Invoke();
        StartCoroutine(GoToMuseumAfterDelay());
    }

    private void Lose()
    {
        gameOver = true;
        losePanel.SetActive(true);
        StartCoroutine(RestartAfterDelay());
    }

    IEnumerator GoToMuseumAfterDelay()
    {
        yield return new WaitForSeconds(restartDelay);
        var pm = FindAnyObjectByType<PauseMenuManager>();
        if (pm != null)
            pm.GoToMuseum();
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(restartDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    private void UpdateMissesUI()
    {
        if (missesText == null) return;
        int remaining = maxMisses - misses;
        missesText.text = remaining > 0 ? new string('♥', remaining) : "";
    }

    public bool IsGameOver => gameOver;

    // =========================
    // UI BUILDER
    // =========================
    void BuildUI()
    {
        GameObject canvasGO = new GameObject("DDRGameCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
        }

        winPanel  = BuildResultPanel(canvasGO.transform, "WinPanel",  "YOU WIN!", new Color(0.05f, 0.25f, 0.05f, 0.95f), new Color(0.6f, 1f, 0.6f), out winSubtitleText);
        losePanel = BuildResultPanel(canvasGO.transform, "LosePanel", "GAME OVER", new Color(0.25f, 0.05f, 0.05f, 0.95f), new Color(1f, 0.5f, 0.5f), out _);
    }

    GameObject BuildResultPanel(Transform parent, string name, string title, Color bgColor, Color titleColor, out TextMeshProUGUI subtitleOut)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        Image bg = panel.AddComponent<Image>();
        bg.color = bgColor;
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        // Title
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(panel.transform, false);
        TextMeshProUGUI titleText = titleGO.AddComponent<TextMeshProUGUI>();
        titleText.text = title;
        titleText.fontSize = 120;
        titleText.color = titleColor;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        RectTransform trt = titleGO.GetComponent<RectTransform>();
        trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.5f);
        trt.anchoredPosition = new Vector2(0, 60);
        trt.sizeDelta = new Vector2(1200, 200);

        // Subtitle
        GameObject subGO = new GameObject("Subtitle");
        subGO.transform.SetParent(panel.transform, false);
        TextMeshProUGUI subText = subGO.AddComponent<TextMeshProUGUI>();
        subText.text = $"Restarting in {restartDelay:0.#} seconds...";
        subText.fontSize = 36;
        subText.color = Color.white;
        subText.alignment = TextAlignmentOptions.Center;
        RectTransform srt = subGO.GetComponent<RectTransform>();
        srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 0.5f);
        srt.anchoredPosition = new Vector2(0, -80);
        srt.sizeDelta = new Vector2(900, 60);

        subtitleOut = subText;
        return panel;
    }
}
