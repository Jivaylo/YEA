using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class MinigameLobbyManager : MonoBehaviour
{
    public enum Minigame { Skeleton, Memory, Rhythm, Motion }

    [Header("Completion")]
    [Tooltip("Which minigame this lobby is for — drives the COMPLETED badge, read from GameSessionState.")]
    [SerializeField] Minigame minigame;
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
    // Which scene the skip was set for — so a skip only applies to a restart of the SAME
    // minigame, not to opening a different minigame after a win.
    static string skipLobbyScene;

    // Call when returning to the museum so the lobby shows again on next entry.
    public static void ResetLobby()
    {
        skipLobby = false;
        skipLobbyScene = null;
    }

    // Clear the skip whenever a scene OTHER than the skip's own minigame scene loads.
    // A restart-from-pause reloads the same scene (skip kept). A win goes through
    // BrainUnlockScene/museum first (a different scene), so the skip is cleared and the
    // lobby shows again next time — even if you reopen the same minigame.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RegisterSkipReset()
    {
        SceneManager.sceneLoaded -= OnAnySceneLoaded;
        SceneManager.sceneLoaded += OnAnySceneLoaded;
    }

    static void OnAnySceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (skipLobby && scene.name != skipLobbyScene)
            ResetLobby();
    }

    bool IsCompleted()
    {
        switch (minigame)
        {
            case Minigame.Skeleton: return GameSessionState.skeletonDone;
            case Minigame.Memory:   return GameSessionState.memoryDone;
            case Minigame.Rhythm:   return GameSessionState.rhythmDone;
            case Minigame.Motion:   return GameSessionState.motionDone;
            default: return false;
        }
    }

    void Start()
    {
        if (completedBadge != null)
            completedBadge.SetActive(IsCompleted());

        FillControlsText();

        playButton.onClick.AddListener(OnPlay);
        backButton.onClick.AddListener(OnBack);

        // Only skip the lobby if the skip was set for THIS scene (a restart of the same
        // minigame). Otherwise show the lobby and clear any stale skip from a prior game.
        if (skipLobby && skipLobbyScene == SceneManager.GetActiveScene().name)
        {
            OnPlay();
            return;
        }
        skipLobby = false;
        skipLobbyScene = null;

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
        skipLobbyScene = SceneManager.GetActiveScene().name;
        if (pauseMenu != null)
            pauseMenu.ApplyGameplayCursor();
        gameObject.SetActive(false);
        onPlay.Invoke();
    }

    void OnBack()
    {
        IsActive = false;
        skipLobby = false;
        skipLobbyScene = null;
        if (pauseMenu != null)
            pauseMenu.GoToMuseum();
        else
            SceneManager.LoadScene("SampleScene");
    }
}
