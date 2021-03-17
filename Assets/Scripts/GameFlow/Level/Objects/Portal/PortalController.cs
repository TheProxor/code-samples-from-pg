using Modules.Sound;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Drawmasters.Levels
{
    public class PortalController : IDeinitializable, IInitializable
    {
        #region Fields

        public static event Action<LaserLevelObject, Vector3, Vector2> OnAllowTeleportRay; // position direction

        public static event Action<PortalObject.Type> OnTypeChanged;

        private readonly List<PortalObject> portals = new List<PortalObject>();

        public static PortalObject.Type createType; // fast static

        private readonly List<(ITeleportable, PortalObject)> readyToTeleport = new List<(ITeleportable, PortalObject)>();

        private readonly PortalObject.Type firstType;
        private readonly PortalObject.Type secondType;

        private Guid portalIdleSoundGuid;

        #endregion



        #region Ctor

        public PortalController()
        {
            firstType = PortalObject.Type.First;
            secondType = PortalObject.Type.Second;

            createType = firstType;
        }

        #endregion



        #region Interface

        public void Initialize()
        {
            LaserVisualSpreadComponent.OnShouldTeleportLaser += LaserVisualSpreadComponent_OnShouldTeleportLaser;
        }


        public void Deinitialize()
        {
            LaserVisualSpreadComponent.OnShouldTeleportLaser -= LaserVisualSpreadComponent_OnShouldTeleportLaser;

            for (int i = portals.Count - 1; i >= 0; i--)
            {
                RemovePortal(portals[i]);
            }
        }

        #endregion



        #region Methods

        public void CreateNewPortal(Vector3 portalPosition, Vector3 leftBorderPosition, Vector3 rightBorderPosition)
        {
            PortalObject removable = portals.Find(p => p.CurrentType == createType);

            if (removable != null)
            {
                removable.Hide(() => RemovePortal(removable));
            }

            float angle = Mathf.Atan2(leftBorderPosition.y - rightBorderPosition.y, leftBorderPosition.x - rightBorderPosition.x) * 180 / Mathf.PI;
            angle += 180.0f;

            PortalObject portal = Content.Management.CreatePortalObject(null, portalPosition, angle, createType);

            float bordersDistance = Vector3.Distance(leftBorderPosition, rightBorderPosition);

            if (portal.Width > bordersDistance && portal.Width > 0.0f)
            {
                Vector3 scale = portal.transform.localScale * bordersDistance / portal.Width;
                portal.transform.localScale = scale;
            }

            float leftBorderDistance = Vector3.Distance(portalPosition, leftBorderPosition);
            float rightBorderDistance = Vector3.Distance(portalPosition, rightBorderPosition);

            if (leftBorderDistance < portal.Width * 0.5f)
            {
                float offsetDelta = portal.Width * 0.5f - leftBorderDistance;
                MovePortal(offsetDelta, -1.0f);
            }
            else if (rightBorderDistance < portal.Width * 0.5f)
            {
                float offsetDelta = portal.Width * 0.5f - rightBorderDistance;
                MovePortal(offsetDelta, 1.0f);
            }

            ChangeCreatedType();

            portal.Show();
            portal.OnPortalEnter += Portal_OnPortalEnter;
            portal.OnPortalExit += Portal_OnPortalExit;

            portals.Add(portal);

            foreach (var readyToTeleportItems in readyToTeleport)
            {
                TryTeleportObject(readyToTeleportItems.Item2, readyToTeleportItems.Item1);
            }

            readyToTeleport.Clear(); // wrong, cuz need remove only if teleported!!!

            if (!portals.IsNullOrEmpty() && !SoundManager.Instance.IsActive(portalIdleSoundGuid))
            {
                portalIdleSoundGuid = SoundManager.Instance.PlaySound(AudioKeys.Ingame.WEAPON_PORTALGUN_IDLE, isLooping: true);
            }

            void MovePortal(float offsetDelta, float rightSideMultiplier)
            {
                Vector3 newPosition = portal.transform.position - portal.transform.right * rightSideMultiplier * offsetDelta;
                portal.transform.position = newPosition;
            }
        }


        public static Vector2 CalculateEndObjectPosition(PortalObject enteredPortal, PortalObject exitPortal, Vector3 pointEnterPosition, float portalExitoffset)
        {
            Vector2 endPosition = (exitPortal.transform.position + portalExitoffset * exitPortal.transform.up).ToVector2();

            return endPosition;
        }


        public static Vector2 CalculateEndDirection(PortalObject enteredPortal, PortalObject exitPortal, Vector3 enteredDirection)
        {
            return exitPortal.transform.up;
        }


        private void RemovePortal(PortalObject portal)
        {
            portals.Remove(portal);

            portal.OnPortalEnter -= Portal_OnPortalEnter;
            portal.OnPortalExit -= Portal_OnPortalExit;
            portal.Deinitialize();

            readyToTeleport.RemoveAll((e) => e.Item2 == portal);
            Content.Management.DestroyObject(portal.gameObject);

            if (portals.IsNullOrEmpty())
            {
                SoundManager.Instance.StopSound(portalIdleSoundGuid);
            }
        }


        private void ChangeCreatedType()
        {
            createType = createType == firstType ? secondType : firstType;

            OnTypeChanged?.Invoke(createType);
        }


        private bool TryTeleportObject(PortalObject enteredPortal, ITeleportable teleportable)
        {
            bool result = false;

            PortalObject otherPortal = portals.Find(p => p.CurrentType != enteredPortal.CurrentType && p.CurrentType != PortalObject.Type.None);

            if (otherPortal != null)
            {
                result = teleportable.TryTeleport(enteredPortal, otherPortal);
            }

            if (result)
            {
                foreach (var portal in portals)
                {
                    portal.PlayTeleportedEffect();
                }
            }

            return result;
        }

        #endregion



        #region Events handlers

        private void Portal_OnPortalEnter(PortalObject enteredPortal, ITeleportable teleportable)
        {
            bool wasTeleported = TryTeleportObject(enteredPortal, teleportable);

            if (!wasTeleported)
            {
                readyToTeleport.Add((teleportable, enteredPortal));
            }
        }


        private void Portal_OnPortalExit(PortalObject enteredPortal, ITeleportable teleportable)
        {
            var itemToRemove = readyToTeleport.Find(e => e.Item1 == teleportable);
            readyToTeleport.Remove(itemToRemove);
        }


        private void LaserVisualSpreadComponent_OnShouldTeleportLaser(PortalObject enteredPortal, Vector2 enteredDirection, Vector3 hitPosition, LaserLevelObject laser)
        {
            PortalObject exitPortal = portals.Find(p => p.CurrentType != enteredPortal.CurrentType && p.CurrentType != PortalObject.Type.None);

            if (exitPortal != null)
            {
                float offset = IngameData.Settings.portalsSettings.laserPortalExitOffset;
                Vector2 endPosition = CalculateEndObjectPosition(enteredPortal, exitPortal, hitPosition, offset);

                Vector2 direction = CalculateEndDirection(enteredPortal, exitPortal, enteredDirection);
                OnAllowTeleportRay?.Invoke(laser, endPosition, direction.normalized);
            }
        }

        #endregion
    }
}
