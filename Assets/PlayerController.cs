using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Runtime.InteropServices;

public class PlayerController : MonoBehaviour
{
    public float forwardSpeed = 15f;
    public float maxSpeed = 40f;
    public float acceleration = 0.5f;
    public float steerSpeed = 10f;
    [HideInInspector] public float mapWidth;
    public float flightDuration = 5f;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public GameObject gameOverPanel;
    public GameObject tutorialPanel;
    public Transform cameraGhost;
    public ParticleSystem dirtParticles;

    public AudioClip powerUpSound;
    public AudioClip redBullVoiceSound;
    public AudioClip flapSound;
    public AudioClip crashSound;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private AudioSource audioSource;
    private Color originalColor;
    private int originalSortingOrder;
    private bool isAlive = true;
    private bool isFlying = false;
    private float horizontalInput = 0f;
    public TextMeshProUGUI playerNameUI;

    [DllImport("__Internal")]
    private static extern void SendScoreToBubble(int score);

    void Start()
    {
        float screenEdge = Camera.main.orthographicSize * Camera.main.aspect;
        mapWidth = screenEdge - 0.5f;

        Application.targetFrameRate = 60;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            originalSortingOrder = spriteRenderer.sortingOrder;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        int savedHighScore = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
        {
            highScoreText.text = "BEST: " + savedHighScore + "m";
        }

        Time.timeScale = 1f;

        if (tutorialPanel != null)
        {
            if (savedHighScore == 0)
            {
                tutorialPanel.SetActive(true);
                Time.timeScale = 0f;
            }
            else
            {
                tutorialPanel.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (!isAlive) return;

        horizontalInput = 0f;
        bool inputDetected = false;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
            {
                horizontalInput = -1f;
                inputDetected = true;
            }
            else if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
            {
                horizontalInput = 1f;
                inputDetected = true;
            }
        }

        if (horizontalInput == 0f)
        {
            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                float screenHalf = Screen.width / 2;
                if (Mouse.current.position.ReadValue().x < screenHalf)
                    horizontalInput = -1f;
                else
                    horizontalInput = 1f;

                inputDetected = true;
            }
            else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                float screenHalf = Screen.width / 2;
                if (Touchscreen.current.primaryTouch.position.ReadValue().x < screenHalf)
                    horizontalInput = -1f;
                else
                    horizontalInput = 1f;

                inputDetected = true;
            }
        }

        if (inputDetected && tutorialPanel != null && tutorialPanel.activeSelf)
        {
            tutorialPanel.SetActive(false);
            Time.timeScale = 1f;
        }

        int currentScore = Mathf.Abs(Mathf.FloorToInt(transform.position.y));
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString() + "m";
        }
    }

    void FixedUpdate()
    {
        if (!isAlive) return;

        if (forwardSpeed < maxSpeed)
        {
            forwardSpeed += acceleration * Time.fixedDeltaTime;
        }

        rb.linearVelocity = new Vector2(horizontalInput * steerSpeed, -forwardSpeed);
    }

    void LateUpdate()
    {
        if (!isAlive) return;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -mapWidth, mapWidth);
        transform.position = pos;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("PowerUp"))
        {
            if (audioSource != null)
            {
                if (powerUpSound != null) audioSource.PlayOneShot(powerUpSound);
                if (redBullVoiceSound != null) audioSource.PlayOneShot(redBullVoiceSound);
            }

            Destroy(collision.gameObject);
            StartCoroutine(FlyMode());
        }
        else if (collision.gameObject.CompareTag("Obstacle") && !isFlying)
        {
            Die();
        }
    }

    IEnumerator FlyMode()
    {
        isFlying = true;

        if (dirtParticles != null) dirtParticles.Stop();
        transform.localScale = new Vector3(1.3f, 1.3f, 1f);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.cyan;
            spriteRenderer.sortingOrder = 100;
        }

        float flapTimer = 0f;
        float actualFlightTime = (flightDuration * 2.5f) - 2f;

        while (flapTimer < actualFlightTime)
        {
            if (audioSource != null && flapSound != null)
            {
                audioSource.PlayOneShot(flapSound);
            }
            yield return new WaitForSeconds(0.4f);
            flapTimer += 0.4f;
        }

        for (int i = 0; i < 10; i++)
        {
            if (spriteRenderer != null) spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.1f);
            if (spriteRenderer != null) spriteRenderer.color = Color.cyan;
            yield return new WaitForSeconds(0.1f);
        }

        transform.localScale = new Vector3(1f, 1f, 1f);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
            spriteRenderer.sortingOrder = originalSortingOrder;
        }

        if (dirtParticles != null) dirtParticles.Play();

        isFlying = false;
    }

    void Die()
    {
        isAlive = false;
        rb.linearVelocity = Vector2.zero;

        if (animator != null)
        {
            animator.enabled = false;
        }

        if (audioSource != null && crashSound != null)
        {
            audioSource.PlayOneShot(crashSound);
        }

        if (dirtParticles != null)
        {
            dirtParticles.Stop();
        }

        int score = Mathf.Abs(Mathf.FloorToInt(transform.position.y));

#if UNITY_WEBGL && !UNITY_EDITOR
        SendScoreToBubble(score);
#endif

        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        if (score > highScore)
        {
            PlayerPrefs.SetInt("HighScore", score);
            highScore = score;
        }

        if (scoreText != null)
        {
            scoreText.text = "Score: " + score + "m";
            scoreText.color = Color.cyan;
        }

        if (highScoreText != null)
        {
            highScoreText.text = "BEST: " + highScore + "m";
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (cameraGhost != null)
        {
            StartCoroutine(ShakeCamera());
        }
    }

    IEnumerator ShakeCamera()
    {
        Vector3 originalPos = cameraGhost.localPosition;
        float elapsed = 0.0f;
        float duration = 0.3f;
        float magnitude = 0.5f;

        while (elapsed < duration)
        {
            float x = originalPos.x + Random.Range(-1f, 1f) * magnitude;
            float y = originalPos.y + Random.Range(-1f, 1f) * magnitude;

            cameraGhost.localPosition = new Vector3(x, y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraGhost.localPosition = originalPos;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SetPlayerName(string nameFromBubble)
    {
        if (playerNameUI != null)
        {
            playerNameUI.text = nameFromBubble;
        }
        Debug.Log("Name received from Bubble: " + nameFromBubble);
    }
}