using UnityEngine;

[CreateAssetMenu(menuName = "Memory Game/Memory Item")]
public class MemoryItem : ScriptableObject
{
    public string itemName;
    public Sprite image;
    public AudioClip sound;
}