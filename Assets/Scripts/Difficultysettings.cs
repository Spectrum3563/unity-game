using UnityEngine;

public enum DifficultyLevel
{
    Easy = 0,
    Normal = 1,
    Hard = 2
}

/// <summary>
/// Holds the chosen difficulty. It is static, so it is not attached to any
/// object and it survives the scene reload when the player presses Retry.
/// </summary>
public static class DifficultySettings
{
    public static DifficultyLevel Current = DifficultyLevel.Normal;

    // Multiplies enemy health.
    public static float EnemyHealthMultiplier
    {
        get
        {
            switch (Current)
            {
                case DifficultyLevel.Easy: return 0.7f;
                case DifficultyLevel.Hard: return 1.4f;
                default: return 1f;
            }
        }
    }

    // Multiplies enemy movement speed.
    public static float EnemySpeedMultiplier
    {
        get
        {
            switch (Current)
            {
                case DifficultyLevel.Easy: return 0.85f;
                case DifficultyLevel.Hard: return 1.15f;
                default: return 1f;
            }
        }
    }

    // Multiplies the time between spawns (higher = fewer enemies).
    public static float SpawnIntervalMultiplier
    {
        get
        {
            switch (Current)
            {
                case DifficultyLevel.Easy: return 1.3f;
                case DifficultyLevel.Hard: return 0.75f;
                default: return 1f;
            }
        }
    }

    // Multiplies how fast enemies get stronger over time.
    public static float GrowthMultiplier
    {
        get
        {
            switch (Current)
            {
                case DifficultyLevel.Easy: return 0.6f;
                case DifficultyLevel.Hard: return 1.5f;
                default: return 1f;
            }
        }
    }
}