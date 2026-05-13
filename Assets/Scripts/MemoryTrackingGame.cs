using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class MemoryTrackingGame : MonoBehaviour
{
    [Header("References")]
    public Transform[] dots;
    public TextMeshProUGUI scoreText;

    [Header("Materials")]
    public Material greyMaterial;
    public Material greenMaterial;

    [Header("Movement")]
    public float startSpeed = 1.5f;
    public float speedIncreaseEveryPoint = 0.03f;
    public float maxSpeed = 8f;

    [Header("Scoring")]
    public float followDistance = 1.2f;
    public float pointsPerSecond = 5f;

    [Header("Memory Timing")]
    public float showGreenTime = 5f;
    public float hiddenTrackingTime = 10f;

    private Rigidbody[] dotRigidbodies;
    private int correctDotIndex;

    private float timer;
    private bool showingGreen = true;

    private float currentSpeed;
    private float score;
    private int displayedScore;

    void Start()
    {
        currentSpeed = startSpeed;
        dotRigidbodies = new Rigidbody[dots.Length];

        for (int i = 0; i < dots.Length; i++)
        {
            dotRigidbodies[i] = dots[i].GetComponent<Rigidbody>();

            if (dotRigidbodies[i] == null)
            {
                dotRigidbodies[i] = dots[i].gameObject.AddComponent<Rigidbody>();
            }

            dotRigidbodies[i].useGravity = false;
            dotRigidbodies[i].linearDamping = 0f;
            dotRigidbodies[i].angularDamping = 0f;
            dotRigidbodies[i].constraints = RigidbodyConstraints.FreezePositionY;

            SetDotMaterial(i, greyMaterial);
            GiveRandomVelocity(i);
        }

        PickNewCorrectDot();
        UpdateScoreText();
    }

    void Update()
    {
        HandleMemoryTimer();
        CheckMouseTracking();
    }

    void FixedUpdate()
    {
        KeepDotSpeeds();
    }

    void GiveRandomVelocity(int index)
    {
        Vector3 direction = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;

        dotRigidbodies[index].linearVelocity = direction * currentSpeed;
    }

    void KeepDotSpeeds()
    {
        for (int i = 0; i < dotRigidbodies.Length; i++)
        {
            Vector3 velocity = dotRigidbodies[i].linearVelocity;
            velocity.y = 0f;

            if (velocity.magnitude < 0.1f)
            {
                GiveRandomVelocity(i);
            }
            else
            {
                dotRigidbodies[i].linearVelocity = velocity.normalized * currentSpeed;
            }
        }
    }

    void HandleMemoryTimer()
    {
        timer += Time.deltaTime;

        if (showingGreen && timer >= showGreenTime)
        {
            SetDotMaterial(correctDotIndex, greyMaterial);
            showingGreen = false;
            timer = 0f;
        }
        else if (!showingGreen && timer >= hiddenTrackingTime)
        {
            PickNewCorrectDot();
        }
    }

    void PickNewCorrectDot()
    {
        for (int i = 0; i < dots.Length; i++)
        {
            SetDotMaterial(i, greyMaterial);
        }

        correctDotIndex = Random.Range(0, dots.Length);
        SetDotMaterial(correctDotIndex, greenMaterial);

        showingGreen = true;
        timer = 0f;
    }

    void CheckMouseTracking()
    {
        Ray ray = Camera.main.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 mouseWorldPos = hit.point;

            float distance = Vector3.Distance(
                mouseWorldPos,
                dots[correctDotIndex].position
            );

            if (distance <= followDistance)
            {
                score += pointsPerSecond * Time.deltaTime;

                int newDisplayedScore = Mathf.FloorToInt(score);

                if (newDisplayedScore > displayedScore)
                {
                    int pointsGained = newDisplayedScore - displayedScore;
                    displayedScore = newDisplayedScore;

                    currentSpeed += speedIncreaseEveryPoint * pointsGained;
                    currentSpeed = Mathf.Clamp(currentSpeed, startSpeed, maxSpeed);

                    UpdateScoreText();
                }
            }
        }
    }

    void SetDotMaterial(int dotIndex, Material material)
    {
        Renderer renderer = dots[dotIndex].GetComponent<Renderer>();
        renderer.material = material;
    }

    void UpdateScoreText()
    {
        scoreText.text = "Score: " + displayedScore;
    }
}