using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Input Actions (ThirdPersonControls asset)")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Minigame Return")]
    [SerializeField] private bool inMinigame = false;
    [SerializeField] private bool keepCursorVisibleOnResume = false;
    [SerializeField] private string overworldSceneName = "SampleScene";
    [SerializeField] private Vector3 overworldSpawnPosition;

    private GameObject pausePanel;
    private GameObject settingsPanel;
    private bool isPaused = false;

    private Font uiFont;

    // Rebind buttons (settings panel)
    private Button rebindUpBtn, rebindDownBtn, rebindLeftBtn, rebindRightBtn;
    private Button rebindRunBtn, rebindInteractBtn, rebindInspectBtn, rebindDropBtn;
    private Button colorblindCycleBtn;
    private int colorblindIndex = 0;
    private readonly string[] colorblindLabels = { "Off", "Deuteranopia", "Protanopia", "Tritanopia" };

    // ── Lifecycle ───────────────────────────────────────────────────────────────

    void Awake()
    {
        uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        BuildUI();
    }

    void Start()
    {
        string saved = PlayerPrefs.GetString("InputBindingOverrides", "");
        if (!string.IsNullOrEmpty(saved))
            inputActions.LoadBindingOverridesFromJson(saved);

        RefreshRebindLabels();
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (settingsPanel.activeSelf)
                ShowPause();
            else
                TogglePause();
        }
    }

    // ── Pause state ─────────────────────────────────────────────────────────────

    void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        settingsPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        if (keepCursorVisibleOnResume)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void ShowPause()
    {
        pausePanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    // ── Rebinding (mirrors MainMenuManager) ─────────────────────────────────────

    void RebindUp()    => StartCoroutine(WaitForActionKey("Move", 1, rebindUpBtn));
    void RebindDown()  => StartCoroutine(WaitForActionKey("Move", 2, rebindDownBtn));
    void RebindLeft()  => StartCoroutine(WaitForActionKey("Move", 3, rebindLeftBtn));
    void RebindRight() => StartCoroutine(WaitForActionKey("Move", 4, rebindRightBtn));
    void RebindRun()   => StartCoroutine(WaitForActionKey("Run",  0, rebindRunBtn));
    void RebindInteract() => StartCoroutine(WaitForPrefsKey("InteractKey", rebindInteractBtn));
    void RebindInspect()  => StartCoroutine(WaitForPrefsKey("InspectKey",  rebindInspectBtn));
    void RebindDrop()     => StartCoroutine(WaitForPrefsKey("DropKey",     rebindDropBtn));

    IEnumerator WaitForActionKey(string actionName, int bindingIndex, Button btn)
    {
        SetBtnLabel(btn, "press key");
        yield return null;

        var action = inputActions?.FindAction(actionName);
        if (action == null) yield break;

        Key pressed = Key.None;
        yield return PollForKey(k => pressed = k);

        if (pressed == Key.None) { RefreshRebindLabels(); yield break; }

        var control = Keyboard.current[pressed];
        action.ApplyBindingOverride(bindingIndex, $"<Keyboard>/{control.name}");
        SaveInputOverrides();
        SetBtnLabel(btn, control.displayName);
    }

    IEnumerator WaitForPrefsKey(string prefsKey, Button btn)
    {
        SetBtnLabel(btn, "press key");
        yield return null;

        Key pressed = Key.None;
        yield return PollForKey(k => pressed = k);

        if (pressed == Key.None) { RefreshRebindLabels(); yield break; }

        PlayerPrefs.SetString(prefsKey, pressed.ToString());
        SetBtnLabel(btn, Keyboard.current[pressed].displayName);
    }

    IEnumerator PollForKey(System.Action<Key> onResult)
    {
        while (true)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame) { onResult(Key.None); yield break; }
            if (Keyboard.current.anyKey.wasPressedThisFrame)
            {
                foreach (Key k in System.Enum.GetValues(typeof(Key)))
                {
                    if (k == Key.None || k == Key.Escape) continue;
                    if (Keyboard.current[k].wasPressedThisFrame) { onResult(k); yield break; }
                }
            }
            yield return null;
        }
    }

    void SaveInputOverrides()
    {
        if (inputActions != null)
            PlayerPrefs.SetString("InputBindingOverrides", inputActions.SaveBindingOverridesAsJson());
    }

    void RefreshRebindLabels()
    {
        var move = inputActions?.FindAction("Move");
        if (move != null)
        {
            SetBtnLabel(rebindUpBtn,    BindingLabel(move, 1));
            SetBtnLabel(rebindDownBtn,  BindingLabel(move, 2));
            SetBtnLabel(rebindLeftBtn,  BindingLabel(move, 3));
            SetBtnLabel(rebindRightBtn, BindingLabel(move, 4));
        }
        var run = inputActions?.FindAction("Run");
        if (run != null) SetBtnLabel(rebindRunBtn, BindingLabel(run, 0));

        SetBtnLabel(rebindInteractBtn, PlayerPrefs.GetString("InteractKey", "E"));
        SetBtnLabel(rebindInspectBtn,  PlayerPrefs.GetString("InspectKey",  "F"));
        SetBtnLabel(rebindDropBtn,     PlayerPrefs.GetString("DropKey",     "Q"));
    }

    string BindingLabel(InputAction action, int bindingIndex)
    {
        if (bindingIndex >= action.bindings.Count) return "?";
        return InputControlPath.ToHumanReadableString(
            action.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
    }

    void CycleColorblind()
    {
        colorblindIndex = (colorblindIndex + 1) % colorblindLabels.Length;
        MainMenuManager.ColorblindMode = colorblindIndex;
        PlayerPrefs.SetInt("ColorblindMode", colorblindIndex);
        ColorblindFilter.Apply(colorblindIndex);
        SetBtnLabel(colorblindCycleBtn, colorblindLabels[colorblindIndex]);
    }

    // ── UI helpers ──────────────────────────────────────────────────────────────

    void SetBtnLabel(Button btn, string text)
    {
        if (btn == null) return;
        var tmp = btn.GetComponentInChildren<TMP_Text>();
        if (tmp != null) { tmp.text = text; return; }
        var legacy = btn.GetComponentInChildren<Text>();
        if (legacy != null) legacy.text = text;
    }

    // ── UI Builder ──────────────────────────────────────────────────────────────

    void BuildUI()
    {
        GameObject canvasGO = new GameObject("PauseMenuCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        pausePanel    = BuildPausePanel(canvasGO.transform);
        settingsPanel = BuildSettingsPanel(canvasGO.transform);

        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    GameObject BuildPausePanel(Transform canvasRoot)
    {
        GameObject backdrop = MakeFullscreenPanel("PauseBackdrop", canvasRoot, new Color(0, 0, 0, 0.6f));

        float cardHeight = inMinigame ? 660f : 480f;
        GameObject card = MakeBox("PauseCard", backdrop.transform, new Color(0.1f, 0.1f, 0.15f, 0.97f),
            Vector2.zero, new Vector2(520, cardHeight));

        float titleY  = inMinigame ? 250f  : 160f;
        float resumeY = inMinigame ? 140f  :  50f;
        float settingsY = inMinigame ? 55f : -40f;
        float quitY   = inMinigame ? -200f : -150f;

        MakeLabel("PausedTitle", card.transform, "PAUSED", 56, Color.white, new Vector2(0, titleY), new Vector2(460, 70));

        Button resumeBtn   = MakeButton("ResumeBtn",   card.transform, "Resume",       new Vector2(0, resumeY),  new Vector2(340, 70));
        Button settingsBtn = MakeButton("SettingsBtn", card.transform, "Settings",     new Vector2(0, settingsY), new Vector2(340, 70));
        Button quitBtn     = MakeButton("QuitBtn",     card.transform, "Quit to Menu", new Vector2(0, quitY),    new Vector2(340, 70),
            new Color(0.6f, 0.1f, 0.1f, 1f));

        resumeBtn.onClick.AddListener(Resume);
        settingsBtn.onClick.AddListener(() => { pausePanel.SetActive(false); settingsPanel.SetActive(true); });
        quitBtn.onClick.AddListener(() => { Time.timeScale = 1f; SceneManager.LoadScene(mainMenuSceneName); });

        if (inMinigame)
        {
            Button restartBtn = MakeButton("RestartBtn", card.transform, "Restart Minigame",
                new Vector2(0, -30f), new Vector2(340, 70), new Color(0.55f, 0.35f, 0.05f, 1f));
            restartBtn.onClick.AddListener(() => { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); });

            Button museumBtn = MakeButton("MuseumBtn", card.transform, "Back to Museum",
                new Vector2(0, -115f), new Vector2(340, 70), new Color(0.1f, 0.45f, 0.2f, 1f));
            museumBtn.onClick.AddListener(GoToMuseum);
        }

        return backdrop;
    }

    void GoToMuseum()
    {
        Time.timeScale = 1f;
        SceneManager.sceneLoaded += OnMuseumSceneLoaded;
        SceneManager.LoadScene(overworldSceneName);
    }

    void OnMuseumSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnMuseumSceneLoaded;
        var player = GameObject.FindWithTag("Player");
        if (player != null)
            player.transform.position = overworldSpawnPosition;
    }

    GameObject BuildSettingsPanel(Transform canvasRoot)
    {
        GameObject backdrop = MakeFullscreenPanel("SettingsBackdrop", canvasRoot, new Color(0, 0, 0, 0.6f));

        GameObject card = MakeBox("SettingsCard", backdrop.transform, new Color(0.1f, 0.1f, 0.15f, 0.97f),
            Vector2.zero, new Vector2(680, 740));

        MakeLabel("SettingsTitle", card.transform, "SETTINGS", 44, Color.white, new Vector2(0, 320), new Vector2(620, 60));

        // Keybind rows
        float startY = 190f;
        float rowH   = 52f;

        rebindUpBtn       = MakeRebindRow(card.transform, "Move Up",    startY - rowH * 0, () => RebindUp(),      rebindUpBtn);
        rebindDownBtn     = MakeRebindRow(card.transform, "Move Down",  startY - rowH * 1, () => RebindDown(),    rebindDownBtn);
        rebindLeftBtn     = MakeRebindRow(card.transform, "Move Left",  startY - rowH * 2, () => RebindLeft(),    rebindLeftBtn);
        rebindRightBtn    = MakeRebindRow(card.transform, "Move Right", startY - rowH * 3, () => RebindRight(),   rebindRightBtn);
        rebindRunBtn      = MakeRebindRow(card.transform, "Run",        startY - rowH * 4, () => RebindRun(),     rebindRunBtn);
        rebindInteractBtn = MakeRebindRow(card.transform, "Interact",   startY - rowH * 5, () => RebindInteract(),rebindInteractBtn);
        rebindInspectBtn  = MakeRebindRow(card.transform, "Inspect",    startY - rowH * 6, () => RebindInspect(), rebindInspectBtn);
        rebindDropBtn     = MakeRebindRow(card.transform, "Drop",       startY - rowH * 7, () => RebindDrop(),    rebindDropBtn);

        // Colorblind row
        float cbY = startY - rowH * 8 - 4f;
        MakeLabel("CBLabel", card.transform, "Colorblind", 22, new Color(0.8f, 0.8f, 0.8f),
            new Vector2(-100f, cbY), new Vector2(180, 40));
        colorblindIndex = PlayerPrefs.GetInt("ColorblindMode", 0);
        colorblindCycleBtn = MakeButton("CBCycleBtn", card.transform, colorblindLabels[colorblindIndex],
            new Vector2(110f, cbY), new Vector2(200, 40));
        colorblindCycleBtn.onClick.AddListener(CycleColorblind);

        // Back button
        Button backBtn = MakeButton("BackBtn", card.transform, "Back",
            new Vector2(0, -320), new Vector2(200, 50), new Color(0.25f, 0.25f, 0.3f, 1f));
        backBtn.onClick.AddListener(ShowPause);

        return backdrop;
    }

    // Builds a label-on-left, button-on-right row and returns the button
    Button MakeRebindRow(Transform parent, string labelText, float yPos, System.Action onClick, Button _)
    {
        MakeLabel(labelText + "Lbl", parent, labelText, 22, new Color(0.8f, 0.8f, 0.8f),
            new Vector2(-100f, yPos), new Vector2(180, 40));
        Button btn = MakeButton(labelText + "Btn", parent, "?",
            new Vector2(110f, yPos), new Vector2(160, 40));
        btn.onClick.AddListener(() => onClick());
        return btn;
    }

    // ── Primitive builders ───────────────────────────────────────────────────────

    GameObject MakeFullscreenPanel(string name, Transform parent, Color color)
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

    GameObject MakeBox(string name, Transform parent, Color color, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = color;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return go;
    }

    void MakeLabel(string name, Transform parent, string text, int size, Color color, Vector2 pos, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Text t = go.AddComponent<Text>();
        t.font = uiFont;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = sizeDelta;
    }

    Button MakeButton(string name, Transform parent, string label, Vector2 pos, Vector2 size,
        Color? bgColor = null)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = bgColor ?? new Color(0.2f, 0.4f, 0.8f, 1f);
        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = Color.white * 1.3f;
        cb.pressedColor     = Color.white * 0.75f;
        btn.colors = cb;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        MakeLabel(name + "Lbl", go.transform, label, 22, Color.white, Vector2.zero, size);
        return btn;
    }

}
