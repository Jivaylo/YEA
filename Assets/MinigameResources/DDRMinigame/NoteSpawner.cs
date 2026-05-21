using System;
using System.Collections;
using TMPro;
using UnityEngine;
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

    // UI built in code
    private GameObject winPanel, losePanel;
    private TMP_Text missesText;
    private Font uiFont;

    private int score = 0;
    private int misses = 0;
    private bool gameOver = false;

    void Awake()
    {
        uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        BuildUI();
    }

    void Start()
    {
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        UpdateMissesUI();
        StartCoroutine(SpawnLoop());
    }

    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            NextStage();
            Debug.Log("Switched to stage: " + currentStage);
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
                if (score >= stage.scoreForNextStage)
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
        winPanel.SetActive(true);
        Debug.Log("DDR: Win! Score = " + score);
        StartCoroutine(RestartAfterDelay());
    }

    private void Lose()
    {
        gameOver = true;
        losePanel.SetActive(true);
        Debug.Log("DDR: Lose. Misses = " + misses);
        StartCoroutine(RestartAfterDelay());
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
        if (missesText != null)
            missesText.text = "Misses: " + misses + " / " + maxMisses;
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

        // Misses counter (top-left of screen)
        GameObject missesGO = new GameObject("MissesText");
        missesGO.transform.SetParent(canvasGO.transform, false);
        missesText = missesGO.AddComponent<TextMeshProUGUI>();
        missesText.fontSize = 36;
        missesText.color = new Color(1f, 0.7f, 0.7f);
        missesText.alignment = TextAlignmentOptions.TopLeft;
        RectTransform mrt = missesGO.GetComponent<RectTransform>();
        mrt.anchorMin = new Vector2(0f, 1f);
        mrt.anchorMax = new Vector2(0f, 1f);
        mrt.pivot = new Vector2(0f, 1f);
        mrt.anchoredPosition = new Vector2(40, -40);
        mrt.sizeDelta = new Vector2(400, 60);

        winPanel  = BuildResultPanel(canvasGO.transform, "WinPanel",  "YOU WIN!", new Color(0.05f, 0.25f, 0.05f, 0.95f), new Color(0.6f, 1f, 0.6f));
        losePanel = BuildResultPanel(canvasGO.transform, "LosePanel", "GAME OVER", new Color(0.25f, 0.05f, 0.05f, 0.95f), new Color(1f, 0.5f, 0.5f));
    }

    GameObject BuildResultPanel(Transform parent, string name, string title, Color bgColor, Color titleColor)
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

        // Subtitle (restart notice)
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

        return panel;
    }
}
