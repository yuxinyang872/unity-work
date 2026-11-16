using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerStatus))]
[RequireComponent(typeof(PlayerExperience))]
public sealed class PlayerUnit : MonoBehaviour
{
    [Header("組件")]
    [SerializeField, Tooltip("玩家的生命值計算組件。")]
    private PlayerStatus status;

    [SerializeField, Tooltip("玩家的經驗值與等級計算組件。")]
    private PlayerExperience experience;

    private bool deathNotified;

    public event Action<PlayerUnit> Died;

    public Transform Transform => transform;
    public float CurrentHealth => status.CurrentHealth;
    public float MaxHealth => status.MaxHealth;
    public float HealthNormalized => status.HealthNormalized;
    public bool IsDead => status.IsDead;

    public int Level => experience != null ? experience.Level : 1;
    public int CurrentExperience =>
        experience != null ? experience.CurrentExperience : 0;
    public int RequiredExperience =>
        experience != null ? experience.RequiredExperience : 1;
    public float ExperienceNormalized =>
        experience != null ? experience.ExperienceNormalized : 0f;

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

    public void SetMaxHealth(float value, bool restoreHealth = false)
    {
        if (status == null)
        {
            return;
        }

        status.SetMaxHealth(value, restoreHealth);

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

    public void AddExperience(int amount)
    {
        if (experience == null || IsDead || amount <= 0)
        {
            return;
        }

        experience.AddExperience(amount);
    }

    private void TryNotifyDeath()
    {
        if (!status.IsDead || deathNotified)
        {
            return;
        }

        deathNotified = true;
        Debug.Log("玩家已進入死亡狀態。", this);
        Died?.Invoke(this);
    }

    private void ResolveDependencies()
    {
        if (status == null)
        {
            status = GetComponent<PlayerStatus>();
        }

        if (experience == null)
        {
            experience = GetComponent<PlayerExperience>();
        }

        if (status == null || experience == null)
        {
            Debug.LogError("PlayerUnit 初始化失敗，缺少 PlayerStatus 或 PlayerExperience。", this);
            enabled = false;
        }
    }
}
