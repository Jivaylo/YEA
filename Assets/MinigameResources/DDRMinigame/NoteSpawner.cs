using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms.Impl;

[System.Serializable]
public class DifficultyStage
{
    public float spawnRate;
    public float noteSpeed;
    public int scoreForNextStage;
    public bool pause = false;
    public float pauseTime;
}

public class NoteSpawner : MonoBehaviour
{
    [SerializeField] private GameObject notePrefab;

    [SerializeField] private DifficultyStage[] stages;
    [SerializeField] private int currentStage = 0;

    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private int[] stageThresholds; // score needed for each stage

    private int score = 0;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    void Update()
    {
        // Example: press 'E' to go to the next difficulty stage
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            NextStage();
            Debug.Log("Switched to stage: " + currentStage);
        }
    }

    IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            var stage = stages[currentStage];

            if (!stage.pause)
            {
                SpawnNote();
                yield return new WaitForSeconds(stage.spawnRate);
                if (score >= stage.scoreForNextStage)
                {
                    NextStage();
                }
            }
            else
            {
                yield return new WaitForSeconds(stage.pauseTime);
                NextStage();
            }
            
        }
    }
    private void SpawnNote()
    {
        GameObject noteObj = Instantiate(notePrefab, transform.position, Quaternion.identity);
        Note note = noteObj.GetComponent<Note>();

        // randomize properties
        note.direction = (Direction)UnityEngine.Random.Range(0, 4);
        note.noteMod = (NoteMod)UnityEngine.Random.Range(0, Enum.GetNames(typeof(NoteMod)).Length);

        // use current stage speed directly
        note.speed = stages[currentStage].noteSpeed;
    }

    public void NextStage()
    {
        currentStage = Mathf.Min(currentStage + 1, stages.Length - 1);
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }
}