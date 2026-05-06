using UnityEngine;
using TMPro;

public class StudyRoom : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private TextMeshPro roundCountDisplay;
    [SerializeField] private TextMeshPro imageNameDisplay;
    [SerializeField] private TextMeshPro soundRevealDisplay;

    [Header("Interaction")]
    [SerializeField] private Interactable soundButton;

    [Header("Spawn")]
    [SerializeField] public Transform playerSpawn;

    private AudioSource audioSource;
    private bool exitTriggered;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void Setup(MemoryRound round, int globalRound, int totalRounds)
    {
        exitTriggered = false;

        if (roundCountDisplay)  roundCountDisplay.text  = $"Round {globalRound} / {totalRounds}";
        if (imageNameDisplay)   imageNameDisplay.text   = round.image != null ? round.image.itemName : "???";
        if (soundRevealDisplay) soundRevealDisplay.text = "[ press E on the button to hear the sound ]";

        if (soundButton != null)
        {
            soundButton.prompt = "Press E — Play Sound";
            soundButton.onInteract.RemoveAllListeners();
            soundButton.onInteract.AddListener(() =>
            {
                string sName = round.sound != null ? round.sound.itemName : "???";
                if (soundRevealDisplay) soundRevealDisplay.text = $"Sound: {sName}";
                Debug.Log($"[Sound] Playing: {sName}");
                if (round.sound?.sound != null) audioSource.PlayOneShot(round.sound.sound);
            });
        }
    }

    public void OnPlayerExited() => exitTriggered = true;

    public bool IsExitTriggered => exitTriggered;
}
