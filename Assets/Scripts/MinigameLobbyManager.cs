using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class MinigameLobbyManager : MonoBehaviour
{
    [Header("Completion")]
    [SerializeField] string completionKey;
    [SerializeField] GameObject completedBadge;

    [Header("Buttons")]
    [SerializeField] Button playButton;
    [SerializeField] Button backButton;
    [SerializeField] PauseMenuManager pauseMenu;

    [Header("Controls Text")]
    [Tooltip("Tokens like [up] [down] [left] [right] are swapped for the player's currently-bound keys.")]
    [SerializeField] TMP_Text controlsText;
    [Tooltip("Assign ThirdPersonControls.inputactions so the tokens reflect rebinds from Settings.")]
    [SerializeField] InputActionAsset inputActions;

    [Header("On Play")]
    public UnityEvent onPlay;

    public static bool IsActive { get; private set; }
    static bool skipLobby;

    // Call when returning to the museum so the lobby shows again on next entry.
    public static void ResetLobby() => skipLobby = false;

    void Start()
    {
        bool completed = PlayerPrefs.GetInt(completionKey, 0) == 1;
        if (completedBadge != null)
            completedBadge.SetActive(completed);

        FillControlsText();

        playButton.onClick.AddListener(OnPlay);
        backButton.onClick.AddListener(OnBack);

        if (skipLobby)
        {
            OnPlay();
            return;
        }

        IsActive = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Swap [up]/[down]/[left]/[right] tokens in the controls text for the keys the
    // player currently has bound (respecting rebinds saved from Settings).
    void FillControlsText()
    {
        if (controlsText == null) return;

        // Defaults — used if the asset isn't assigned or a binding can't be resolved.
        string up = "W", down = "S", left = "A", right = "D";

        if (inputActions != null)
        {
            // Clone so we never mutate the shared asset, then apply saved rebinds.
            var asset = Instantiate(inputActions);
            string json = PlayerPrefs.GetString("InputBindingOverrides", "");
            if (!string.IsNullOrEmpty(json))
                asset.LoadBindingOverridesFromJson(json);

            var move = asset.FindActionMap("Player")?.FindAction("Move");
            if (move != null)
            {
                // Move composite binding indices: 1=up, 2=down, 3=left, 4=right
                up    = KeyLabel(move.bindings[1].effectivePath, up);
                down  = KeyLabel(move.bindings[2].effectivePath, down);
                left  = KeyLabel(move.bindings[3].effectivePath, left);
                right = KeyLabel(move.bindings[4].effectivePath, right);
            }

            Destroy(asset);
        }

        controlsText.text = controlsText.text
            .Replace("[up]", up)
            .Replace("[down]", down)
            .Replace("[left]", left)
            .Replace("[right]", right);
    }

    // Turns a binding path like "<Keyboard>/w" into a display label like "W".
    string KeyLabel(string path, string fallback)
    {
        if (string.IsNullOrEmpty(path)) return fallback;
        string human = InputControlPath.ToHumanReadableString(
            path, InputControlPath.HumanReadableStringOptions.OmitDevice);
        return string.IsNullOrEmpty(human) ? fallback : human;
    }

    void OnPlay()
    {
        IsActive = false;
        skipLobby = true;
        if (pauseMenu != null)
            pauseMenu.ApplyGameplayCursor();
        gameObject.SetActive(false);
        onPlay.Invoke();
    }

    void OnBack()
    {
        IsActive = false;
        skipLobby = false;
        if (pauseMenu != null)
            pauseMenu.GoToMuseum();
        else
            SceneManager.LoadScene("SampleScene");
    }
}
