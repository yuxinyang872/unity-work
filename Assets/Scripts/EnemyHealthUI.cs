using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class EnemyHealthUI : MonoBehaviour
{
    [Header("敵人")]
    [SerializeField, Tooltip("所屬的 EnemyUnit。")]
    private EnemyUnit enemyUnit;

    [Header("生命值 UI")]
    [SerializeField, Tooltip("敵人血量 Slider。")]
    private Slider healthSlider;

    [Header("攝影機")]
    [SerializeField, Tooltip("朝向的攝影機，可留空使用 Main Camera。")]
    private Camera targetCamera;

    [SerializeField, Tooltip("使世界空間的血條朝向攝影機。")]
    private bool faceCamera = true;

    private void Awake()
    {
        if (enemyUnit == null)
        {
            enemyUnit = GetComponentInParent<EnemyUnit>();
        }

        if (healthSlider == null)
        {
            healthSlider = GetComponentInChildren<Slider>(true);
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (enemyUnit == null || healthSlider == null)
        {
            Debug.LogError("EnemyHealthUI 初始化失敗，請指定 EnemyUnit 與 Slider。", this);
            enabled = false;
            return;
        }

        healthSlider.minValue = 0f;
        healthSlider.maxValue = 1f;
        healthSlider.wholeNumbers = false;
        healthSlider.interactable = false;
    }

    private void OnEnable()
    {
        RefreshHealth();
    }

    private void LateUpdate()
    {
        RefreshHealth();

        if (!faceCamera)
        {
            return;
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera != null)
        {
            transform.rotation = targetCamera.transform.rotation;
        }
    }

    private void RefreshHealth()
    {
        if (enemyUnit == null || healthSlider == null)
        {
            return;
        }

        healthSlider.value = enemyUnit.HealthNormalized;
    }
}
