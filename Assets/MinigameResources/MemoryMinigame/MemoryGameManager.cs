using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class GeneratedStageData
{
    public MemoryStage stage;
    public List<MemoryRound> rounds;
    public List<QuestionConfig> questions;
}

public class MemoryGameManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private List<MemoryStage> stages = new List<MemoryStage>();
    [SerializeField] private List<MemoryItem> availableItems = new List<MemoryItem>();

    [Header("Runtime Data")]
    [SerializeField] private List<GeneratedStageData> generatedStages = new List<GeneratedStageData>();


    // =========================
    // UNITY START
    // =========================
    void Start()
    {
        GenerateAllStages();
    }


    // =========================
    // ROUND GENERATION
    // =========================
    List<MemoryRound> GenerateRounds(int count)
    {
        List<MemoryRound> result = new List<MemoryRound>();

        for (int i = 0; i < count; i++)
        {
            MemoryRound round = new MemoryRound();

            if (availableItems.Count > 0)
            {
                round.image = availableItems[Random.Range(0, availableItems.Count)];
                round.sound = availableItems[Random.Range(0, availableItems.Count)];
            }

            result.Add(round);
        }

        return result;
    }


    // =========================
    // QUESTION GENERATION
    // =========================
    List<QuestionConfig> GenerateQuestions(MemoryStage stage)
    {
        List<QuestionConfig> result = new List<QuestionConfig>();

        for (int i = 0; i < stage.numberOfRounds; i++)
        {
            int maxValidBack = i;

            List<QuestionConfig> validPool = stage.questionPool
                .Where(q => q.roundsBack <= maxValidBack)
                .ToList();

            // Prevent pattern: same type + roundsBack + 1
            if (result.Count > 0)
            {
                QuestionConfig last = result[result.Count - 1];

                validPool = validPool
                    .Where(q => !(q.questionType == last.questionType &&
                                  q.roundsBack == last.roundsBack + 1))
                    .ToList();
            }

            // Fallback 1: relax pattern rule
            if (validPool.Count == 0)
            {
                validPool = stage.questionPool
                    .Where(q => q.roundsBack <= maxValidBack)
                    .ToList();
            }

            // Fallback 2: absolute safety (NO skipping allowed)
            if (validPool.Count == 0)
            {
                validPool = stage.questionPool;
            }

            // FINAL GUARANTEE: always pick something
            QuestionConfig chosen = PickWeighted(validPool);
            result.Add(chosen);
        }

        return result;
    }

    QuestionConfig PickWeighted(List<QuestionConfig> pool)
    {
        int totalWeight = 0;

        for (int i = 0; i < pool.Count; i++)
            totalWeight += pool[i].weight;

        int roll = Random.Range(0, totalWeight);

        int current = 0;

        for (int i = 0; i < pool.Count; i++)
        {
            current += pool[i].weight;

            if (roll < current)
                return pool[i];
        }

        return pool[0];
    }

    // =========================
    // MATCHING LOGIC
    // =========================
    void EnsureMatchAround(List<MemoryRound> rounds, int targetIndex)
    {
        int choice = Random.Range(0, 4);

        int matchIndex;

        if (choice < 2)
            matchIndex = targetIndex;
        else if (choice == 2)
            matchIndex = targetIndex - 1;
        else
            matchIndex = targetIndex + 1;

        if (matchIndex < 0 || matchIndex >= rounds.Count)
            matchIndex = targetIndex;

        rounds[matchIndex].sound = rounds[matchIndex].image;
    }


    void ApplyMatchingConstraints(List<MemoryRound> rounds, List<QuestionConfig> questions)
    {
        for (int i = 0; i < questions.Count; i++)
        {
            if (questions[i].questionType == MemoryQuestion.QuestionType.Matching)
            {
                int targetRound = rounds.Count - 1 - questions[i].roundsBack;

                if (targetRound >= 0 && targetRound < rounds.Count)
                {
                    EnsureMatchAround(rounds, targetRound);
                }
            }
        }
    }


    // =========================
    // PRE-GENERATE ALL STAGES
    // =========================
    public void GenerateAllStages()
    {
        generatedStages.Clear();

        foreach (MemoryStage stage in stages)
        {
            GeneratedStageData data = new GeneratedStageData();
            data.stage = stage;

            data.rounds = GenerateRounds(stage.numberOfRounds);
            data.questions = GenerateQuestions(stage);

            ApplyMatchingConstraints(data.rounds, data.questions);

            generatedStages.Add(data);
        }

        Debug.Log($"Generated {generatedStages.Count} stages.");
    }


    // =========================
    // DEBUG
    // =========================
    [ContextMenu("Generate All Stages (Debug)")]
    void DebugGenerate()
    {
        GenerateAllStages();
    }
}