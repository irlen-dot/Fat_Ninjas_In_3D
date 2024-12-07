using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int poolSize = 10;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private bool spawnOnStart = true;
    private Transform enemyParent;
    [SerializeField] private Transform routeParent;

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
}