using UnityEngine;

[DisallowMultipleComponent]
public sealed class DeadModule : MonoBehaviour
{
    private PlayerUnit playerUnit;
    private EnemyUnit enemyUnit;
    private bool isTriggered;

    private void Awake()
    {
        ResolveUnit();
    }

    private void OnEnable()
    {
        // 物件池重複啟用同一隻敵人時，重設本次死亡狀態。
        isTriggered = false;
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void TriggerDeath()
    {
        if (isTriggered)
        {
            return;
        }

        isTriggered = true;

        EnemyPoolMember poolMember = GetComponent<EnemyPoolMember>();

        if (poolMember != null && poolMember.Owner != null)
        {
            Debug.Log($"{gameObject.name} 觸發死亡，返回敵人物件池。", this);
            poolMember.ReturnToPool();
            return;
        }

        Debug.Log($"{gameObject.name} 觸發死亡，將刪除物件。", this);
        Destroy(gameObject);
    }

    private void ResolveUnit()
    {
        playerUnit = GetComponent<PlayerUnit>();
        enemyUnit = GetComponent<EnemyUnit>();

        if (playerUnit != null || enemyUnit != null)
        {
            return;
        }

        Debug.LogError(
            "DeadModule 初始化失敗，物件上不存在 PlayerUnit 或 EnemyUnit。",
            this);

        enabled = false;
    }

    private void Subscribe()
    {
        if (playerUnit != null)
        {
            playerUnit.Died += HandlePlayerDeath;
        }

        if (enemyUnit != null)
        {
            enemyUnit.Died += HandleEnemyDeath;
        }
    }

    private void Unsubscribe()
    {
        if (playerUnit != null)
        {
            playerUnit.Died -= HandlePlayerDeath;
        }

        if (enemyUnit != null)
        {
            enemyUnit.Died -= HandleEnemyDeath;
        }
    }

    private void HandlePlayerDeath(PlayerUnit unit)
    {
        TriggerDeath();
    }

    private void HandleEnemyDeath(EnemyUnit unit)
    {
        TriggerDeath();
    }
}