using UnityEngine;
using UnityEngine.Events;

// Plays a cutscene in the museum scene the first time the player returns
// after winning a minigame. The minigame sets a one-shot "JustWon" flag;
// this consumes it so the cutscene plays exactly once.
public class MuseumCutsceneTrigger : MonoBehaviour
{
    [Tooltip("One-shot flag key set by the minigame on win. e.g. DDRJustWon or MemoryJustWon")]
    [SerializeField] string justWonKey;

    [Tooltip("What to run when the player returns after winning. Wire your cutscene here.")]
    public UnityEvent onReturnAfterWin;

    void Start()
    {
        if (PlayerPrefs.GetInt(justWonKey, 0) == 1)
        {
            PlayerPrefs.SetInt(justWonKey, 0);   // consume it so it only fires once
            PlayerPrefs.Save();
            onReturnAfterWin.Invoke();
        }
    }
}
