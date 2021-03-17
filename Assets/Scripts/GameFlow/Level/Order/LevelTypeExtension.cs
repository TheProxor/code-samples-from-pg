namespace Drawmasters
{
    public static class LevelTypeExtension
    {
        public static bool IsCommonLevel(this LevelType type)
            => type == LevelType.Simple;
    }
}

