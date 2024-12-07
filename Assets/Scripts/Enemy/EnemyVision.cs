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

    public delegate void OnPlayerDetectedHandler(Vector3 playerPosition);
    public event OnPlayerDetectedHandler OnPlayerDetected;

    public delegate void OnPlayerLostHandler();
    public event OnPlayerLostHandler OnPlayerLost;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        StartCoroutine(VisionRoutine());
    }

    private IEnumerator VisionRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f); // Check every 0.2 seconds for performance

        while (true)
        {
            CheckVision();
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
                // Check if there are obstacles between enemy and player
                if (!Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask))
                {
                    Debug.Log("Player is on sight.");
                    // Player is in sight, increase alert level
                    IncreaseAlertLevel();
                    lastKnownPosition = player.position;
                    return;
                }
            }
        }

        // Player is not in sight, decrease alert level
        DecreaseAlertLevel();
    }

    private void IncreaseAlertLevel()
    {
        currentAlertLevel += alertIncreaseSpeed * Time.deltaTime;
        
        if (currentAlertLevel >= maxAlertLevel && !isPlayerDetected)
        {
            isPlayerDetected = true;
            OnPlayerDetected?.Invoke(lastKnownPosition);
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

    // Optional: Visualize the vision cone in the editor
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