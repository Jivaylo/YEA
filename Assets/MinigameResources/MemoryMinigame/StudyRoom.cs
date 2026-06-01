using UnityEngine;
using TMPro;

public class StudyRoom : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private TextMeshPro roundCountDisplay;
    [SerializeField] private TextMeshPro imageNameDisplay;
    [SerializeField] private SpriteRenderer imageSpriteDisplay;
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

        if (roundCountDisplay) roundCountDisplay.text = $"Round {globalRound} / {totalRounds}";

        // Show sprite if available, otherwise show the item name
        bool hasSprite = round.image?.image != null;
        if (imageSpriteDisplay)
        {
            imageSpriteDisplay.gameObject.SetActive(hasSprite);
            if (hasSprite) imageSpriteDisplay.sprite = round.image.image;
        }
        if (imageNameDisplay)
        {
            imageNameDisplay.gameObject.SetActive(!hasSprite);
            if (!hasSprite) imageNameDisplay.text = round.image != null ? round.image.itemName : "???";
        }

        bool hasClip = round.sound?.sound != null;
        if (soundRevealDisplay) soundRevealDisplay.text = "";
        if (soundButton) soundButton.gameObject.SetActive(hasClip);

        if (soundButton != null)
        {
            soundButton.prompt = "Press E — Play Sound";
            soundButton.onInteract.RemoveAllListeners();
            soundButton.onInteract.AddListener(() =>
            {
                if (round.sound?.sound != null) audioSource.PlayOneShot(round.sound.sound);
            });
        }
    }

    public void OnPlayerExited() => exitTriggered = true;

    public bool IsExitTriggered => exitTriggered;
}
