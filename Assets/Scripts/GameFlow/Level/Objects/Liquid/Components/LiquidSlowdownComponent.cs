using System.Collections.Generic;
using UnityEngine;
using Drawmasters.Utils;


namespace Drawmasters.Levels
{
    public class LiquidSlowdownComponent : LiquidComponent
    {
        #region Fields

        // callbacks on disable dont work correct on unity 2018.x
        // https://issuetracker.unity3d.com/issues/ontriggerexit2d-is-not-happening-when-other-colliding-object-is-beeing-destroyed?_ga=2.255655496.1114288414.1574659337-1498481534.1569474615
        // https://forum.unity.com/threads/physics2dsettings-callbacks-on-disable-doesnt-work-in-2018-2-3f1.545618/
        private Dictionary<Rigidbody2D, CollidableObjectType> slowedRigidbody;
        private LiquidSettings settings;

        #endregion



        #region Methods

        public override void Enable()
        {
            slowedRigidbody = new Dictionary<Rigidbody2D, CollidableObjectType>();

            liquid.CollisionNotifier.OnCustomTriggerEnter2D += CollisionNotifier_OnCustomTriggerEnter2D;
            liquid.CollisionNotifier.OnCustomTriggerExit2D += CollisionNotifier_OnCustomTriggerExit2D;

            settings = IngameData.Settings.liquidSettings;
        }


        public override void Disable()
        {
            liquid.CollisionNotifier.OnCustomTriggerEnter2D -= CollisionNotifier_OnCustomTriggerEnter2D;
            liquid.CollisionNotifier.OnCustomTriggerExit2D -= CollisionNotifier_OnCustomTriggerExit2D;

            foreach (var rb in slowedRigidbody)
            {
                Rigidbody2D savedRb = rb.Key;

                if (savedRb != null)
                {
                    savedRb.drag -= settings.FindLinearDrag(rb.Value);
                    savedRb.angularDrag -= settings.FindAngularDrag(rb.Value);
                }
            }

            slowedRigidbody.Clear();
        }


        private bool TryGetCollidedRigidbody(Collider2D collision, out Rigidbody2D foundRigidbody)
        {
            foundRigidbody = CollisionUtility.FindLevelObjectRigidbody(collision);

            return foundRigidbody != null;
        }

        #endregion



        #region Events handlers

        private void CollisionNotifier_OnCustomTriggerEnter2D(GameObject go, Collider2D collision)
        {
            CollidableObject collidableObject = collision.GetComponent<CollidableObject>();

            if (collidableObject == null)
            {
                return;
            }

            if (TryGetCollidedRigidbody(collision, out Rigidbody2D foundRigidbody) &&
                !slowedRigidbody.ContainsKey(foundRigidbody))
            {
                foundRigidbody.drag += settings.FindLinearDrag(collidableObject.Type);
                foundRigidbody.angularDrag += settings.FindAngularDrag(collidableObject.Type);

                slowedRigidbody.Add(foundRigidbody, collidableObject.Type);
            }
        }


        private void CollisionNotifier_OnCustomTriggerExit2D(GameObject go, Collider2D collision)
        {
            CollidableObject collidableObject = collision.GetComponent<CollidableObject>();

            if (collidableObject == null)
            {
                return;
            }

            if (TryGetCollidedRigidbody(collision, out Rigidbody2D foundRigidbody) &&
                slowedRigidbody.ContainsKey(foundRigidbody))
            {
                foundRigidbody.drag -= settings.FindLinearDrag(collidableObject.Type);
                foundRigidbody.angularDrag -= settings.FindAngularDrag(collidableObject.Type);

                slowedRigidbody.Remove(foundRigidbody);
            }
        }

        #endregion
    }
}
