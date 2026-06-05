using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class BrainUnlockSceneManager : MonoBehaviour
{
    [Header("Brain Parts")]
    public GameObject cerebellum;
    public GameObject temporalLobe;
    public GameObject occipitalLobe;
    public GameObject parietalLobe;

    [Header("UI")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    [Header("Return Scene")]
    public string museumSceneName = "yeajasper14-4";

    void Start()
    {
        if (cerebellum != null) cerebellum.SetActive(false);
        if (temporalLobe != null) temporalLobe.SetActive(false);
        if (occipitalLobe != null) occipitalLobe.SetActive(false);
        if (parietalLobe != null) parietalLobe.SetActive(false);

        string part = PlayerPrefs.GetString("UnlockedBrainPart", "");

        switch (part)
        {
            case "Cerebellum":
                if (cerebellum != null) cerebellum.SetActive(true);
                SetText("Cerebellum Unlocked", "Controls balance and coordination.");
                break;

            case "TemporalLobe":
                if (temporalLobe != null) temporalLobe.SetActive(true);
                SetText("Temporal Lobe Unlocked", "Important for memory and learning.");
                break;

            case "OccipitalLobe":
                if (occipitalLobe != null) occipitalLobe.SetActive(true);
                SetText("Occipital Lobe Unlocked", "Processes visual information.");
                break;

            case "ParietalLobe":
                if (parietalLobe != null) parietalLobe.SetActive(true);
                SetText("Parietal Lobe Unlocked", "Helps with movement and spatial awareness.");
                break;

            default:
                SetText("Brain Part Unlocked", "");
                break;
        }
    }

    void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            var pauseMenu = FindAnyObjectByType<PauseMenuManager>();

            if (pauseMenu != null)
                pauseMenu.GoToMuseum();
            else
                SceneManager.LoadScene(museumSceneName);
        }
    }

    void SetText(string title, string description)
    {
        if (titleText != null)
            titleText.text = title;

        if (descriptionText != null)
            descriptionText.text = description;
    }
}