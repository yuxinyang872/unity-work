using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyStatus))]
public sealed class EnemyUnit : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private EnemyStatus status;

    private bool deathNotified;

    public event Action<EnemyUnit> Died;

    public Transform Transform => transform;

    public float CurrentHealth => status.CurrentHealth;

    public float MaxHealth => status.MaxHealth;

    public float HealthNormalized => status.HealthNormalized;

    public bool IsDead => status.IsDead;

    private void Awake()
    {
        ResolveDependencies();
    }

    public float TakeDamage(float damage)
    {
        if (status == null || IsDead)
        {
            return 0f;
        }

        float actualDamage = status.TakeDamage(damage);

        TryNotifyDeath();

        return actualDamage;
    }

    public float Heal(float amount)
    {
        if (status == null)
        {
            return 0f;
        }

        return status.Heal(amount);
    }

    public void SetMaxHealth(
        float value,
        bool restoreHealth = false)
    {
        if (status == null)
        {
            return;
        }

        status.SetMaxHealth(
            value,
            restoreHealth);

        if (restoreHealth && !status.IsDead)
        {
            deathNotified = false;
        }
    }

    public void RestoreFullHealth()
    {
        if (status == null)
        {
            return;
        }

        status.RestoreFullHealth();
        deathNotified = false;
    }

    private void TryNotifyDeath()
    {
        if (!status.IsDead || deathNotified)
        {
            return;
        }

        deathNotified = true;

        Debug.Log(
            $"{gameObject.name} 已進入死亡狀態。",
            this);

        Died?.Invoke(this);
    }

    private void ResolveDependencies()
    {
        if (status == null)
        {
            status = GetComponent<EnemyStatus>();
        }

        if (status != null)
        {
            return;
        }

        Debug.LogError(
            "EnemyUnit 初始化失敗，找不到 EnemyStatus。",
            this);

        enabled = false;
    }
}