using UnityEngine;
using TMPro;

public class FadeInText : MonoBehaviour
{
    public TextMeshProUGUI textUI;
    public float speed = 1f;

    void Start()
    {
        Color c = textUI.color;
        c.a = 0;
        textUI.color = c;
    }

    void Update()
    {
        Color c = textUI.color;
        c.a = Mathf.MoveTowards(c.a, 1f, speed * Time.deltaTime);
        textUI.color = c;
    }
}
