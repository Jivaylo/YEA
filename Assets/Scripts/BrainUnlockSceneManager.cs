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

                titleText.text = "Cerebellum Unlocked";
                descriptionText.text = "Controls balance and coordination.";
                break;

            case "TemporalLobe":
                if (temporalLobe != null) temporalLobe.SetActive(true);

                titleText.text = "Temporal Lobe Unlocked";
                descriptionText.text = "Important for memory and learning.";
                break;

            case "OccipitalLobe":
                if (occipitalLobe != null) occipitalLobe.SetActive(true);

                titleText.text = "Occipital Lobe Unlocked";
                descriptionText.text = "Processes visual information.";
                break;

            case "ParietalLobe":
                if (parietalLobe != null) parietalLobe.SetActive(true);

                titleText.text = "Parietal Lobe Unlocked";
                descriptionText.text = "Helps with movement and spatial awareness.";
                break;

            default:
                titleText.text = "Brain Part Unlocked";
                descriptionText.text = "";
                break;
        }
    }

    void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(museumSceneName);
        }
    }
}