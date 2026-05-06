using UnityEngine;

[System.Serializable]
public class MemoryRound
{
    public MemoryItem image;
    public MemoryItem sound;
}

[System.Serializable]
public class MemoryQuestion
{
    public enum QuestionType
    {
        Image,
        Sound
    }

    public QuestionType questionType;
    public int roundsBack;
}

[System.Serializable]
public class QuestionConfig
{
    public MemoryQuestion.QuestionType questionType;
    public int roundsBack;

    [Range(1, 100)]
    public int weight = 1;
}