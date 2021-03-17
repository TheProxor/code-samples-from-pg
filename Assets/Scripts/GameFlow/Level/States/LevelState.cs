namespace Drawmasters.Levels
{
    public enum LevelState
    {
        None                    = 0,
        Initialized             = 1,
        Playing                 = 2,
        AllTargetsHitted        = 3,
        OutOfAmmo               = 4,
        EndPlaying              = 5,
        Tutorial                = 6,
        FriendlyDeath           = 7,
        StageChanging           = 8,
        Paused                  = 9,
        FinishDrawing           = 11,
        ReturnToInitial         = 12,
        Unloaded                = 13,
        Finished                = 14
    }
}
