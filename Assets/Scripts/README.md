# Unity 6 3D | Game Over and Result UI phase

## Scope

This lesson uses the existing BattleUIRoot and ResultUIRoot. It transitions to the Result phase when PlayerUnit.Died occurs, stops SurvivalTimer, and writes the survival time into the existing TextTime_Value. It does not implement Kill Count, Score, restart, or a new result UI layout.

## Files

- GameManager.cs: complete replacement of uploaded GameManager(6).cs; subscribes to PlayerUnit.Died and exposes GameOverStarted. Existing enemy and pool APIs remain intact.
- SurvivalTimer.cs: complete replacement of previous SurvivalTimer lesson source; only public StopTimer() is added for synchronous final-time capture.
- GamePhaseController.cs: new scene-level controller with explicit Battle and Result phases.
- ResultScreenPresenter.cs: new presenter attached to existing ResultUIRoot; updates TextTime_Value.
- CameraFollow.cs: complete replacement of existing CameraFollow(3).cs with a null-target guard after DeadModule destroys the player.

## Hierarchy (reuse actual objects)

Scene
  GameManager [GameManager, GamePhaseController]
  SurvivalTimer [SurvivalTimer]
  BattleUIRoot (existing)
  ResultUIRoot (existing) [ResultScreenPresenter]
    Img_Bg
      Img_ResultBoardBg
        Img_ResultArea
          Img_KillCountArea
          TextGroup_Time
            TextTime_Title
            TextTime_Value  [UnityEngine.UI.Text]
          TextGroup_Score

Assign GamePhaseController: GameManager, SurvivalTimer, BattleUIRoot, ResultUIRoot, ResultScreenPresenter.
Assign ResultScreenPresenter: TextTime_Value's Text component.

The lesson follows the existing PlayerHUD which uses UnityEngine.UI.Text. If TextTime_Value actually uses TextMeshProUGUI, in ResultScreenPresenter change `using UnityEngine.UI;` to `using TMPro;` and the field type from `Text` to `TMP_Text` (do not add duplicate UI texts).

## Timeline

1. Battle: BattleUIRoot active, ResultUIRoot inactive, SurvivalTimer running.
2. Player health drops to zero; PlayerUnit.Died is fired.
3. GameManager.HandlePlayerDeath sets IsGameOver and emits GameOverStarted exactly once.
4. GamePhaseController calls SurvivalTimer.StopTimer() and snapshots ElapsedSeconds regardless of death event subscriber order.
5. GamePhaseController deactivates BattleUIRoot, activates ResultUIRoot, and updates TextTime_Value.
6. Optionally Time.timeScale = 0 to freeze gameplay; on GamePhaseController destruction, it returns to 1.

## Validation

- Initial state: BattleUIRoot true; ResultUIRoot false.
- Player at 65 seconds, dies: IsGameOver true, CurrentPhase Result, time text 01:05.
- Enemy death/recycle does not switch phases.
- Multiple damage/death callbacks do not re-enter results.
- UI roots are scene siblings, not parents of GameManager or SurvivalTimer.
- If GamePhaseController is attached to GameManager, do not put GameManager under BattleUIRoot.
- If enabling Pause On Result, physics no longer progresses, but a UI animation using scaled time will also pause; disable this toggle if your result animation requires scaled time.
- Verify that player's DeadModule destroys the player and CameraFollow guard prevents MissingReferenceException.

## Note

The snippets and files were reviewed against the uploaded sources, but Unity Editor compile and Play Mode have not been run in this environment.
