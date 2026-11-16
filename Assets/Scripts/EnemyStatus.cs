using UnityEngine;

public sealed class EnemyStatus : MonoBehaviour
{
    [Header("Health")]
    [SerializeField, Min(1f)]
    private float maxHealth = 30f;

    [SerializeField]
    private float currentHealth;

    public float MaxHealth => maxHealth;

    public float CurrentHealth => currentHealth;

    public bool IsDead => currentHealth <= 0f;

    public float HealthNormalized =>
        maxHealth > 0f
            ? currentHealth / maxHealth
            : 0f;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        currentHealth = maxHealth;
    }

    public float TakeDamage(float damage)
    {
        if (damage <= 0f || IsDead)
        {
            return 0f;
        }

        float previousHealth = currentHealth;

        currentHealth = Mathf.Max(
            0f,
            currentHealth - damage);

        return previousHealth - currentHealth;
    }

    public float Heal(float amount)
    {
        if (amount <= 0f || IsDead)
        {
            return 0f;
        }

        float previousHealth = currentHealth;

        currentHealth = Mathf.Min(
            maxHealth,
            currentHealth + amount);

        return currentHealth - previousHealth;
    }

    public void SetMaxHealth(
        float value,
        bool restoreHealth = false)
    {
        maxHealth = Mathf.Max(1f, value);

        if (restoreHealth)
        {
            currentHealth = maxHealth;
            return;
        }

        currentHealth = Mathf.Min(
            currentHealth,
            maxHealth);
    }

    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
    }
}