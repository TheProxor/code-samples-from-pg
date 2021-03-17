using Drawmasters.Levels;

namespace Drawmasters
{
    public static class LevelTargetTypeExtension
    {
        public static bool IsFriendly(this LevelTargetType type)
            => type == LevelTargetType.Hostage ||
               type == LevelTargetType.Shooter;

        public static bool IsEnemy(this LevelTargetType type)
            => type == LevelTargetType.Boss ||
               type == LevelTargetType.Enemy;

    }
}

