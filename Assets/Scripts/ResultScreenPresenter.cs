using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class ResultScreenPresenter : MonoBehaviour
{
    [Header("既有結果頁文字")]
    [SerializeField, Tooltip("指定 ResultUIRoot 下的 TextTime_Value。")]
    private Text textTimeValue;

    public void ShowSurvivalTime(float elapsedSeconds)
    {
        if (textTimeValue == null)
        {
            Debug.LogError("結果頁尚未指定 TextTime_Value 文字元件。", this);
            return;
        }

        int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(elapsedSeconds));
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        textTimeValue.text = $"{minutes:00}:{seconds:00}";
    }
}
