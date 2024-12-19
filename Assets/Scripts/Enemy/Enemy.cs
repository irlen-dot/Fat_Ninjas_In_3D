using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [Header("Debug Visualization")]
    [SerializeField] private bool showRouteGizmos = true;
    [SerializeField] private Color routeColor = Color.yellow;
    [SerializeField] private Color pointColor = Color.red;
    [SerializeField] private float pointSize = 0.5f;
    private Coroutine investigationCoroutine;
    private NavMeshAgent nav;
    private List<Transform> routePoints = new List<Transform>();
    private int currentPointIndex = 0;
    private bool isWaiting = false;
    private float waitTime = 2f;
    public float WaitTime { set { waitTime = value; } }

    public bool IsProvokedByGlass;
    private bool isRespondingToGlass = false;
    private Coroutine waitCoroutine;

    private List<Transform> investigationPoints = new List<Transform>();
    private int currentInvestigationIndex = 0;
    private bool isInvestigating = false;


    void Start()
    {
        nav = GetComponent<NavMeshAgent>();
        if (routePoints != null && routePoints.Count > 0)
        {
            MoveToNextPoint();
        }
    }

    public void SetInvestigationPoints(List<Transform> points)
    {
        if (points == null || points.Count == 0) return;

        // Stop any current waiting
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }

        // Stop any current investigation
        if (investigationCoroutine != null)
        {
            StopCoroutine(investigationCoroutine);
            investigationCoroutine = null;
        }

        // Reset waiting state
        isWaiting = false;
        nav.isStopped = false;

        investigationPoints = new List<Transform>(points);
        currentInvestigationIndex = 0;
        isInvestigating = true;
        isRespondingToGlass = true;

        // Start investigating
        MoveToNextInvestigationPoint();
    }

    private void MoveToNextInvestigationPoint()
    {
        if (currentInvestigationIndex >= investigationPoints.Count)
        {
            // Finished investigating, return to patrol
            FinishInvestigation();
            return;
        }

        if (investigationPoints[currentInvestigationIndex] != null)
        {
            nav.SetDestination(investigationPoints[currentInvestigationIndex].position);
        }
        else
        {
            currentInvestigationIndex++;
            MoveToNextInvestigationPoint();
        }
    }

    private void FinishInvestigation()
    {
        isInvestigating = false;
        isRespondingToGlass = false;
        investigationPoints.Clear();
        currentInvestigationIndex = 0;

        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }

        if (investigationCoroutine != null)
        {
            StopCoroutine(investigationCoroutine);
            investigationCoroutine = null;
        }

        // Resume patrol
        if (routePoints.Count > 0)
        {
            MoveToNextPoint();
        }
    }

    private IEnumerator WaitAndMoveToNextInvestigationPoint()
    {
        yield return new WaitForSeconds(waitTime);
        currentInvestigationIndex++;
        investigationCoroutine = null;  // Clear the reference
        MoveToNextInvestigationPoint();
    }

    public void SetRoutePoints(List<Transform> points)
    {
        if (points != null && points.Count > 0)
        {
            routePoints = new List<Transform>(points);
            // If we're setting new route points and we're already active,
            // start moving to the first point
            if (gameObject.activeInHierarchy && nav != null && !isRespondingToGlass)
            {
                currentPointIndex = 0;
                MoveToNextPoint();
            }
        }
        else
        {
            Debug.LogWarning("Attempted to set null or empty route points list");
        }
    }

    void Update()
    {
        if (!nav.pathPending && nav.remainingDistance <= nav.stoppingDistance)
        {
            if (isInvestigating && investigationCoroutine == null)
            {
                investigationCoroutine = StartCoroutine(WaitAndMoveToNextInvestigationPoint());
            }
            else if (!isWaiting && !isInvestigating && routePoints != null && routePoints.Count > 0)
            {
                waitCoroutine = StartCoroutine(WaitAtCurrentPoint());
            }
        }
    }

    private void MoveToNextPoint()
    {
        if (routePoints == null || routePoints.Count == 0) return;
        if (isRespondingToGlass) return; // Don't move to next point if responding to glass

        if (currentPointIndex >= routePoints.Count)
        {
            currentPointIndex = 0;
        }

        if (routePoints[currentPointIndex] != null)
        {
            nav.SetDestination(routePoints[currentPointIndex].position);
        }
        else
        {
            Debug.LogWarning($"Waypoint at index {currentPointIndex} is null");
            currentPointIndex = (currentPointIndex + 1) % routePoints.Count;
        }
    }

    private IEnumerator WaitAtCurrentPoint()
    {
        isWaiting = true;
        nav.isStopped = true;

        yield return new WaitForSeconds(waitTime);

        // Only proceed if we're not responding to glass
        if (!isRespondingToGlass)
        {
            nav.isStopped = false;
            currentPointIndex++;
            MoveToNextPoint();
        }

        isWaiting = false;
    }

    public void ClearRoutePoints()
    {
        routePoints.Clear();
        currentPointIndex = 0;
        isRespondingToGlass = false; // Reset glass response state
    }

    private void OnDrawGizmos()
    {
        // Draw route points
        if (routePoints != null && routePoints.Count > 0)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < routePoints.Count; i++)
            {
                if (routePoints[i] == null) continue;
                Gizmos.DrawWireSphere(routePoints[i].position, 0.3f);
                if (i < routePoints.Count - 1 && routePoints[i + 1] != null)
                {
                    Gizmos.DrawLine(routePoints[i].position, routePoints[i + 1].position);
                }
            }
        }

        // Draw investigation points if investigating
        if (isInvestigating && investigationPoints != null && investigationPoints.Count > 0)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < investigationPoints.Count; i++)
            {
                if (investigationPoints[i] == null) continue;
                Gizmos.DrawWireSphere(investigationPoints[i].position, 0.3f);
                if (i < investigationPoints.Count - 1 && investigationPoints[i + 1] != null)
                {
                    Gizmos.DrawLine(investigationPoints[i].position, investigationPoints[i + 1].position);
                }
            }

            // Highlight current investigation point
            if (currentInvestigationIndex < investigationPoints.Count &&
                investigationPoints[currentInvestigationIndex] != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(investigationPoints[currentInvestigationIndex].position, 0.4f);
            }
        }
    }
}