using UnityEngine;
using UnityEngine.UI;

public class ColorblindFilter : MonoBehaviour
{
    static ColorblindFilter instance;
    Image overlay;

    static readonly Color[] filterColors = new Color[]
    {
        new Color(0f,    0f,    0f,    0f),     // 0: Off
        new Color(0.3f,  0.1f,  0f,    0.25f),  // 1: Deuteranopia (red-green)
        new Color(0.2f,  0.1f,  0f,    0.2f),   // 2: Protanopia   (red weak)
        new Color(0f,    0.1f,  0.3f,  0.2f),   // 3: Tritanopia   (blue-yellow)
    };

    void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);

        GameObject canvasGO = new GameObject("ColorblindOverlayCanvas");
        canvasGO.transform.SetParent(transform);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GameObject overlayGO = new GameObject("Overlay");
        overlayGO.transform.SetParent(canvasGO.transform, false);
        overlay = overlayGO.AddComponent<Image>();
        overlay.raycastTarget = false;
        RectTransform rt = overlay.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        Apply(PlayerPrefs.GetInt("ColorblindMode", 0));
    }

    public static void Apply(int mode)
    {
        if (instance == null || instance.overlay == null) return;
        mode = Mathf.Clamp(mode, 0, filterColors.Length - 1);
        instance.overlay.color = filterColors[mode];
    }
}
