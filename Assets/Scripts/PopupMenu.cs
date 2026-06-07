using UnityEngine;

/// <summary>
/// Generic popup menu. Call Show() to reveal it (shows the mouse and freezes the
/// player) and Hide() to dismiss it (wire Hide to a close button on the popup).
/// Movement is blocked via Time.timeScale = 0, which every movement script in this
/// project already early-exits on, so the character can't move while the popup is up.
/// </summary>
public class PopupMenu : MonoBehaviour
{
    [Tooltip("The root object of the popup UI to show/hide. Defaults to this GameObject if left empty.")]
    [SerializeField] private GameObject popupRoot;

    [Tooltip("If true, freezes the game (Time.timeScale = 0) while the popup is open so the player can't move.")]
    [SerializeField] private bool freezeGameWhileOpen = true;

    [Tooltip("If true, this popup auto-shows when the scene starts, but ONLY when the game was just entered via the main menu's Play button (not on minigame returns).")]
    [SerializeField] private bool showOnEnterFromMainMenu = false;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        // Decide shown-vs-hidden here (NOT in Start), because if popupRoot is this same
        // GameObject, disabling it would stop Start from ever running.
        bool autoShow = showOnEnterFromMainMenu && GameSessionState.justEnteredFromMainMenu;
        if (autoShow)
        {
            GameSessionState.justEnteredFromMainMenu = false; // consume so it fires only once
            Show();
        }
        else
        {
            ResolveRoot().SetActive(false); // start hidden
        }
    }

    private GameObject ResolveRoot()
    {
        if (popupRoot == null)
            popupRoot = gameObject;
        return popupRoot;
    }

    /// <summary>Reveal the popup: shows the cursor and stops the player from moving.</summary>
    public void Show()
    {
        IsOpen = true;
        ResolveRoot().SetActive(true);

        // Make the mouse appear.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (freezeGameWhileOpen)
            Time.timeScale = 0f;
    }

    /// <summary>Hide the popup: hides the cursor again and lets the player move. Wire this to the popup's close button.</summary>
    public void Hide()
    {
        IsOpen = false;
        ResolveRoot().SetActive(false);

        if (freezeGameWhileOpen)
            Time.timeScale = 1f;

        // Hide/lock the cursor for normal gameplay.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        // While open, keep the cursor shown/unlocked so another script's Start/Update
        // (e.g. the player controller locking the cursor on scene entry) can't steal it back.
        if (IsOpen)
        {
            if (Cursor.lockState != CursorLockMode.None) Cursor.lockState = CursorLockMode.None;
            if (!Cursor.visible) Cursor.visible = true;
        }
    }

    /// <summary>Convenience toggle if you ever want to bind it to a key.</summary>
    public void Toggle()
    {
        if (IsOpen) Hide();
        else Show();
    }
}
