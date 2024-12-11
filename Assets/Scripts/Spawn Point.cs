using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEditor.EditorTools;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int poolSize = 10;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private bool spawnOnStart = true;
    private Transform enemyParent;
    [SerializeField] private Transform routeParent;

    [SerializeField]
    [Tooltip("If the player will break the glass, will these NPCs be triggered by It?")]
    private bool triggeredByGlass = true;
    [SerializeField] private float waitTime = 2f;
    [SerializeField] private Color gizmosColor = Color.red;


    private List<Transform> routePoints = new List<Transform>();
    private Queue<GameObject> enemyPool;
    private bool isSpawning = false;
    private int currentRouteIndex = 0;

    void Start()
    {
        // Get all route points from routeParent
        if (routeParent != null)
        {
            foreach (Transform child in routeParent)
            {
                routePoints.Add(child);
            }
        }
        else
        {
            Debug.LogError("Route Parent is not assigned!");
            return;
        }

        InitializePool();

        if (spawnOnStart)
        {
            StartSpawning();
        }
    }

    private void InitializePool()
    {
        enemyPool = new Queue<GameObject>();

        if (enemyParent == null)
        {
            enemyParent = transform;
        }

        for (int i = 0; i < poolSize; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab, enemyParent);
            Enemy enemyComponent = enemy.GetComponent<Enemy>();
            enemyComponent.IsProvokedByGlass = triggeredByGlass;
            enemyComponent.WaitTime = waitTime;
            if (enemyComponent != null)
            {
                enemyComponent.SetRoutePoints(routePoints);
            }

            enemy.SetActive(false);
            enemyPool.Enqueue(enemy);
        }
    }

    private void AssignRouteToEnemy(Enemy enemy)
    {
        enemy.SetRoutePoints(routePoints);
    }

    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            StartCoroutine(SpawnRoutine());
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }

    private IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPool.Count == 0 || routePoints.Count == 0)
        {
            Debug.LogWarning("NPC pool is empty or no route points defined!");
            return;
        }

        GameObject enemy = enemyPool.Dequeue();
        enemy.transform.position = transform.position;
        enemy.transform.rotation = transform.rotation;

        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            AssignRouteToEnemy(enemyComponent);
        }

        enemy.SetActive(true);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmosColor;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmosColor;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}