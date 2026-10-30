using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Spawn")]
    [SerializeField] private float spawnInterval = 2f;

    [Header("Small - 50%")]
    [SerializeField] private float smallSpeedMin = 0.8f;
    [SerializeField] private float smallSpeedMax = 1.2f;
    [SerializeField] private float smallHealth = 1000f;

    [Header("Medium - 35%")]
    [SerializeField] private float mediumSpeedMin = 0.5f;
    [SerializeField] private float mediumSpeedMax = 0.8f;
    [SerializeField] private float mediumHealth = 2500f;

    [Header("Large - 15%")]
    [SerializeField] private float largeSpeedMin = 0.3f;
    [SerializeField] private float largeSpeedMax = 0.5f;
    [SerializeField] private float largeHealth = 5000f;

    private float spawnTimer;

    private void Start()
    {
        spawnTimer = spawnInterval;
    }

    private void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnEnemy();
            spawnTimer = spawnInterval;
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null || spawnPoint == null)
        {
            Debug.LogError("EnemySpawner is missing Enemy Prefab or Spawn Point.");
            return;
        }

        GameObject enemyObject = Instantiate(
            enemyPrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        EnemyController enemy = enemyObject.GetComponent<EnemyController>();

        if (enemy == null)
        {
            Debug.LogError("Enemy prefab does not have an EnemyController.");
            return;
        }

        // Weighted random selection:
        // Small  = 50%
        // Medium = 35%
        // Large  = 15%

        float roll = Random.Range(0f, 100f);

        EnemyController.EnemySize size;

        if (roll < 50f)
        {
            size = EnemyController.EnemySize.Small;
        }
        else if (roll < 85f)
        {
            size = EnemyController.EnemySize.Medium;
        }
        else
        {
            size = EnemyController.EnemySize.Large;
        }

        switch (size)
        {
            case EnemyController.EnemySize.Small:
                enemy.Configure(
                    size,
                    smallSpeedMin,
                    smallSpeedMax,
                    smallHealth
                );
                break;

            case EnemyController.EnemySize.Medium:
                enemy.Configure(
                    size,
                    mediumSpeedMin,
                    mediumSpeedMax,
                    mediumHealth
                );
                break;

            case EnemyController.EnemySize.Large:
                enemy.Configure(
                    size,
                    largeSpeedMin,
                    largeSpeedMax,
                    largeHealth
                );
                break;
        }
    }
}