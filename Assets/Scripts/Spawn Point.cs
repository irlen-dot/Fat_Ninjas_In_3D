using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int poolSize = 10;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private Transform enemyParent;

    private Queue<GameObject> enemyPool;
    private bool isSpawning = false;

    void Start()
    {
        InitializePool();
        
        if (spawnOnStart)
        {
            StartSpawning();
        }
    }

    private void InitializePool()
    {
        enemyPool = new Queue<GameObject>();

        enemyParent = transform;
        // if (enemyParent == null)
        // {
        //     enemyParent = new GameObject("EnemyPool").transform;
        // }

        for (int i = 0; i < poolSize; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab, enemyParent);
            enemy.SetActive(false);
            enemyPool.Enqueue(enemy);
        }
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
        if (enemyPool.Count == 0)
        {
            Debug.LogWarning("NPC pool is empty! Consider increasing pool size.");
            return;
        }

        GameObject enemy = enemyPool.Dequeue();
        enemy.transform.position = transform.position;
        enemy.transform.rotation = transform.rotation;
        enemy.SetActive(true);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
    }
}