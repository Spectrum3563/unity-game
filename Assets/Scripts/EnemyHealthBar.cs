using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;

    private EnemyController enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<EnemyController>();

        if (slider == null)
            slider = GetComponent<Slider>();
    }

    private void Update()
    {
        if (enemy == null)
            return;

        if (enemy.maxHealth <= 0f)
            return;

        slider.value = enemy.currentHealth / enemy.maxHealth;
    }
}