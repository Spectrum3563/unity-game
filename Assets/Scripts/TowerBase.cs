using UnityEngine;

public class TowerBase : MonoBehaviour
{
    public enum TowerType
    {
        Ranged,
        Slow,
        Dot
    }

    [Header("Tower")]
    public TowerType towerType = TowerType.Ranged;
    [Range(1, 3)]
    public int level = 1;

    [Header("Combat")]
    public float range = 5f;
    public float attackInterval = 0.3f;
    public float damage = 150f;
    public int maxTargets = 1;
    public float projectileSpeed = 4f;

    [Header("Slow")]
    [Tooltip("Movement speed multiplier applied to enemies.")]
    [Range(0f, 1f)]
    public float slowMultiplier = 0.5f;

    [Tooltip("How long the slow lasts.")]
    public float slowDuration = 1f;

    [Header("Placement")]
    public Vector2Int occupiedCell;
    public bool placeOnWall = true;
    public bool placeOnPath = false;

    [Header("Detection")]
    public LayerMask enemyLayer;

    [Header("Ghost")]
    public SpriteRenderer ghostRenderer;

    private float cooldown;

    private void Awake()
    {
        ApplyStats();
    }

    private void OnValidate()
    {
        ApplyStats();
    }

    private void Update()
    {
        if (cooldown > 0f)
        {
            cooldown -= Time.deltaTime;
            return;
        }

        switch (towerType)
        {
            case TowerType.Ranged:
                AttackRanged();
                cooldown = attackInterval;
                break;

            case TowerType.Slow:
                AttackSlow();
                cooldown = attackInterval;
                break;

            case TowerType.Dot:
                AttackDot();
                cooldown = attackInterval;
                break;
        }
    }

    private void ApplyStats()
    {
        switch (towerType)
        {
            case TowerType.Ranged:
                placeOnWall = true;
                placeOnPath = false;

                switch (level)
                {
                    case 1:
                        range = 5f;
                        damage = 150f;
                        attackInterval = 0.3f;
                        projectileSpeed = 4f;
                        maxTargets = 1;
                        break;

                    case 2:
                        range = 6f;
                        damage = 300f;
                        attackInterval = 0.3f;
                        projectileSpeed = 5f;
                        maxTargets = 1;
                        break;

                    case 3:
                        range = 7f;
                        damage = 500f;
                        attackInterval = 0.2f;
                        projectileSpeed = 7f;
                        maxTargets = 2;
                        break;
                }

                break;

            case TowerType.Slow:
                placeOnWall = true;
                placeOnPath = false;

                switch (level)
                {
                    case 1:
                        range = 4f;
                        break;

                    case 2:
                        range = 6f;
                        break;

                    case 3:
                        range = 8f;
                        break;
                }

                damage = 0f;
                break;

            case TowerType.Dot:
                placeOnWall = false;
                placeOnPath = true;

                range = 0f;

                switch (level)
                {
                    case 1:
                        damage = 80f;
                        break;

                    case 2:
                        damage = 120f;
                        break;

                    case 3:
                        damage = 200f;
                        break;
                }

                attackInterval = 0.1f;
                break;
        }
    }

    private void AttackRanged()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            transform.position,
            range,
            enemyLayer
        );

        if (enemies.Length == 0)
            return;

        int targets = Mathf.Min(maxTargets, enemies.Length);

        for (int i = 0; i < targets; i++)
        {
            EnemyController enemy = enemies[i].GetComponent<EnemyController>();

            if (enemy == null || enemy.isDead)
                continue;

            enemy.TakeDamage(damage);
        }
    }

    private void AttackSlow()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            transform.position,
            range,
            enemyLayer
        );

        foreach (Collider2D collider in enemies)
        {
            EnemyController enemy = collider.GetComponent<EnemyController>();

            if (enemy == null || enemy.isDead)
                continue;

            enemy.ApplySlow(slowMultiplier, slowDuration);
        }
    }

    private void AttackDot()
    {
        /*
         * DOT will be implemented separately.
         *
         * Since this tower is placed directly on the path,
         * it should eventually damage enemies occupying
         * the same path cell as this tower.
         */
    }

    public virtual bool IsPlacementAllowed(TilePlacementType placementType)
    {
        if (towerType == TowerType.Dot)
            return placementType == TilePlacementType.Path;

        return placementType == TilePlacementType.Wall;
    }

    public virtual void LevelUp()
    {
        if (level >= 3)
            return;

        level++;
        ApplyStats();
    }

    private void OnDrawGizmosSelected()
    {
        if (towerType == TowerType.Dot)
            return;

        Gizmos.DrawWireSphere(transform.position, range);
    }
}

public enum TilePlacementType
{
    Wall,
    Path,
    Invalid
}