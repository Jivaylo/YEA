using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class MemoryTrackingGame : MonoBehaviour
{
    [Header("References")]
    public Transform[] balls;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI gameOverText;

    [Header("Materials")]
    public Material greyMaterial;
    public Material greenMaterial;

    [Header("Game Settings")]
    public float gameTime = 60f;
    public float showGreenTime = 4f;
    public float hiddenTrackingTime = 10f;

    [Header("Scoring")]
    public float followDistance = 1f;
    public float pointsPerSecond = 10f;
    public float losePointsPerSecond = 4f;

    [Header("Movement")]
    public float startSpeed = 2f;
    public float speedIncreaseEveryPoint = 0.02f;
    public float maxSpeed = 9f;

    [Header("Area Limits")]
    public float minX = -4.5f;
    public float maxX = 4.5f;
    public float minZ = -4.5f;
    public float maxZ = 4.5f;

    private Rigidbody[] ballRigidbodies;
    private int correctBallIndex;

    private float currentSpeed;
    private float score;
    private float gameTimer;
    private float memoryTimer;

    private bool showingGreen = true;
    private bool gameEnded = false;

    void Start()
    {
        currentSpeed = startSpeed;
        gameTimer = gameTime;

        ballRigidbodies = new Rigidbody[balls.Length];

        for (int i = 0; i < balls.Length; i++)
        {
            ballRigidbodies[i] = balls[i].GetComponent<Rigidbody>();

            ballRigidbodies[i].useGravity = false;
            ballRigidbodies[i].linearDamping = 0f;
            ballRigidbodies[i].angularDamping = 0f;
            ballRigidbodies[i].constraints = RigidbodyConstraints.FreezePositionY;

            SetBallMaterial(i, greyMaterial);
            GiveRandomVelocity(i);
        }

        PickNewCorrectBall();

        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);

        UpdateUI();
    }

    void Update()
    {
        if (gameEnded) return;

        HandleGameTimer();
        HandleMemoryTimer();
        CheckMouseTracking();
        UpdateUI();
    }

    void FixedUpdate()
    {
        if (gameEnded) return;

        KeepBallSpeeds();
        KeepBallsInsideArea();
    }

    void HandleGameTimer()
    {
        gameTimer -= Time.deltaTime;

        if (gameTimer <= 0f)
        {
            gameTimer = 0f;
            EndGame();
        }
    }

    void HandleMemoryTimer()
    {
        memoryTimer += Time.deltaTime;

        if (showingGreen && memoryTimer >= showGreenTime)
        {
            SetBallMaterial(correctBallIndex, greyMaterial);
            showingGreen = false;
            memoryTimer = 0f;
        }
        else if (!showingGreen && memoryTimer >= hiddenTrackingTime)
        {
            PickNewCorrectBall();
        }
    }

    void CheckMouseTracking()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            float distance = Vector3.Distance(
                hit.point,
                balls[correctBallIndex].position
            );

            if (distance <= followDistance)
            {
                score += pointsPerSecond * Time.deltaTime;
                currentSpeed += speedIncreaseEveryPoint * Time.deltaTime;
            }
            else
            {
                score -= losePointsPerSecond * Time.deltaTime;
            }
        }
        else
        {
            score -= losePointsPerSecond * Time.deltaTime;
        }

        score = Mathf.Max(0f, score);
        currentSpeed = Mathf.Clamp(currentSpeed, startSpeed, maxSpeed);
    }

    void PickNewCorrectBall()
    {
        for (int i = 0; i < balls.Length; i++)
            SetBallMaterial(i, greyMaterial);

        correctBallIndex = Random.Range(0, balls.Length);
        SetBallMaterial(correctBallIndex, greenMaterial);

        showingGreen = true;
        memoryTimer = 0f;
    }

    void GiveRandomVelocity(int index)
    {
        Vector3 direction = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;

        ballRigidbodies[index].linearVelocity = direction * currentSpeed;
    }

    void KeepBallSpeeds()
    {
        for (int i = 0; i < ballRigidbodies.Length; i++)
        {
            Vector3 velocity = ballRigidbodies[i].linearVelocity;
            velocity.y = 0f;

            if (velocity.magnitude < 0.1f)
                GiveRandomVelocity(i);
            else
                ballRigidbodies[i].linearVelocity = velocity.normalized * currentSpeed;
        }
    }

    void KeepBallsInsideArea()
    {
        Vector3 center = new Vector3(
            (minX + maxX) / 2f,
            balls[0].position.y,
            (minZ + maxZ) / 2f
        );

        float padding = 0.8f;

        for (int i = 0; i < balls.Length; i++)
        {
            Vector3 pos = balls[i].position;
            bool hitWall = false;

            if (pos.x <= minX + padding)
            {
                pos.x = minX + padding;
                hitWall = true;
            }
            else if (pos.x >= maxX - padding)
            {
                pos.x = maxX - padding;
                hitWall = true;
            }

            if (pos.z <= minZ + padding)
            {
                pos.z = minZ + padding;
                hitWall = true;
            }
            else if (pos.z >= maxZ - padding)
            {
                pos.z = maxZ - padding;
                hitWall = true;
            }

            if (hitWall)
            {
                balls[i].position = pos;

                Vector3 directionToCenter = center - pos;
                directionToCenter.y = 0f;

                if (directionToCenter.sqrMagnitude < 0.01f)
                    directionToCenter = Random.insideUnitSphere;

                directionToCenter.y = 0f;
                directionToCenter.Normalize();

                ballRigidbodies[i].linearVelocity = directionToCenter * currentSpeed;
            }
        }
    }

    void SetBallMaterial(int index, Material material)
    {
        balls[index].GetComponent<Renderer>().material = material;
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + Mathf.FloorToInt(score);

        if (timerText != null)
            timerText.text = "Time: " + Mathf.CeilToInt(gameTimer);
    }

    void EndGame()
    {
        gameEnded = true;

        for (int i = 0; i < ballRigidbodies.Length; i++)
        {
            ballRigidbodies[i].linearVelocity = Vector3.zero;
        }

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.transform.SetAsLastSibling();
            gameOverText.text = "GAME OVER\nFinal Score: " + Mathf.FloorToInt(score);
        }
    }
}