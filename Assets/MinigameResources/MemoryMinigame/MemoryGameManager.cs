using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class GeneratedStageData
{
    public MemoryStage stage;
    public List<MemoryRound> rounds;
    public List<QuestionConfig> questions;
}

public class MemoryGameManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private List<MemoryStage> stages = new List<MemoryStage>();
    [SerializeField] private List<MemoryItem> availableItems = new List<MemoryItem>();

    [Header("Runtime Data")]
    [SerializeField] private List<GeneratedStageData> generatedStages = new List<GeneratedStageData>();

    // --- runtime UI (built in code, swap for real UI later) ---
    private GameObject studyPanel, questionPanel, resultPanel;
    private Text roundCountText, imageNameText, soundRevealText, questionBodyText, resultBodyText;
    private Button playSoundButton, nextRoundButton, nextStageButton, restartButton;
    private Button[] choiceButtons = new Button[3];
    private Text[]   choiceLabels  = new Text[3];
    private Text resultHeaderText, questionRoundText;

    private AudioSource audioSource;
    private Font uiFont;
    private int currentStageIndex;
    private int score, totalQuestions;
    private int globalRoundOffset, globalTotalRounds;


    // =========================
    // LIFECYCLE
    // =========================
    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (uiFont == null) uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        BuildUI();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        GenerateAllStages();
        StartStage(0);
    }


    // =========================
    // STAGE FLOW
    // =========================
    public void StartStage(int index)
    {
        if (index < 0 || index >= generatedStages.Count) return;
        currentStageIndex = index;

        if (index == 0) { score = 0; totalQuestions = 0; }

        globalTotalRounds  = generatedStages.Sum(s => s.stage.numberOfRounds);
        globalRoundOffset  = generatedStages.Take(index).Sum(s => s.stage.numberOfRounds);

        GeneratedStageData data = generatedStages[index];
        data.rounds    = GenerateRounds(data.stage.numberOfRounds);
        data.questions = GenerateQuestions(data.stage);
        ApplyMatchingConstraints(data.rounds, data.questions);

        StartCoroutine(RunStage(data));
    }

    IEnumerator RunStage(GeneratedStageData data)
    {
        HideAll();

        for (int i = 0; i < data.rounds.Count; i++)
        {
            yield return StartCoroutine(ShowStudyRound(data.rounds[i], globalRoundOffset + i, globalTotalRounds));

            bool correct = false;
            yield return StartCoroutine(ShowQuestion(data.rounds, i, data.questions[i], result => correct = result));

            if (!correct)
            {
                yield return StartCoroutine(ShowWrong());
                yield break;
            }
        }

        // Auto-advance to next stage, or show final result
        int next = currentStageIndex + 1;
        if (next < generatedStages.Count)
            StartStage(next);
        else
            ShowResult();
    }

    IEnumerator ShowWrong()
    {
        HideAll();
        resultPanel.SetActive(true);
        resultHeaderText.text = "WRONG!";
        resultBodyText.text   = "Starting over...";
        nextStageButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);

        yield return new WaitForSeconds(2f);

        GenerateAllStages();
        StartStage(0);
    }


    // =========================
    // STUDY PHASE
    // =========================
    IEnumerator ShowStudyRound(MemoryRound round, int index, int total)
    {
        HideAll();
        studyPanel.SetActive(true);

        roundCountText.text  = $"Round {index + 1} / {total}";
        imageNameText.text   = round.image != null ? round.image.itemName : "???";
        soundRevealText.text = "[ click Play Sound ]";

        bool advanced = false;

        playSoundButton.onClick.RemoveAllListeners();
        playSoundButton.onClick.AddListener(() =>
        {
            string sName = round.sound != null ? round.sound.itemName : "???";
            soundRevealText.text = $"Sound: {sName}";

            if (round.sound?.sound != null)
                audioSource.PlayOneShot(round.sound.sound);
        });

        nextRoundButton.onClick.RemoveAllListeners();
        nextRoundButton.onClick.AddListener(() => advanced = true);

        yield return new WaitUntil(() => advanced);
    }


    // =========================
    // QUESTION PHASE
    // =========================
    IEnumerator ShowQuestion(List<MemoryRound> rounds, int currentIndex, QuestionConfig config, System.Action<bool> onResult)
    {
        HideAll();
        questionPanel.SetActive(true);

        questionRoundText.text = $"Round {globalRoundOffset + currentIndex + 1} / {globalTotalRounds}";

        // If we can't look back far enough, ask about the current round instead
        int lookback = Mathf.Min(config.roundsBack, currentIndex);
        MemoryRound target = rounds[currentIndex - lookback];
        int targetGlobalRound = globalRoundOffset + (currentIndex - lookback) + 1;
        string roundTag = lookback == 0 ? "" : $" (round {targetGlobalRound})";
        string back = lookback == 0 ? "you just saw"
                    : lookback == 1 ? $"1 round ago{roundTag}"
                    : $"{lookback} rounds ago{roundTag}";

        int? playerChoice = null;
        bool correct = false;

        if (config.questionType == MemoryQuestion.QuestionType.Matching)
        {
            questionBodyText.text = lookback == 0
                ? "Are the image and sound in this round the SAME?"
                : $"In the round from {back},\nwere the image and sound the SAME?";

            string[] labels = { "Yes", "No" };
            bool isMatch = target.IsMatching();

            for (int i = 0; i < choiceButtons.Length; i++)
            {
                bool visible = i < 2;
                choiceButtons[i].gameObject.SetActive(visible);
                if (!visible) continue;
                choiceLabels[i].text = labels[i];
                int captured = i;
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => playerChoice = captured);
            }

            yield return new WaitUntil(() => playerChoice.HasValue);

            totalQuestions++;
            bool answeredYes = playerChoice.Value == 0;
            correct = answeredYes == isMatch;
        }
        else
        {
            MemoryItem correctItem = config.questionType == MemoryQuestion.QuestionType.Image
                ? target.image : target.sound;
            string typeWord = config.questionType == MemoryQuestion.QuestionType.Image ? "image" : "sound";

            questionBodyText.text = lookback == 0
                ? $"What was the {typeWord} in this round?"
                : $"What was the {typeWord} from {back}?";

            List<MemoryItem> choices = BuildChoices(correctItem);
            int correctIndex = choices.IndexOf(correctItem);

            for (int i = 0; i < choiceButtons.Length; i++)
            {
                choiceButtons[i].gameObject.SetActive(true);
                choiceLabels[i].text = $"{(char)('A' + i)})  {choices[i].itemName}";
                int captured = i;
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => playerChoice = captured);
            }

            yield return new WaitUntil(() => playerChoice.HasValue);

            totalQuestions++;
            correct = playerChoice.Value == correctIndex;
        }

        if (correct) score++;
        onResult(correct);
    }

    List<MemoryItem> BuildChoices(MemoryItem correct)
    {
        List<MemoryItem> wrong = availableItems.Where(i => i != correct).ToList();

        // Shuffle wrong pool
        for (int i = wrong.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (wrong[i], wrong[j]) = (wrong[j], wrong[i]);
        }

        List<MemoryItem> choices = new List<MemoryItem> { correct };
        for (int i = 0; i < Mathf.Min(2, wrong.Count); i++)
            choices.Add(wrong[i]);

        // Shuffle choices so correct isn't always first
        for (int i = choices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (choices[i], choices[j]) = (choices[j], choices[i]);
        }

        return choices;
    }


    // =========================
    // RESULT SCREEN
    // =========================
    void ShowResult()
    {
        HideAll();
        resultPanel.SetActive(true);
        resultHeaderText.text = "You made it!";
        resultBodyText.text   = $"{score} / {totalQuestions} correct";
        nextStageButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(true);
    }

    void HideAll()
    {
        studyPanel.SetActive(false);
        questionPanel.SetActive(false);
        resultPanel.SetActive(false);
    }


    // =========================
    // ROUND / QUESTION GENERATION
    // =========================
    List<MemoryRound> GenerateRounds(int count)
    {
        var result = new List<MemoryRound>();
        for (int i = 0; i < count; i++)
        {
            var round = new MemoryRound();
            if (availableItems.Count > 0)
            {
                round.image = availableItems[Random.Range(0, availableItems.Count)];
                round.sound = availableItems[Random.Range(0, availableItems.Count)];
            }
            result.Add(round);
        }
        return result;
    }

    List<QuestionConfig> GenerateQuestions(MemoryStage stage)
    {
        var result = new List<QuestionConfig>();
        for (int i = 0; i < stage.numberOfRounds; i++)
        {
            var pool = stage.questionPool.Where(q => q.roundsBack <= i).ToList();

            if (result.Count > 0)
            {
                QuestionConfig last = result[result.Count - 1];
                var filtered = pool.Where(q => !(q.questionType == last.questionType && q.roundsBack == last.roundsBack + 1)).ToList();
                if (filtered.Count > 0) pool = filtered;
            }

            if (pool.Count == 0) pool = stage.questionPool;

            result.Add(PickWeighted(pool));
        }
        return result;
    }

    QuestionConfig PickWeighted(List<QuestionConfig> pool)
    {
        int total = pool.Sum(q => q.weight);
        int roll  = Random.Range(0, total);
        int curr  = 0;
        foreach (var q in pool) { curr += q.weight; if (roll < curr) return q; }
        return pool[0];
    }

    void EnsureMatchAround(List<MemoryRound> rounds, int targetIndex)
    {
        int choice = Random.Range(0, 4);
        int idx = choice < 2 ? targetIndex : choice == 2 ? targetIndex - 1 : targetIndex + 1;
        if (idx < 0 || idx >= rounds.Count) idx = targetIndex;
        rounds[idx].sound = rounds[idx].image;
    }

    void ApplyMatchingConstraints(List<MemoryRound> rounds, List<QuestionConfig> questions)
    {
        for (int i = 0; i < questions.Count; i++)
        {
            if (questions[i].questionType == MemoryQuestion.QuestionType.Matching)
            {
                int t = rounds.Count - 1 - questions[i].roundsBack;
                if (t >= 0 && t < rounds.Count) EnsureMatchAround(rounds, t);
            }
        }
    }

    public void GenerateAllStages()
    {
        generatedStages.Clear();
        foreach (var stage in stages)
        {
            var data = new GeneratedStageData { stage = stage };
            data.rounds    = GenerateRounds(stage.numberOfRounds);
            data.questions = GenerateQuestions(stage);
            ApplyMatchingConstraints(data.rounds, data.questions);
            generatedStages.Add(data);
        }
        Debug.Log($"Generated {generatedStages.Count} stages.");
    }

    [ContextMenu("Generate All Stages (Debug)")]
    void DebugGenerate() => GenerateAllStages();


    // =========================
    // UI BUILDER
    // Replace this whole method later with a real Canvas from the editor.
    // Just assign the panel GameObjects + button/text references in the inspector
    // and delete the BuildUI() call from Awake().
    // =========================
    void BuildUI()
    {
        GameObject canvasGO = new GameObject("MemoryGameCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
        }

        studyPanel    = BuildStudyPanel(canvasGO.transform);
        questionPanel = BuildQuestionPanel(canvasGO.transform);
        resultPanel   = BuildResultPanel(canvasGO.transform);
    }

    GameObject BuildStudyPanel(Transform parent)
    {
        GameObject panel = MakePanel("StudyPanel", parent, new Color(0.08f, 0.08f, 0.15f, 0.97f));

        roundCountText = MakeText("RoundCount", panel.transform, "Round 1 / 4", 28, Color.white,
            new Vector2(0, 420), new Vector2(600, 50), TextAnchor.MiddleCenter);

        // Image card
        GameObject card = MakeBox("ImageCard", panel.transform, new Color(0.15f, 0.15f, 0.25f, 1f),
            new Vector2(0, 80), new Vector2(380, 280));
        imageNameText = MakeText("ImageName", card.transform, "???", 48, new Color(0.9f, 0.9f, 1f),
            Vector2.zero, new Vector2(360, 260), TextAnchor.MiddleCenter);

        Text imageLabel = MakeText("ImageLabel", panel.transform, "IMAGE", 22, new Color(0.6f, 0.6f, 0.8f),
            new Vector2(0, 240), new Vector2(300, 36), TextAnchor.MiddleCenter);

        soundRevealText = MakeText("SoundReveal", panel.transform, "[ click Play Sound ]", 26,
            new Color(0.7f, 1f, 0.7f), new Vector2(0, -120), new Vector2(600, 44), TextAnchor.MiddleCenter);

        playSoundButton = MakeButton("PlaySoundBtn", panel.transform, "Play Sound",
            new Vector2(-120, -210), new Vector2(220, 60));

        nextRoundButton = MakeButton("NextBtn", panel.transform, "Next  -->",
            new Vector2(160, -210), new Vector2(180, 60));

        return panel;
    }

    GameObject BuildQuestionPanel(Transform parent)
    {
        GameObject panel = MakePanel("QuestionPanel", parent, new Color(0.08f, 0.12f, 0.08f, 0.97f));

        questionRoundText = MakeText("QRoundCount", panel.transform, "", 28, Color.white,
            new Vector2(0, 390), new Vector2(600, 50), TextAnchor.MiddleCenter);

        MakeText("QHeader", panel.transform, "QUESTION", 32, new Color(0.6f, 1f, 0.6f),
            new Vector2(0, 320), new Vector2(700, 50), TextAnchor.MiddleCenter);

        questionBodyText = MakeText("QBody", panel.transform, "", 34, Color.white,
            new Vector2(0, 160), new Vector2(800, 180), TextAnchor.MiddleCenter);

        float[] yPositions = { -20f, -100f, -180f };
        for (int i = 0; i < 3; i++)
        {
            Button btn = MakeButton($"ChoiceBtn{i}", panel.transform, "",
                new Vector2(0, yPositions[i]), new Vector2(500, 70));
            choiceButtons[i] = btn;
            choiceLabels[i]  = btn.GetComponentInChildren<Text>();
        }

        return panel;
    }

    GameObject BuildResultPanel(Transform parent)
    {
        GameObject panel = MakePanel("ResultPanel", parent, new Color(0.08f, 0.08f, 0.08f, 0.97f));

        resultHeaderText = MakeText("ResultHeader", panel.transform, "", 44, new Color(1f, 0.85f, 0.2f),
            new Vector2(0, 200), new Vector2(700, 60), TextAnchor.MiddleCenter);

        resultBodyText = MakeText("ResultBody", panel.transform, "", 36, Color.white,
            new Vector2(0, 40), new Vector2(700, 160), TextAnchor.MiddleCenter);

        nextStageButton = MakeButton("NextStageBtn", panel.transform, "Next Stage  -->",
            new Vector2(-140, -160), new Vector2(260, 70));

        restartButton = MakeButton("RestartBtn", panel.transform, "Restart",
            new Vector2(160, -160), new Vector2(200, 70));

        nextStageButton.onClick.AddListener(() => StartStage(currentStageIndex + 1));
        restartButton.onClick.AddListener(() => StartStage(currentStageIndex));

        return panel;
    }


    // =========================
    // UI HELPERS
    // =========================
    GameObject MakePanel(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = color;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return go;
    }

    GameObject MakeBox(string name, Transform parent, Color color, Vector2 anchoredPos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = color;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        return go;
    }

    Text MakeText(string name, Transform parent, string content, int size, Color color,
        Vector2 anchoredPos, Vector2 sizeDelta, TextAnchor anchor)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Text t = go.AddComponent<Text>();
        t.font = uiFont;
        t.text = content;
        t.fontSize = size;
        t.color = color;
        t.alignment = anchor;
        t.resizeTextForBestFit = false;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        return t;
    }

    Button MakeButton(string name, Transform parent, string label, Vector2 anchoredPos, Vector2 size,
        Color? bgColor = null)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = bgColor ?? new Color(0.2f, 0.4f, 0.8f, 1f);
        Button btn = go.AddComponent<Button>();

        ColorBlock cb = btn.colors;
        cb.highlightedColor = Color.white * 1.2f;
        cb.pressedColor     = Color.white * 0.8f;
        btn.colors = cb;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        MakeText(name + "Label", go.transform, label, 26, Color.white,
            Vector2.zero, size, TextAnchor.MiddleCenter);

        return btn;
    }
}
