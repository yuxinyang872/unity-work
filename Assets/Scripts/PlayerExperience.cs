using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerExperience : MonoBehaviour
{
    private const int GrowthNumerator = 11;
    private const int GrowthDenominator = 10;

    [Header("經驗值設定")]
    [SerializeField, Min(1)]
    [Tooltip("Lv.1 升到 Lv.2 所需要的經驗值。")]
    private int firstLevelRequirement = 5;

    public int Level { get; private set; } = 1;
    public int CurrentExperience { get; private set; }
    public int RequiredExperience { get; private set; }

    public float ExperienceNormalized =>
        RequiredExperience > 0
            ? (float)CurrentExperience / RequiredExperience
            : 0f;

    private void Awake()
    {
        RequiredExperience = Mathf.Max(1, firstLevelRequirement);
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        long accumulatedExperience = (long)CurrentExperience + amount;

        while (accumulatedExperience >= RequiredExperience && Level < int.MaxValue)
        {
            accumulatedExperience -= RequiredExperience;
            Level++;
            RequiredExperience = CalculateNextRequirement(RequiredExperience);

            Debug.Log(
                $"玩家升級至 Lv.{Level}，下一級需要 {RequiredExperience} EXP。",
                this);
        }

        CurrentExperience = (int)Math.Min(accumulatedExperience, int.MaxValue);
    }

    private static int CalculateNextRequirement(int previousRequirement)
    {
        long nextRequirement =
            ((long)previousRequirement * GrowthNumerator + GrowthDenominator - 1)
            / GrowthDenominator;

        return (int)Math.Min(nextRequirement, int.MaxValue);
    }
}
