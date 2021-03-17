using UnityEngine;


namespace Drawmasters
{
    public interface IStartGame
    {
        void StartGame(GameMode mode, WeaponType weaponType, Transform levelTransform);
    }
}