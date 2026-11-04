using UnityEngine;

public class Projectile : MonoBehaviour
{
    private EnemyController target;
    private float damage;
    private float speed;
    private GameObject hitEffectPrefab;
    private float hitEffectLifetime;
    private Vector3 lastTargetPosition;

    public void Init(
        EnemyController target,
        float damage,
        float speed,
        GameObject hitEffectPrefab,
        float hitEffectLifetime)
    {
        this.target = target;
        this.damage = damage;
        this.speed = speed;
        this.hitEffectPrefab = hitEffectPrefab;
        this.hitEffectLifetime = hitEffectLifetime;

        lastTargetPosition = target.transform.position;
    }

    private void Update()
    {
        // Follow the target while it is alive; otherwise fly to where it was.
        if (target != null && !target.isDead)
            lastTargetPosition = target.transform.position;

        Vector3 direction = lastTargetPosition - transform.position;
        float step = speed * Time.deltaTime;

        if (direction.magnitude <= step)
        {
            Hit();
            return;
        }

        transform.position += direction.normalized * step;

        // Rotate to face the direction of travel (assumes the sprite points right).
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Hit()
    {
        if (target != null && !target.isDead)
        {
            target.TakeDamage(damage);

            if (hitEffectPrefab != null)
            {
                GameObject fx = Instantiate(
                    hitEffectPrefab,
                    lastTargetPosition,
                    Quaternion.identity
                );
                Destroy(fx, hitEffectLifetime);
            }
        }

        Destroy(gameObject);
    }
}