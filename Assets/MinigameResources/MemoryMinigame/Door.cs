using UnityEngine;
using TMPro;

// Place on each door in QuestionRoom.
// Needs a trigger Collider on this GameObject or a child.
public class Door : MonoBehaviour
{
    [Header("Answer")]
    public int answerIndex;

    [Header("Display")]
    [SerializeField] private TextMeshPro labelDisplay;       // shown for all question types
    [SerializeField] private TextMeshPro imageNameDisplay;   // shown above door for image questions (text fallback)
    [SerializeField] private SpriteRenderer imageSpriteDisplay; // shown above door for image questions (sprite)
    [SerializeField] private GameObject pictureFrame;         // frame around the image — hidden for sound questions
    [SerializeField] private Interactable soundButton;        // shown in front for sound questions

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

        bool hasSprite = isImage && item?.image != null;

        if (imageSpriteDisplay)
        {
            imageSpriteDisplay.gameObject.SetActive(hasSprite);
            if (hasSprite) imageSpriteDisplay.sprite = item.image;
        }
        if (imageNameDisplay)
        {
            // Show text only if this is an image question AND there's no sprite to display
            imageNameDisplay.gameObject.SetActive(isImage && !hasSprite);
            if (isImage && !hasSprite) imageNameDisplay.text = item != null ? item.itemName : "";
        }

        if (pictureFrame != null)
            pictureFrame.SetActive(isImage); // hide the frame for sound questions

        if (soundButton != null)
        {
            soundButton.gameObject.SetActive(isSound);
            if (isSound && item != null)
            {
                soundButton.prompt = $"Press E — Hear sound";
                soundButton.onInteract.RemoveAllListeners();
                soundButton.onInteract.AddListener(() =>
                {
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
