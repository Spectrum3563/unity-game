using UnityEngine;
using UnityEngine.Rendering;

public class TowerBase : MonoBehaviour
{
    private const int RangeCircleSegments = 64;
    private const float RangeCircleOpacity = 0.12f;
    private static Mesh rangeCircleMesh;
    private static Material rangedRangeCircleMaterial;
    private static Material slowRangeCircleMaterial;
    private static bool rangeCircleInitializationAttempted;

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

    [Header("Effects")]
    public GameObject projectilePrefab;
    public GameObject hitEffectPrefab;
    public float hitEffectLifetime = 0.5f;

    [Header("Hit Effect Per Level")]
    [Tooltip("Element 0 = level 1, Element 1 = level 2, Element 2 = level 3. Empty slots use Hit Effect Prefab above.")]
    public GameObject[] levelHitEffects = new GameObject[3];

    [Tooltip("Lifetime per level. 0 uses Hit Effect Lifetime above.")]
    public float[] levelHitEffectLifetimes = new float[3];

    [Header("Level Up Effect")]
    [Tooltip("Spawned on the tower each time it levels up.")]
    public GameObject levelUpEffectPrefab;
    public float levelUpEffectLifetime = 1f;
    [Tooltip("Offset from the tower position, e.g. (0, 0.3, 0).")]
    public Vector3 levelUpEffectOffset = Vector3.zero;

    [Header("Level Sprites")]
    [Tooltip("Element 0 = level 1, Element 1 = level 2, Element 2 = level 3")]
    public Sprite[] levelSprites;

    [Tooltip("Size multiplier per level. Element 0 = level 1, Element 1 = level 2, Element 2 = level 3. 1 = original size.")]
    public float[] levelScales = { 1f, 1f, 1f };

    private float cooldown;
    private bool isPlacementPreview;
    private SpriteRenderer spriteRenderer;
    private Vector3 baseScale = Vector3.one;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            baseScale = spriteRenderer.transform.localScale;

        ApplyStats();
        UpdateSprite();
    }

    private void OnValidate()
    {
        ApplyStats();
    }

    private void Update()
    {
        if (isPlacementPreview)
            return;

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

    private void LateUpdate()
    {
        if ((towerType != TowerType.Slow && towerType != TowerType.Ranged) ||
            range <= 0f)
            return;

        Camera camera = Camera.main;
        if (camera == null)
            return;

        if (!TryCreateRangeCircleResources())
            return;

        Material material = towerType == TowerType.Ranged
            ? rangedRangeCircleMaterial
            : slowRangeCircleMaterial;

        Vector3 position = transform.position + Vector3.forward * 0.05f;
        Matrix4x4 matrix = Matrix4x4.TRS(
            position,
            Quaternion.identity,
            new Vector3(range, range, 1f)
        );

        Graphics.DrawMesh(
            rangeCircleMesh,
            matrix,
            material,
            gameObject.layer,
            camera,
            0,
            null,
            ShadowCastingMode.Off,
            false
        );
    }

    private static bool TryCreateRangeCircleResources()
    {
        if (rangeCircleMesh != null &&
            rangedRangeCircleMaterial != null &&
            slowRangeCircleMaterial != null)
            return true;

        if (rangeCircleInitializationAttempted)
            return false;

        rangeCircleInitializationAttempted = true;
        Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        if (shader == null)
        {
            Debug.LogError(
                "Could not find a compatible sprite shader for the slow tower range indicator."
            );
            return false;
        }

        rangeCircleMesh = new Mesh
        {
            name = "Slow Tower Range Circle",
            hideFlags = HideFlags.HideAndDontSave
        };

        Vector3[] vertices = new Vector3[RangeCircleSegments + 1];
        int[] triangles = new int[RangeCircleSegments * 3];
        for (int i = 0; i < RangeCircleSegments; i++)
        {
            float angle = i * Mathf.PI * 2f / RangeCircleSegments;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);

            int triangleIndex = i * 3;
            triangles[triangleIndex] = 0;
            triangles[triangleIndex + 1] = i + 1;
            triangles[triangleIndex + 2] = (i + 1) % RangeCircleSegments + 1;
        }

        rangeCircleMesh.vertices = vertices;
        rangeCircleMesh.triangles = triangles;
        rangeCircleMesh.RecalculateBounds();

        rangedRangeCircleMaterial = CreateRangeCircleMaterial(
            shader,
            "Ranged Tower Range Circle",
            new Color(1f, 0.1f, 0.1f, RangeCircleOpacity)
        );
        slowRangeCircleMaterial = CreateRangeCircleMaterial(
            shader,
            "Slow Tower Range Circle",
            new Color(0f, 0.85f, 1f, RangeCircleOpacity)
        );

        return true;
    }

    private static Material CreateRangeCircleMaterial(
        Shader shader,
        string materialName,
        Color color
    )
    {
        return new Material(shader)
        {
            name = materialName,
            color = color,
            renderQueue = (int)RenderQueue.Transparent,
            hideFlags = HideFlags.HideAndDontSave
        };
    }

    public void SetPlacementPreviewMode()
    {
        isPlacementPreview = true;
        cooldown = 0f;
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

    private void UpdateSprite()
    {
        if (spriteRenderer == null)
            return;

        int index = level - 1;

        if (levelSprites != null &&
            index >= 0 &&
            index < levelSprites.Length &&
            levelSprites[index] != null)
        {
            spriteRenderer.sprite = levelSprites[index];
        }

        if (levelScales != null &&
            index >= 0 &&
            index < levelScales.Length &&
            levelScales[index] > 0f)
        {
            spriteRenderer.transform.localScale = baseScale * levelScales[index];
        }
    }

    private GameObject GetHitEffectForLevel()
    {
        int index = level - 1;

        if (levelHitEffects != null &&
            index >= 0 &&
            index < levelHitEffects.Length &&
            levelHitEffects[index] != null)
        {
            return levelHitEffects[index];
        }

        return hitEffectPrefab;
    }

    private float GetHitEffectLifetimeForLevel()
    {
        int index = level - 1;

        if (levelHitEffectLifetimes != null &&
            index >= 0 &&
            index < levelHitEffectLifetimes.Length &&
            levelHitEffectLifetimes[index] > 0f)
        {
            return levelHitEffectLifetimes[index];
        }

        return hitEffectLifetime;
    }

    private void SpawnHitEffect(Vector3 position)
    {
        GameObject effect = GetHitEffectForLevel();

        if (effect == null)
            return;

        GameObject fx = Instantiate(effect, position, Quaternion.identity);
        Destroy(fx, GetHitEffectLifetimeForLevel());
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

            if (projectilePrefab != null)
            {
                GameObject p = Instantiate(
                    projectilePrefab,
                    transform.position,
                    Quaternion.identity
                );

                Projectile projectile = p.GetComponent<Projectile>();

                if (projectile != null)
                {
                    projectile.Init(
                        enemy,
                        damage,
                        projectileSpeed,
                        GetHitEffectForLevel(),
                        GetHitEffectLifetimeForLevel()
                    );
                }
                else
                {
                    Destroy(p);
                }
            }
            else
            {
                // No projectile assigned: instant hit.
                enemy.TakeDamage(damage);
                SpawnHitEffect(enemy.transform.position);
            }
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
        UpdateSprite();
        SpawnLevelUpEffect();
    }

    private void SpawnLevelUpEffect()
    {
        if (levelUpEffectPrefab == null)
            return;

        GameObject fx = Instantiate(
            levelUpEffectPrefab,
            transform.position + levelUpEffectOffset,
            Quaternion.identity
        );

        Destroy(fx, levelUpEffectLifetime);
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