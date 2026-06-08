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

    [Header("Final Reward")]
    public GameObject fullBrain;
    public bool overrideFullBrainTransform = false;
    public Vector3 fullBrainPosition = Vector3.zero;
    public Vector3 fullBrainRotation = Vector3.zero;
    public Vector3 fullBrainScale = Vector3.one;
    public float fullBrainRotateSpeed = 20f;

    [Header("UI")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    [Header("Return Scene")]
    public string museumSceneName = "yeajasper14-4";

    private GameObject activeBrain;

    void Start()
    {
        HideAllBrains();

        if (GameSessionState.FullBrainUnlocked())
        {
            if (fullBrain != null)
            {
                fullBrain.SetActive(true);

                fullBrain.transform.position = fullBrainPosition;
                fullBrain.transform.rotation = Quaternion.Euler(fullBrainRotation);
                fullBrain.transform.localScale = fullBrainScale;

                activeBrain = fullBrain;
            }

            SetText(
                "FULL BRAIN UNLOCKED!",
                "Congratulations! You beat Thinkthrough. Explore the museum at your leisure!"
            );

            return;
        }

        string part = PlayerPrefs.GetString("UnlockedBrainPart", "");

        switch (part)
        {
            case "Cerebellum":
                ShowPart(cerebellum);
                SetText("Cerebellum Unlocked", "Controls balance and coordination.");
                break;

            case "TemporalLobe":
                ShowPart(temporalLobe);
                SetText("Temporal Lobe Unlocked", "Important for memory and learning.");
                break;

            case "OccipitalLobe":
                ShowPart(occipitalLobe);
                SetText("Occipital Lobe Unlocked", "Processes visual information.");
                break;

            case "ParietalLobe":
                ShowPart(parietalLobe);
                SetText("Parietal Lobe Unlocked", "Helps with movement and spatial awareness.");
                break;

            default:
                SetText("Brain Part Unlocked", "");
                break;
        }
    }

    void Update()
    {
        if (activeBrain != null)
            activeBrain.transform.Rotate(0f, fullBrainRotateSpeed * Time.deltaTime, 0f, Space.World);

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            SceneManager.LoadScene(museumSceneName);
    }

    void HideAllBrains()
    {
        if (cerebellum != null) cerebellum.SetActive(false);
        if (temporalLobe != null) temporalLobe.SetActive(false);
        if (occipitalLobe != null) occipitalLobe.SetActive(false);
        if (parietalLobe != null) parietalLobe.SetActive(false);
        if (fullBrain != null) fullBrain.SetActive(false);
    }

    void ShowPart(GameObject part)
    {
        if (part != null)
            part.SetActive(true);
    }

    void SetText(string title, string description)
    {
        if (titleText != null) titleText.text = title;
        if (descriptionText != null) descriptionText.text = description;
    }
}