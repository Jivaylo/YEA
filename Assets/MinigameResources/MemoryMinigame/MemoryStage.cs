using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Memory Game/Stage")]
public class MemoryStage : ScriptableObject
{
    public int numberOfRounds;
    public List<QuestionConfig> questionPool;
}