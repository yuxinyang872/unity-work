using UnityEngine;

public enum GamePhase
{
    Battle,
    Result
}

[DisallowMultipleComponent]
public sealed class GamePhaseController : MonoBehaviour
{
    [Header("階段依賴")]
    [SerializeField, Tooltip("負責接收遊戲結束事件的 GameManager。")]
    private GameManager gameManager;

    [SerializeField, Tooltip("前一節課建立的生存時間計時器。")]
    private SurvivalTimer survivalTimer;

    [Header("階段 UI Root")]
    [SerializeField, Tooltip("戰鬥階段顯示的既有 UI Root。")]
    private GameObject battleUIRoot;

    [SerializeField, Tooltip("結果階段顯示的既有 UI Root。")]
    private GameObject resultUIRoot;

    [SerializeField, Tooltip("負責將生存時間填入既有結果頁的組件。")]
    private ResultScreenPresenter resultPresenter;

    [Header("遊戲結束")]
    [SerializeField, Tooltip("進入結果階段後是否暫停遊戲時間。")]
    private bool pauseOnResult = true;

    private bool pausedByController;

    public GamePhase CurrentPhase { get; private set; } = GamePhase.Battle;

    private void Start()
    {
        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
        }

        if (!ValidateConfiguration())
        {
            enabled = false;
            return;
        }

        CurrentPhase = GamePhase.Battle;
        battleUIRoot.SetActive(true);
        resultUIRoot.SetActive(false);

        gameManager.GameOverStarted += HandleGameOver;

        if (gameManager.IsGameOver)
        {
            HandleGameOver();
        }
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.GameOverStarted -= HandleGameOver;
        }

        if (pausedByController)
        {
            Time.timeScale = 1f;
        }
    }

    private void HandleGameOver()
    {
        if (CurrentPhase == GamePhase.Result)
        {
            return;
        }

        survivalTimer.StopTimer();
        float finalSeconds = survivalTimer.ElapsedSeconds;

        CurrentPhase = GamePhase.Result;
        battleUIRoot.SetActive(false);
        resultUIRoot.SetActive(true);
        resultPresenter.ShowSurvivalTime(finalSeconds);

        if (pauseOnResult)
        {
            Time.timeScale = 0f;
            pausedByController = true;
        }

        Debug.Log($"已進入結果階段，生存時間：{finalSeconds:0.0} 秒。", this);
    }

    private bool ValidateConfiguration()
    {
        if (gameManager == null || survivalTimer == null ||
            battleUIRoot == null || resultUIRoot == null ||
            resultPresenter == null)
        {
            Debug.LogError("階段控制器缺少必要的 Inspector 參照。", this);
            return false;
        }

        if (battleUIRoot == resultUIRoot ||
            resultUIRoot.transform.IsChildOf(battleUIRoot.transform) ||
            battleUIRoot.transform.IsChildOf(resultUIRoot.transform) ||
            transform.IsChildOf(battleUIRoot.transform) ||
            transform.IsChildOf(resultUIRoot.transform))
        {
            Debug.LogError("兩個 UI Root 必須彼此獨立，階段控制器不能位於任一 Root 之下。", this);
            return false;
        }

        return true;
    }
}
