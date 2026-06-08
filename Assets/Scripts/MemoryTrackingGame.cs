using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

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
    public Material[] decoyMaterials;

    [Header("Game Settings")]
    public float gameTime = 60f;
    public float showGreenTime = 3f;
    public float hiddenTrackingTime = 8f;

    [Header("Win Settings")]
    public float winScore = 150f;
    public string brainUnlockSceneName = "BrainUnlockScene";
    public float winDelayBeforeBrainScene = 3f;

    [Header("Scoring")]
    public float followDistance = 1.2f;
    public float pointsPerSecond = 7f;
    public float losePointsPerSecond = 5f;

    [Header("Movement")]
    public float startSpeed = 3f;
    public float speedIncreaseEveryPoint = 0.06f;
    public float maxSpeed = 11f;

    [Header("Area Limits")]
    public float minX = -9f;
    public float maxX = 9f;
    public float minZ = -9f;
    public float maxZ = 9f;

    private Rigidbody[] ballRigidbodies;
    private int correctBallIndex = -1;
    private int previousCorrectBallIndex = -1;

    private float currentSpeed;
    private float score;
    private float gameTimer;
    private float memoryTimer;

    private bool showingGreen = true;
    private bool gameEnded = false;
    private bool started = false;

    public void StartGame()
    {
        started = true;
        gameEnded = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        currentSpeed = startSpeed;
        score = 0f;
        gameTimer = gameTime;
        memoryTimer = 0f;

        ballRigidbodies = new Rigidbody[balls.Length];

        for (int i = 0; i < balls.Length; i++)
        {
            ballRigidbodies[i] = balls[i].GetComponent<Rigidbody>();

            ballRigidbodies[i].useGravity = false;
            ballRigidbodies[i].linearDamping = 0f;
            ballRigidbodies[i].angularDamping = 0f;

            ballRigidbodies[i].constraints =
                RigidbodyConstraints.FreezePositionY |
                RigidbodyConstraints.FreezeRotationX |
                RigidbodyConstraints.FreezeRotationY |
                RigidbodyConstraints.FreezeRotationZ;

            SetRandomNormalMaterial(i);
            GiveRandomVelocity(i);
        }

        PickNewCorrectBall();

        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);

        UpdateUI();
    }

    void Update()
    {
        if (!started || gameEnded) return;

        HandleGameTimer();
        HandleMemoryTimer();
        CheckMouseTracking();
        UpdateUI();
    }

    void FixedUpdate()
    {
        if (!started || gameEnded) return;

        KeepBallSpeeds();
        KeepBallsInsideArea();
    }

    void HandleGameTimer()
    {
        gameTimer -= Time.deltaTime;

        if (gameTimer <= 0f)
        {
            gameTimer = 0f;
            CheckWinOrLose();
        }
    }

    void CheckWinOrLose()
    {
        if (score >= winScore)
            WinGame();
        else
            LoseGame();
    }

    void WinGame()
    {
        gameEnded = true;
        StopBalls();

        PlayerPrefs.SetInt("MotionTrackingCompleted", 1);
        GameSessionState.motionDone = true;
        PlayerPrefs.SetString("UnlockedBrainPart", "OccipitalLobe");
        PlayerPrefs.Save();

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.transform.SetAsLastSibling();
            gameOverText.text = "YOU WIN!\nFinal Score: " + Mathf.FloorToInt(score);
        }

        StartCoroutine(GoToBrainSceneAfterDelay());
    }

    IEnumerator GoToBrainSceneAfterDelay()
    {
        yield return new WaitForSeconds(winDelayBeforeBrainScene);
        SceneManager.LoadScene(brainUnlockSceneName);
    }

    void LoseGame()
    {
        gameEnded = true;
        StopBalls();

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.transform.SetAsLastSibling();
            gameOverText.text =
                "GAME OVER\nFinal Score: " + Mathf.FloorToInt(score) +
                "\nNeed " + Mathf.FloorToInt(winScore) + " to win";
        }
    }

    void StopBalls()
    {
        if (ballRigidbodies == null) return;

        for (int i = 0; i < ballRigidbodies.Length; i++)
        {
            if (ballRigidbodies[i] != null)
                ballRigidbodies[i].linearVelocity = Vector3.zero;
        }
    }

    void HandleMemoryTimer()
    {
        memoryTimer += Time.deltaTime;

        if (showingGreen && memoryTimer >= showGreenTime)
        {
            SetRandomNormalMaterial(correctBallIndex);
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
            float distance = Vector3.Distance(hit.point, balls[correctBallIndex].position);

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
            SetRandomNormalMaterial(i);

        int newIndex;

        do
        {
            newIndex = Random.Range(0, balls.Length);
        }
        while (balls.Length > 1 && newIndex == previousCorrectBallIndex);

        correctBallIndex = newIndex;
        previousCorrectBallIndex = correctBallIndex;

        SetBallMaterial(correctBallIndex, greenMaterial);

        showingGreen = true;
        memoryTimer = 0f;
    }

    void SetRandomNormalMaterial(int index)
    {
        if (decoyMaterials != null && decoyMaterials.Length > 0)
        {
            Material randomMat = decoyMaterials[Random.Range(0, decoyMaterials.Length)];
            SetBallMaterial(index, randomMat);
        }
        else
        {
            SetBallMaterial(index, greyMaterial);
        }
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
}