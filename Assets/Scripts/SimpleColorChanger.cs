using UnityEngine;

public class SimpleColorChanger : MonoBehaviour
{
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color color;

    void Start()
    {
        var renderer = GetComponent<Renderer>();
        renderer.material.color = color;
    }
}
