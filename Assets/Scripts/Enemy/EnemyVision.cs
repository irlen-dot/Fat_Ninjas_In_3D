using UnityEngine;
using System.Collections;

public class EnemyVision : MonoBehaviour
{
    [Header("Vision Parameters")]
    [SerializeField] private float viewRadius = 8f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private float detectionTime = 1f;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Detection Indicators")]
    [SerializeField] private float maxAlertLevel = 100f;
    [SerializeField] private float alertIncreaseSpeed = 50f;
    [SerializeField] private float alertDecreaseSpeed = 25f;

    private Transform player;
    private float currentAlertLevel = 0f;
    private bool isPlayerDetected = false;
    private Vector3 lastKnownPosition;
    private bool isGamePaused = false;

    public delegate void OnPlayerDetectedHandler(Vector3 playerPosition);
    public event OnPlayerDetectedHandler OnPlayerDetected;

    public delegate void OnPlayerLostHandler();
    public event OnPlayerLostHandler OnPlayerLost;

    [Header("Light Settings")]
    [SerializeField] private Light spotLight;
    [SerializeField] private float lightIntensity = 2f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        StartCoroutine(VisionRoutine());

        // Setup spotlight if not assigned
        if (spotLight == null)
        {
            spotLight = gameObject.AddComponent<Light>();
            spotLight.type = LightType.Spot;
            spotLight.intensity = lightIntensity;
            spotLight.range = viewRadius;
            spotLight.spotAngle = viewAngle;
            spotLight.color = Color.white;
            spotLight.shadows = LightShadows.Hard;
        }
    }

    void Update()
    {
        // Cheat to resume game
        if (isGamePaused && Input.GetKeyDown(KeyCode.R))
        {
            ResumeGame();
        }
    }

    private IEnumerator VisionRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true)
        {
            if (!isGamePaused)
            {
                CheckVision();
            }
            yield return wait;
        }
    }

    private void CheckVision()
    {
        if (player == null) return;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= viewRadius)
        {
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleToPlayer <= viewAngle / 2)
            {
                if (!Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask))
                {
                    Debug.Log("Player is on sight.");
                    IncreaseAlertLevel();
                    lastKnownPosition = player.position;
                    return;
                }
            }
        }

        DecreaseAlertLevel();
    }


    private void ResumeGame()
    {
        Time.timeScale = 1;
        isGamePaused = false;
        Debug.Log("Game Resumed");
    }


    private void IncreaseAlertLevel()
    {
        currentAlertLevel += alertIncreaseSpeed * Time.deltaTime;
        currentAlertLevel = Mathf.Min(currentAlertLevel, maxAlertLevel); // Clamp the value
        Debug.Log($"Alert Level: {currentAlertLevel} / {maxAlertLevel}");

        if (currentAlertLevel >= maxAlertLevel && !isPlayerDetected)
        {
            isPlayerDetected = true;
            OnPlayerDetected?.Invoke(lastKnownPosition);

            // Check for spacebar cheat here
            if (!Input.GetKey(KeyCode.Space))
            {
                PauseGame();
                Debug.Log("Maximum alert reached - Pausing game");
            }
            else
            {
                Debug.Log("Cheat activated - Game continues!");
            }
        }
    }

    private void PauseGame()
    {
        if (!isGamePaused)
        {
            Time.timeScale = 0;
            isGamePaused = true;
            Debug.Log("Game Paused - Press R to resume");
        }
    }


    private void DecreaseAlertLevel()
    {
        currentAlertLevel -= alertDecreaseSpeed * Time.deltaTime;
        currentAlertLevel = Mathf.Max(0f, currentAlertLevel);

        if (currentAlertLevel <= 0f && isPlayerDetected)
        {
            isPlayerDetected = false;
            OnPlayerLost?.Invoke();
        }
    }

    public float GetAlertPercentage()
    {
        return currentAlertLevel / maxAlertLevel;
    }

    private void OnGUI()
    {
        if (isGamePaused)
        {
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 25, 200, 50),
                     "CAUGHT! - Press 'R' to resume");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 viewAngleA = DirFromAngle(-viewAngle / 2);
        Vector3 viewAngleB = DirFromAngle(viewAngle / 2);

        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);
    }

    private Vector3 DirFromAngle(float angleInDegrees)
    {
        angleInDegrees += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}