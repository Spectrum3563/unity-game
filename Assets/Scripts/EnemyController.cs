using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    public enum EnemySize
    {
        Small = 0,
        Medium = 1,
        Large = 2
    }

    [Header("Enemy")]
    public EnemySize enemySize = EnemySize.Medium;

    public float movementSpeed = 0.35f;
    public float currentSpeed;

    public float maxHealth = 2500f;
    public float currentHealth;

    public float wallClearance = 0.75f;

    [Header("Colors")]
    public Color smallColor = Color.green;
    public Color mediumColor = Color.yellow;
    public Color largeColor = Color.red;

    [Header("Path")]
    public Transform currentTarget;
    private List<Vector3> path = new List<Vector3>();
    private int pathIndex;
    private PathGrid pathGrid;

    [Header("Death")]
    public float fadeDuration = 0.35f;
    public bool isDead;

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;

    private float slowMultiplier = 1f;
    private float slowTimer;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        body.gravityScale = 0f;
        body.linearDamping = 0f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        currentHealth = maxHealth;
        currentSpeed = movementSpeed;
    }

    private void Start()
    {
        pathGrid = FindFirstObjectByType<PathGrid>();
        playerHealth = FindFirstObjectByType<PlayerHealth>();

        GameObject targetObject =
            GameObject.FindGameObjectWithTag("IncursionPoint");

        if (pathGrid == null)
        {
            Debug.LogError("No PathGrid found.");
            return;
        }

        if (targetObject == null)
        {
            Debug.LogError("No object tagged IncursionPoint found.");
            return;
        }

        if (playerHealth == null)
        {
            Debug.LogError("No PlayerHealth found.");
        }

        path = pathGrid.FindPath(
            transform.position,
            targetObject.transform.position
        );

        pathIndex = 0;

        if (path.Count == 0)
        {
            Debug.LogError("Enemy could not find a path.");
        }
    }

    public void Configure(
        EnemySize size,
        float speedMin,
        float speedMax,
        float hp
    )
    {
        enemySize = size;

        movementSpeed = Random.Range(speedMin, speedMax);
        currentSpeed = movementSpeed;

        maxHealth = hp;
        currentHealth = maxHealth;

        float scale;

        switch (size)
        {
            case EnemySize.Small:
                scale = 0.3f;
                spriteRenderer.color = smallColor;
                break;

            case EnemySize.Medium:
                scale = 0.5f;
                spriteRenderer.color = mediumColor;
                break;

            case EnemySize.Large:
                scale = 0.8f;
                spriteRenderer.color = largeColor;
                break;

            default:
                scale = 0.5f;
                spriteRenderer.color = mediumColor;
                break;
        }

        transform.localScale = Vector3.one * scale;
        wallClearance = 1.5f * scale;
    }

    public void ApplySlow(float multiplier, float duration)
    {
        slowMultiplier = multiplier;
        slowTimer = duration;
    }

    private void Update()
    {
        if (isDead)
            return;

        if (path == null || path.Count == 0)
            return;

        float effectiveSpeed = currentSpeed * slowMultiplier;

        Vector3 targetPosition = path[pathIndex];
        Vector3 direction = targetPosition - transform.position;

        if (direction.magnitude < 0.05f)
        {
            if (pathIndex >= path.Count - 1)
            {
                ReachBase();
                return;
            }

            pathIndex++;
            return;
        }

        transform.position +=
            direction.normalized * effectiveSpeed * Time.deltaTime;

        if (slowTimer > 0f)
        {
            slowTimer -= Time.deltaTime;

            if (slowTimer <= 0f)
            {
                slowMultiplier = 1f;
            }
        }
    }

    private void ReachBase()
    {
        if (isDead)
            return;

        isDead = true;

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(1);
        }

        Destroy(gameObject);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;
        }

        Destroy(gameObject, fadeDuration);
    }
}