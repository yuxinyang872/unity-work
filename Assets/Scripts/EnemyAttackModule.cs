using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public sealed class EnemyAttackModule : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField, Min(0f)]
    private float contactDamage = 10f;

    [SerializeField, Min(0.01f)]
    private float attackInterval = 1f;

    private float nextAttackTime;

    public float ContactDamage => contactDamage;

    public void SetContactDamage(float value)
    {
        contactDamage = Mathf.Max(0f, value);
    }

    private void OnEnable()
    {
        nextAttackTime = 0f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryAttack(collision.collider);
    }

    private void OnCollisionStay(Collision collision)
    {
        TryAttack(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryAttack(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryAttack(other);
    }

    private void TryAttack(Collider targetCollider)
    {
        if (targetCollider == null)
        {
            return;
        }

        PlayerUnit playerUnit =
            targetCollider.GetComponentInParent<PlayerUnit>();

        if (playerUnit == null)
        {
            return;
        }

        if (playerUnit.IsDead)
        {
            return;
        }

        if (Time.time < nextAttackTime)
        {
            return;
        }

        float actualDamage =
            playerUnit.TakeDamage(contactDamage);

        if (actualDamage <= 0f)
        {
            Debug.LogWarning(
                $"敵人碰觸玩家，但沒有造成傷害。玩家目前生命：{playerUnit.CurrentHealth}/{playerUnit.MaxHealth}",
                this);

            return;
        }

        nextAttackTime =
            Time.time + attackInterval;

        Debug.Log(
            $"敵人對玩家造成 {actualDamage} 點傷害，玩家剩餘生命：{playerUnit.CurrentHealth}/{playerUnit.MaxHealth}",
            this);
    }
}