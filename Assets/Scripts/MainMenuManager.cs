using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject settingsCanvas;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "MemoryMinigame";

    [Header("Input Actions (ThirdPersonControls asset)")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("Keybind Buttons")]
    [SerializeField] private Button rebindUpBtn;
    [SerializeField] private Button rebindDownBtn;
    [SerializeField] private Button rebindLeftBtn;
    [SerializeField] private Button rebindRightBtn;
    [SerializeField] private Button rebindRunBtn;
    [SerializeField] private Button rebindInteractBtn;
    [SerializeField] private Button rebindInspectBtn;
    [SerializeField] private Button rebindDropBtn;

    [Header("Colorblind")]
    [SerializeField] private TMP_Dropdown colorblindDropdown;

    // 0=Off, 1=Deuteranopia, 2=Protanopia, 3=Tritanopia
    public static int ColorblindMode { get; set; }

    void Start()
    {
        // Force fullscreen on launch (MainMenu is scene 0, so this runs at startup).
        Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);

        ShowMainMenu();

        int cb = PlayerPrefs.GetInt("ColorblindMode", 0);
        ColorblindMode = cb;
        if (colorblindDropdown != null)
        {
            colorblindDropdown.ClearOptions();
            colorblindDropdown.AddOptions(new System.Collections.Generic.List<string>
                { "Off", "Deuteranopia", "Protanopia", "Tritanopia" });
            colorblindDropdown.SetValueWithoutNotify(cb);
            colorblindDropdown.onValueChanged.AddListener(OnColorblindChanged);
        }
        ColorblindFilter.Apply(cb);

        LoadInputOverrides();
        RefreshRebindLabels();
    }

    // ── Navigation ─────────────────────────────────────────────────────────────

    public void ShowMainMenu()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
        if (settingsCanvas != null) settingsCanvas.SetActive(false);
    }

    public void OpenSettings()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
        if (settingsCanvas != null) settingsCanvas.SetActive(true);
    }

    public void PlayGame()
    {
        GameSessionState.justEnteredFromMainMenu = true;
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // ── Colorblind ──────────────────────────────────────────────────────────────

    void OnColorblindChanged(int value)
    {
        ColorblindMode = value;
        PlayerPrefs.SetInt("ColorblindMode", value);
        ColorblindFilter.Apply(value);
    }

    // ── Keybind rebinding ──────────────────────────────────────────────────────
    // Move composite binding indices within the Move action:
    //   0 = 2D Vector composite, 1 = up, 2 = down, 3 = left, 4 = right

    public void RebindMoveUp()    => StartCoroutine(WaitForActionKey("Move", 1, rebindUpBtn));
    public void RebindMoveDown()  => StartCoroutine(WaitForActionKey("Move", 2, rebindDownBtn));
    public void RebindMoveLeft()  => StartCoroutine(WaitForActionKey("Move", 3, rebindLeftBtn));
    public void RebindMoveRight() => StartCoroutine(WaitForActionKey("Move", 4, rebindRightBtn));
    public void RebindRun()       => StartCoroutine(WaitForActionKey("Run",  0, rebindRunBtn));

    public void RebindInteract() => StartCoroutine(WaitForPrefsKey("InteractKey", rebindInteractBtn));
    public void RebindInspect()  => StartCoroutine(WaitForPrefsKey("InspectKey",  rebindInspectBtn));
    public void RebindDrop()     => StartCoroutine(WaitForPrefsKey("DropKey",     rebindDropBtn));

    // Rebinds any InputAction binding by index.
    IEnumerator WaitForActionKey(string actionName, int bindingIndex, Button btn)
    {
        SetBtnLabel(btn, "press key");
        yield return null;

        var action = inputActions?.FindAction(actionName);
        if (action == null) yield break;

        Key pressed = Key.None;
        yield return PollForKey(k => pressed = k);

        if (pressed == Key.None)
        {
            RefreshRebindLabels();
            yield break;
        }

        var control = Keyboard.current[pressed];
        string path = $"<Keyboard>/{control.name}";
        action.ApplyBindingOverride(bindingIndex, path);
        SaveInputOverrides();
        SetBtnLabel(btn, control.displayName);
    }

    // Rebinds a PlayerPrefs-stored key (Interact / Inspect / Drop).
    IEnumerator WaitForPrefsKey(string prefsKey, Button btn)
    {
        SetBtnLabel(btn, "press key");
        yield return null;

        Key pressed = Key.None;
        yield return PollForKey(k => pressed = k);

        if (pressed == Key.None)
        {
            RefreshRebindLabels();
            yield break;
        }

        PlayerPrefs.SetString(prefsKey, pressed.ToString());
        SetBtnLabel(btn, Keyboard.current[pressed].displayName);
    }

    // Polls until any key is pressed; Escape cancels (callback gets Key.None).
    IEnumerator PollForKey(System.Action<Key> onResult)
    {
        while (true)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                onResult(Key.None);
                yield break;
            }

            if (Keyboard.current.anyKey.wasPressedThisFrame)
            {
                foreach (Key k in System.Enum.GetValues(typeof(Key)))
                {
                    if (k == Key.None || k == Key.Escape) continue;
                    if (Keyboard.current[k].wasPressedThisFrame)
                    {
                        onResult(k);
                        yield break;
                    }
                }
            }

            yield return null;
        }
    }

    // ── Reset to defaults ──────────────────────────────────────────────────────

    public void ResetToDefaults()
    {
        // Clear all InputAction overrides (restores WASD)
        if (inputActions != null)
        {
            inputActions.RemoveAllBindingOverrides();
            PlayerPrefs.DeleteKey("InputBindingOverrides");
        }

        // Reset PlayerPrefs keys to defaults
        PlayerPrefs.SetString("InteractKey", "E");
        PlayerPrefs.SetString("InspectKey",  "F");
        PlayerPrefs.SetString("DropKey",     "Q");

        RefreshRebindLabels();
    }

    // ── Save / Load ────────────────────────────────────────────────────────────

    void SaveInputOverrides()
    {
        if (inputActions != null)
            PlayerPrefs.SetString("InputBindingOverrides", inputActions.SaveBindingOverridesAsJson());
    }

    void LoadInputOverrides()
    {
        if (inputActions == null) return;
        string saved = PlayerPrefs.GetString("InputBindingOverrides", "");
        if (!string.IsNullOrEmpty(saved))
            inputActions.LoadBindingOverridesFromJson(saved);
    }

    // ── UI helpers ─────────────────────────────────────────────────────────────

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
        if (run != null)
            SetBtnLabel(rebindRunBtn, BindingLabel(run, 0));

        SetBtnLabel(rebindInteractBtn, PlayerPrefs.GetString("InteractKey", "E"));
        SetBtnLabel(rebindInspectBtn,  PlayerPrefs.GetString("InspectKey",  "F"));
        SetBtnLabel(rebindDropBtn,     PlayerPrefs.GetString("DropKey",     "Q"));
    }

    // Reads effectivePath (override takes priority over original) and converts
    // it to a short human-readable label like "W" or "Space".
    string BindingLabel(InputAction action, int bindingIndex)
    {
        if (bindingIndex >= action.bindings.Count) return "?";
        string path = action.bindings[bindingIndex].effectivePath;
        return InputControlPath.ToHumanReadableString(
            path,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
    }

    void SetBtnLabel(Button btn, string text)
    {
        if (btn == null) return;
        var tmp = btn.GetComponentInChildren<TMP_Text>();
        if (tmp != null) { tmp.text = text; return; }
        var legacy = btn.GetComponentInChildren<Text>();
        if (legacy != null) legacy.text = text;
    }
}
