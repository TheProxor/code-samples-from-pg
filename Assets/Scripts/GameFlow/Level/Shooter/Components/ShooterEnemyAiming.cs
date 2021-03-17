using System;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class ShooterEnemyAiming : ShooterComponent
    {
        #region Fields

        public static event Action<LevelTarget> OnAimAtEnemy;

        #endregion



        #region Methods

        public override void Deinitialize()
        {
            shooter.OnAiming -= Shooter_OnAiming;
        }
                

        public override void StartGame()
        {
            shooter.OnAiming += Shooter_OnAiming;
        }


        private void FindEnemy(Vector3 from, Vector2 direction)
        {
            RaycastHit2D hit = Physics2D.Raycast(from, direction, float.MaxValue, ~0);

            #if UNITY_EDITOR
                Debug.DrawLine(from, hit.point, Color.red);
                Debug.DrawRay(from, direction, Color.green);
            #endif

            if (hit.collider == null)
            {
                OnAimAtEnemy?.Invoke(null);

                return;
            }
            
            CollidableObject collidable = hit.collider.GetComponent<CollidableObject>();

            if (collidable == null ||
                !collidable.HasValue ||
                collidable.Type != CollidableObjectType.EnemyTrigger)
            {
                OnAimAtEnemy?.Invoke(null);
            }
            else
            {
                LevelTarget enemy = collidable.LevelTarget;

                OnAimAtEnemy?.Invoke(enemy);
            }
        }

        #endregion



        #region Events handlers

        private void Shooter_OnAiming(Vector3 start, Vector2 direction)
        {
            FindEnemy(start, direction);
        }

        #endregion
    }
}
