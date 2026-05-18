using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PlayerInteractor : MonoBehaviour
{
    private Interactable nearest;
    private Text promptText;

    void Awake()
    {
        GameObject canvasGO = new GameObject("InteractPromptCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5;
        canvasGO.AddComponent<CanvasScaler>();

        GameObject textGO = new GameObject("PromptText");
        textGO.transform.SetParent(canvasGO.transform, false);
        promptText = textGO.AddComponent<Text>();
        promptText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        promptText.fontSize = 30;
        promptText.color = Color.white;
        promptText.alignment = TextAnchor.MiddleCenter;
        promptText.fontStyle = FontStyle.Bold;

        RectTransform rt = promptText.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.15f);
        rt.anchorMax = new Vector2(0.5f, 0.15f);
        rt.sizeDelta = new Vector2(500, 50);
        rt.anchoredPosition = Vector2.zero;
    }

    ButtonControl GetInteractKey()
    {
        string keyName = UnityEngine.InputSystem.Keyboard.current != null
            ? PlayerPrefs.GetString("InteractKey", "E")
            : "E";
        return System.Enum.TryParse(keyName, out UnityEngine.InputSystem.Key k)
            ? Keyboard.current[k]
            : Keyboard.current[UnityEngine.InputSystem.Key.E];
    }

    void Update()
    {
        Interactable[] all = FindObjectsByType<Interactable>(FindObjectsSortMode.None);
        nearest = null;
        float bestDist = float.MaxValue;

        foreach (Interactable item in all)
        {
            float dist = Vector3.Distance(transform.position, item.transform.position);
            if (dist < item.radius && dist < bestDist)
            {
                bestDist = dist;
                nearest = item;
            }
        }

        promptText.text = nearest != null ? nearest.prompt : "";

        if (nearest != null && GetInteractKey().wasPressedThisFrame)
            nearest.Interact();
    }
}
