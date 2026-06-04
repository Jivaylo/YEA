using UnityEngine;
using UnityEngine.Events;

// Plays a cutscene in the museum scene the first time the player returns
// after winning a minigame. The minigame sets a one-shot "JustWon" flag;
// this consumes it so the cutscene plays exactly once.
public class MuseumCutsceneTrigger : MonoBehaviour
{
    public enum Minigame { DDR, Memory }

    [Tooltip("Which minigame's win triggers this cutscene.")]
    [SerializeField] Minigame minigame;

    [Tooltip("What to run when the player returns after winning. Wire your cutscene here.")]
    public UnityEvent onReturnAfterWin;

    string JustWonKey => minigame == Minigame.DDR ? "DDRJustWon" : "MemoryJustWon";

    void Start()
    {
        if (PlayerPrefs.GetInt(JustWonKey, 0) == 1)
        {
            PlayerPrefs.SetInt(JustWonKey, 0);   // consume it so it only fires once
            PlayerPrefs.Save();
            onReturnAfterWin.Invoke();
        }
    }
}
