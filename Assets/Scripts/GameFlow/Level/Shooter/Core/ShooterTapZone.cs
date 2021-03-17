using UnityEngine;


namespace Drawmasters.Levels
{
    public class ShooterTapZone : MonoBehaviour
    {
        [SerializeField] private Shooter shooter = default;

        public Shooter Shooter => shooter;
    }
}
