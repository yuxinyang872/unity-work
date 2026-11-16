using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyUnit))]
[RequireComponent(typeof(EnemyStatus))]
[RequireComponent(typeof(EnemyAttackModule))]
[RequireComponent(typeof(EnemyMovement))]
public sealed class EnemyGrowthModule : MonoBehaviour
{
    [Header("敵人強化增量")]
    [SerializeField, Min(0f)]
    [Tooltip("每次生命強化時增加的最大生命值。")]
    private float healthDelta = 5f;

    [SerializeField, Min(0f)]
    [Tooltip("每次傷害強化時增加的碰撞傷害。")]
    private float damageDelta = 1f;

    [SerializeField, Min(0f)]
    [Tooltip("每次速度強化時增加的移動速度。")]
    private float moveSpeedDelta = 0.2f;

    private EnemyUnit enemyUnit;
    private EnemyStatus enemyStatus;
    private EnemyAttackModule attackModule;
    private EnemyMovement movementModule;
    private PlayerUnit player;

    private float baseMaxHealth;
    private float baseDamage;
    private float baseMoveSpeed;
    private int appliedLevel = -1;

    private void Awake()
    {
        enemyUnit = GetComponent<EnemyUnit>();
        enemyStatus = GetComponent<EnemyStatus>();
        attackModule = GetComponent<EnemyAttackModule>();
        movementModule = GetComponent<EnemyMovement>();

        baseMaxHealth = enemyStatus.MaxHealth;
        baseDamage = attackModule.ContactDamage;
        baseMoveSpeed = movementModule.MoveSpeed;
    }

    private void OnEnable()
    {
        GameManager manager = GameManager.Instance;
        player = manager != null ? manager.GetPlayer() : null;

        if (player == null)
        {
            Debug.LogError("敵人成長模組無法取得等級來源。", this);
            enabled = false;
            return;
        }

        ApplyLevel(player.Level);
    }

    private void Update()
    {
        if (player == null || player.Level == appliedLevel)
        {
            return;
        }

        ApplyLevel(player.Level);
    }

    private void OnDisable()
    {
        player = null;
        appliedLevel = -1;
    }

    private void ApplyLevel(int level)
    {
        int upgrades = Mathf.Max(0, level - 1);
        int cycles = upgrades / 3;
        int remainder = upgrades % 3;

        int healthCount = cycles + (remainder >= 1 ? 1 : 0);
        int damageCount = cycles + (remainder >= 2 ? 1 : 0);
        int speedCount = cycles;

        float newHealth = baseMaxHealth + healthCount * healthDelta;
        float newDamage = baseDamage + damageCount * damageDelta;
        float newSpeed = baseMoveSpeed + speedCount * moveSpeedDelta;

        enemyUnit.SetMaxHealth(newHealth, false);
        attackModule.SetContactDamage(newDamage);
        movementModule.SetMoveSpeed(newSpeed);
        appliedLevel = level;
    }
}
