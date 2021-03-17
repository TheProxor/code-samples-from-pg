using System;

namespace Drawmasters.Proposal
{
    [Flags]
    public enum ChestRewardType
    {
        None           =    0,
        ShooterSkin    =    1 << 1,
        WeaponSKin     =    1 << 2,
        PetSkin        =    1 << 3,
        SoftCurrency   =    1 << 4,
        HardCurrency   =    1 << 5,
        MansionHammers =    1 << 6,
        BonesCurrency  =    1 << 7
    }
}