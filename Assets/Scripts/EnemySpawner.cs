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

    [Header("Difficulty Scaling Over Time")]
    [Tooltip("Extra health per minute, as a fraction. 0.25 = +25% of base health each minute.")]
    [SerializeField] private float healthGrowthPerMinute = 0.25f;

    [Tooltip("Extra speed per minute, as a fraction. 0.05 = +5% of base speed each minute.")]
    [SerializeField] private float speedGrowthPerMinute = 0.05f;

    [Tooltip("Speed multiplier will never exceed this value.")]
    [SerializeField] private float maxSpeedMultiplier = 1.5f;

    [Tooltip("Seconds removed from the spawn interval each minute.")]
    [SerializeField] private float spawnIntervalReductionPerMinute = 0.1f;

    [Tooltip("The spawn interval will never go below this value.")]
    [SerializeField] private float minSpawnInterval = 0.6f;

    private float spawnTimer;
    private float elapsedTime;

    private float ElapsedMinutes => elapsedTime / 60f;

    // Final multipliers = growth over time x the chosen difficulty.
    private float HealthMultiplier =>
        (1f + healthGrowthPerMinute * DifficultySettings.GrowthMultiplier * ElapsedMinutes)
        * DifficultySettings.EnemyHealthMultiplier;

    private float SpeedMultiplier =>
        Mathf.Min(
            1f + speedGrowthPerMinute * DifficultySettings.GrowthMultiplier * ElapsedMinutes,
            maxSpeedMultiplier
        )
        * DifficultySettings.EnemySpeedMultiplier;

    private float CurrentSpawnInterval =>
        Mathf.Max(
            spawnInterval
            - spawnIntervalReductionPerMinute * DifficultySettings.GrowthMultiplier * ElapsedMinutes,
            minSpawnInterval
        )
        * DifficultySettings.SpawnIntervalMultiplier;

    private void Start()
    {
        spawnTimer = spawnInterval * DifficultySettings.SpawnIntervalMultiplier;
        elapsedTime = 0f;
    }

    private void Update()
    {
        // Time.deltaTime is 0 while the game is paused (start menu, game over),
        // so the difficulty only grows while the game is actually running.
        elapsedTime += Time.deltaTime;

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnEnemy();
            spawnTimer = CurrentSpawnInterval;
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

        float healthMult = HealthMultiplier;
        float speedMult = SpeedMultiplier;

        switch (size)
        {
            case EnemyController.EnemySize.Small:
                enemy.Configure(
                    size,
                    smallSpeedMin * speedMult,
                    smallSpeedMax * speedMult,
                    smallHealth * healthMult
                );
                break;

            case EnemyController.EnemySize.Medium:
                enemy.Configure(
                    size,
                    mediumSpeedMin * speedMult,
                    mediumSpeedMax * speedMult,
                    mediumHealth * healthMult
                );
                break;

            case EnemyController.EnemySize.Large:
                enemy.Configure(
                    size,
                    largeSpeedMin * speedMult,
                    largeSpeedMax * speedMult,
                    largeHealth * healthMult
                );
                break;
        }
    }
}