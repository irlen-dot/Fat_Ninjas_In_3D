using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;

public class EnemyVision : MonoBehaviour
{
    [Header("Vision Parameters")]

    [SerializeField] private float viewRadius = 8f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private float detectionTime = 1f;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private bool affectedByTrapLight = true;

    [Header("Detection Indicators")]
    [SerializeField] private float maxAlertLevel = 100f;
    [SerializeField] private float alertIncreaseSpeed = 50f;
    [SerializeField] private float alertDecreaseSpeed = 25f;

    [Header("Vision Visualization")]
    [SerializeField] private CircleMesh visionMesh;


    private Transform player;
    private GameObject playerObj;
    private PlayerCollisionHandler playerCollisionHandler;

    private float currentAlertLevel = 0f;
    private bool isPlayerDetected = false;
    private Vector3 lastKnownPosition;
    private bool isGamePaused = false;

    public delegate void OnPlayerDetectedHandler(Vector3 playerPosition);
    public event OnPlayerDetectedHandler OnPlayerDetected;

    public delegate void OnPlayerLostHandler();
    public event OnPlayerLostHandler OnPlayerLost;
    private Light spotLight;

    [Header("Light Settings")]
    [SerializeField] private float lightIntensity = 2f;

    void Start()
    {
        playerObj = GameObject.FindGameObjectWithTag("Player")?.gameObject;
        player = playerObj.transform;
        playerCollisionHandler = playerObj.GetComponent<PlayerCollisionHandler>();

        StartCoroutine(VisionRoutine());

        // Initialize vision mesh if not set
        if (visionMesh == null)
        {
            GameObject visionObject = new GameObject("VisionMesh");
            visionObject.transform.parent = transform;
            visionObject.transform.localPosition = Vector3.zero;
            visionObject.transform.localRotation = Quaternion.Euler(-90, 0, 0); // Rotate to face forward
            visionMesh = visionObject.AddComponent<CircleMesh>();

            // Add MeshRenderer and set material
            MeshRenderer meshRenderer = visionObject.GetComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Standard"));
            meshRenderer.material.color = new Color(1f, 1f, 0f, 0.3f); // Semi-transparent yellow
        }

        // Update vision mesh parameters
        UpdateVisionMesh();
        InitializeSpotLight();
    }

    private void UpdateVisionMesh()
    {
        if (visionMesh != null)
        {
            // Set the properties
            visionMesh.radius = viewRadius;
            visionMesh.segments = 32;
            visionMesh.startAngle = -viewAngle / 2;
            visionMesh.endAngle = viewAngle / 2;

            // Force mesh update
            visionMesh.UpdateMesh(); // Add this method to your CircleMesh class
        }
    }

    public void UpdateViewRadius(float newRadius)
    {
        viewRadius = newRadius;
        UpdateVisionMesh();
        if (spotLight != null)
        {
            spotLight.range = viewRadius;
        }
    }

    public void UpdateViewAngle(float newAngle)
    {
        viewAngle = newAngle;
        UpdateVisionMesh();
        if (spotLight != null)
        {
            spotLight.spotAngle = viewAngle;
        }
    }

    private void InitializeSpotLight()
    {
        spotLight = gameObject.GetComponentInChildren<Light>();
        if (spotLight != null)
        {
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
        if (isGamePaused && Input.GetKeyDown(KeyCode.R))
        {
            ResumeGame();
        }
    }


    public bool AlertOnVisibility()
    {
        var (isVisible, _, _) = CheckExtendedVision();

        if (isVisible)
        {
            IncreaseAlertLevel();
            lastKnownPosition = player.position;
            return true;
        }

        return false;
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

        if (playerCollisionHandler.UnderTrapLight && affectedByTrapLight)
        {
            // If player is under trap light, directly check line of sight without distance restriction
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            if (angleToPlayer <= viewAngle / 2)
            {
                if (!Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask))
                {
                    Debug.Log("Player caught by trap light!");
                    IncreaseAlertLevel();
                    PauseGame();
                    lastKnownPosition = player.position;
                }
            }
        }
        else
        {
            // Normal vision check with view radius restriction
            if (distanceToPlayer <= viewRadius)
            {
                CheckPlayer(directionToPlayer, distanceToPlayer);
            }
        }
        DecreaseAlertLevel();
    }



    private void CheckPlayer(Vector3 directionToPlayer, float distanceToPlayer)
    {
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer <= viewAngle / 2)
        {
            if (!Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask))
            {
                Debug.Log("Player is on sight.");
                IncreaseAlertLevel();
                PauseGame();
                lastKnownPosition = player.position;
                return;
            }
        }
    }

    private (bool isVisible, float distance, bool isInRange) CheckExtendedVision()
    {
        if (player == null)
            return (false, 0f, false);

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        bool isInViewAngle = angleToPlayer <= viewAngle / 2;
        bool hasLineOfSight = !Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask);
        bool isInRange = distanceToPlayer <= viewRadius;

        bool isVisible = isInViewAngle && hasLineOfSight;

        if (isVisible)
        {
            Debug.Log($"Extended Vision - Player spotted at distance: {distanceToPlayer:F2}" +
                     $"{(isInRange ? " (In Range)" : " (Out of Range)")}");
        }

        return (isVisible, distanceToPlayer, isInRange);
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