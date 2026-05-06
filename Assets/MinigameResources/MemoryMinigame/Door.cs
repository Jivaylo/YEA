using UnityEngine;
using TMPro;

// Place on each door in QuestionRoom.
// Needs a trigger Collider on this GameObject or a child.
public class Door : MonoBehaviour
{
    [Header("Answer")]
    public int answerIndex;

    [Header("Display")]
    [SerializeField] private TextMeshPro labelDisplay;    // shown for all question types
    [SerializeField] private TextMeshPro imageNameDisplay; // shown above door for image questions
    [SerializeField] private Interactable soundButton;     // shown in front for sound questions

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponentInParent<AudioSource>();
    }

    public void Configure(string label, MemoryItem item, MemoryQuestion.QuestionType questionType)
    {
        if (labelDisplay)    labelDisplay.text = label;

        bool isImage = questionType == MemoryQuestion.QuestionType.Image;
        bool isSound = questionType == MemoryQuestion.QuestionType.Sound;

        if (imageNameDisplay)
        {
            imageNameDisplay.gameObject.SetActive(isImage);
            if (isImage) imageNameDisplay.text = item != null ? item.itemName : "";
        }

        if (soundButton != null)
        {
            soundButton.gameObject.SetActive(isSound);
            if (isSound && item != null)
            {
                soundButton.prompt = $"Press E — Hear sound";
                soundButton.onInteract.RemoveAllListeners();
                soundButton.onInteract.AddListener(() =>
                {
                    Debug.Log($"[Sound] Playing: {item.itemName}");
                    if (item.sound != null && audioSource != null)
                        audioSource.PlayOneShot(item.sound);
                });
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            GetComponentInParent<QuestionRoom>().OnDoorEntered(answerIndex);
    }
}
