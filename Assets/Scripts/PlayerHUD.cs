using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PlayerHUD : MonoBehaviour
{
    [Header("玩家")]
    [SerializeField, Tooltip("要顯示資訊的玩家單位。")]
    private PlayerUnit player;

    [Header("生命值 UI")]
    [SerializeField, Tooltip("玩家生命值 Slider。")]
    private Slider healthSlider;

    [SerializeField, Tooltip("玩家生命值文字，可留空。")]
    private Text healthText;

    [Header("經驗值 UI")]
    [SerializeField, Tooltip("玩家經驗值 Slider。")]
    private Slider experienceSlider;

    [SerializeField, Tooltip("玩家經驗值文字，可留空。")]
    private Text experienceText;

    [SerializeField, Tooltip("玩家等級文字，可留空。")]
    private Text levelText;

    private float previousHealth = -1f;
    private float previousMaxHealth = -1f;
    private int previousExperience = -1;
    private int previousRequirement = -1;
    private int previousLevel = -1;

    private void Start()
    {
        if (player == null && GameManager.Instance != null)
        {
            player = GameManager.Instance.GetPlayer();
        }

        if (player == null || healthSlider == null || experienceSlider == null)
        {
            Debug.LogError("PlayerHUD 初始化失敗，請指定玩家與 HP、EXP Slider。", this);
            enabled = false;
            return;
        }

        ConfigureSlider(healthSlider);
        ConfigureSlider(experienceSlider);
        RefreshUI();
    }

    private void Update()
    {
        RefreshUI();
    }

    private static void ConfigureSlider(Slider slider)
    {
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.interactable = false;
    }

    private void RefreshUI()
    {
        if (player == null)
        {
            return;
        }

        healthSlider.value = player.HealthNormalized;
        experienceSlider.value = player.ExperienceNormalized;

        if (!Mathf.Approximately(previousHealth, player.CurrentHealth) ||
            !Mathf.Approximately(previousMaxHealth, player.MaxHealth))
        {
            if (healthText != null)
            {
                healthText.text = $"HP {player.CurrentHealth:0.#} / {player.MaxHealth:0.#}";
            }

            previousHealth = player.CurrentHealth;
            previousMaxHealth = player.MaxHealth;
        }

        if (previousExperience != player.CurrentExperience ||
            previousRequirement != player.RequiredExperience)
        {
            if (experienceText != null)
            {
                experienceText.text =
                    $"EXP {player.CurrentExperience} / {player.RequiredExperience}";
            }

            previousExperience = player.CurrentExperience;
            previousRequirement = player.RequiredExperience;
        }

        if (previousLevel != player.Level)
        {
            if (levelText != null)
            {
                levelText.text = $"Lv.{player.Level}";
            }

            previousLevel = player.Level;
        }
    }
}
