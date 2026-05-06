using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class QuestionRoom : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private TextMeshPro roundCountDisplay;
    [SerializeField] private TextMeshPro questionText;

    [Header("Doors — assign A, B, C in order")]
    [SerializeField] private Door[] doors = new Door[3];

    [Header("Spawn")]
    [SerializeField] public Transform playerSpawn;

    private AudioSource audioSource;
    private int chosenAnswer = -1;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void Setup(QuestionConfig config, List<MemoryItem> choices, string back, int globalRound, int totalRounds)
    {
        chosenAnswer = -1;

        if (roundCountDisplay) roundCountDisplay.text = $"Round {globalRound} / {totalRounds}";

        bool isImage = config.questionType == MemoryQuestion.QuestionType.Image;
        string typeWord = isImage ? "image" : "sound";
        if (questionText) questionText.text = $"What was the {typeWord}\n{back}?";

        for (int i = 0; i < doors.Length; i++)
        {
            if (doors[i] == null) { Debug.LogWarning($"QuestionRoom: Door {i} not assigned in inspector."); continue; }
            doors[i].gameObject.SetActive(true);
            doors[i].Configure($"{(char)('A' + i)}", choices[i], config.questionType);
        }
    }

    public void OnDoorEntered(int index) => chosenAnswer = index;

    public int ChosenAnswer => chosenAnswer;
}
