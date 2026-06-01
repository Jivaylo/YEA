using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class ColorblindFilter : MonoBehaviour
{
    static ColorblindFilter instance;
    Volume volume;
    ChannelMixer channelMixer;
    ColorAdjustments colorAdj;

    void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);

        var volGO = new GameObject("ColorblindVolume");
        volGO.transform.SetParent(transform);
        volume = volGO.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 10f;
        volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
        channelMixer = volume.profile.Add<ChannelMixer>(true);
        colorAdj = volume.profile.Add<ColorAdjustments>(true);
        volume.enabled = false;

        SceneManager.sceneLoaded += OnSceneLoaded;
        EnablePostProcessingOnAllCameras();

        Apply(PlayerPrefs.GetInt("ColorblindMode", 0));
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnablePostProcessingOnAllCameras();
    }

    static void EnablePostProcessingOnAllCameras()
    {
        foreach (var cam in FindObjectsByType<Camera>(FindObjectsSortMode.None))
        {
            var data = cam.GetComponent<UniversalAdditionalCameraData>();
            if (data != null)
                data.renderPostProcessing = true;
        }
    }

    public static void Apply(int mode)
    {
        if (instance != null)
            instance.ApplyMode(Mathf.Clamp(mode, 0, 3));
    }

    void ApplyMode(int mode)
    {
        if (mode == 0) { volume.enabled = false; return; }
        volume.enabled = true;
        ResetMixer();

        switch (mode)
        {
            case 1: // Deuteranopia — missing green cones, confuses red & green
                // Shift green output toward cyan: green things become teal, reds stay red
                channelMixer.greenOutGreenIn.Override(50f);
                channelMixer.greenOutBlueIn.Override(50f);
                colorAdj.contrast.Override(20f);
                break;

            case 2: // Protanopia — missing red cones, reds appear dark/absent
                // Pull red output toward yellow so red things stay bright and visible
                channelMixer.redOutRedIn.Override(20f);
                channelMixer.redOutGreenIn.Override(80f);
                colorAdj.contrast.Override(20f);
                break;

            case 3: // Tritanopia — missing blue cones, confuses blue & yellow
                // Shift blue output toward magenta so blue things contrast against greens
                channelMixer.blueOutRedIn.Override(60f);
                channelMixer.blueOutBlueIn.Override(40f);
                colorAdj.contrast.Override(15f);
                break;
        }
    }

    void ResetMixer()
    {
        channelMixer.redOutRedIn.Override(100f);
        channelMixer.redOutGreenIn.Override(0f);
        channelMixer.redOutBlueIn.Override(0f);
        channelMixer.greenOutRedIn.Override(0f);
        channelMixer.greenOutGreenIn.Override(100f);
        channelMixer.greenOutBlueIn.Override(0f);
        channelMixer.blueOutRedIn.Override(0f);
        channelMixer.blueOutGreenIn.Override(0f);
        channelMixer.blueOutBlueIn.Override(100f);
        colorAdj.contrast.Override(0f);
        colorAdj.saturation.Override(0f);
    }
}
