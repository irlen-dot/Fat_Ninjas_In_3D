using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    private NavMeshAgent nav;
    private List<Transform> routePoints = new List<Transform>();
    private int currentPointIndex = 0;
    private bool isWaiting = false;
    [SerializeField] private float waitTime = 2f;

    public bool IsProvokedByGlass;
    // public bool IsProvokedByGlass { set { isProvokedByGlass = value; } }


    void Start()
    {
        nav = GetComponent<NavMeshAgent>();
        if (routePoints != null && routePoints.Count > 0)
        {
            MoveToNextPoint();
        }
    }

    public void SetTargetPosition(Vector3 position)
    {
        if (nav != null)
        {
            nav.SetDestination(position);
        }
    }

    public void SetRoutePoints(List<Transform> points)
    {
        if (points != null && points.Count > 0)
        {
            routePoints = new List<Transform>(points);
            // If we're setting new route points and we're already active,
            // start moving to the first point
            if (gameObject.activeInHierarchy && nav != null)
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
        if (isWaiting || routePoints == null || routePoints.Count == 0) return;

        if (!nav.pathPending && nav.remainingDistance <= nav.stoppingDistance)
        {
            StartCoroutine(WaitAtCurrentPoint());
        }
    }

    private void MoveToNextPoint()
    {
        if (routePoints == null || routePoints.Count == 0) return;

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

        nav.isStopped = false;
        currentPointIndex++;
        MoveToNextPoint();

        isWaiting = false;
    }

    // Optional: Add method to clear route points when enemy is returned to pool
    public void ClearRoutePoints()
    {
        routePoints.Clear();
        currentPointIndex = 0;
    }
}