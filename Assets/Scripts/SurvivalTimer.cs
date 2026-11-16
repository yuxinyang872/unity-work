using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class SurvivalTimer : MonoBehaviour
{
    [Header("玩家參照")]
    [SerializeField, Tooltip("用來判斷死亡及停止計時的玩家單位。")]
    private PlayerUnit player;

    private float elapsedSeconds;
    private bool isRunning;

    public float ElapsedSeconds => elapsedSeconds;
    public bool IsRunning => isRunning;

    public event Action<float> TimerStopped;

    private void Start()
    {
        if (player == null && GameManager.Instance != null)
        {
            player = GameManager.Instance.GetPlayer();
        }

        if (player == null)
        {
            Debug.LogError("生存計時器初始化失敗，找不到玩家。", this);
            enabled = false;
            return;
        }

        player.Died += HandlePlayerDeath;
        elapsedSeconds = 0f;
        isRunning = !player.IsDead;

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            isRunning = false;
        }
    }

    private void Update()
    {
        if (!isRunning)
        {
            return;
        }

        if (player == null || player.IsDead ||
            (GameManager.Instance != null && GameManager.Instance.IsGameOver))
        {
            StopTimer();
            return;
        }

        elapsedSeconds += Time.deltaTime;
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.Died -= HandlePlayerDeath;
        }
    }

    private void HandlePlayerDeath(PlayerUnit deadPlayer)
    {
        StopTimer();
    }

    public void StopTimer()
    {
        if (!isRunning)
        {
            return;
        }

        isRunning = false;
        Debug.Log($"生存時間結束，總共存活 {elapsedSeconds:0.0} 秒。", this);
        TimerStopped?.Invoke(elapsedSeconds);
    }
}
