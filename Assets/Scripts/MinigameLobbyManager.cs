using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MinigameLobbyManager : MonoBehaviour
{
    [Header("Completion")]
    [SerializeField] string completionKey;
    [SerializeField] GameObject completedBadge;

    [Header("Buttons")]
    [SerializeField] Button playButton;
    [SerializeField] Button backButton;
    [SerializeField] PauseMenuManager pauseMenu;

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
