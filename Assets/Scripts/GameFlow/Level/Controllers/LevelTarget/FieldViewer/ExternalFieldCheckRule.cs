using System;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class ExternalFieldCheckRule : IFieldCheckRule
    {
        #region Fields

        private readonly Func<Vector3, bool> isOutOfZone;

        private readonly List<LevelTarget> trackingObjects = new List<LevelTarget>();

        #endregion



        #region IInitializable

        public void Initialize()
        {
            trackingObjects.Clear();
        }

        #endregion



        #region IDeinitializable

        public void Deinitialize()
        {
            trackingObjects.ForEach(i => i.OnGameFinished -= CheckingObject_OnGameFinished);
            trackingObjects.Clear();
        }

        #endregion



        #region IFieldCheckRule

        public bool IsMatching(LevelTarget checkingObject)
        {
            bool isMatching = default;

            bool isAlreadyEnteredZone = trackingObjects.Contains(checkingObject);
            if (isAlreadyEnteredZone)
            {
                isMatching = checkingObject.Ragdoll2D.IsActive;

                if (isMatching)
                {
                    isMatching &= isOutOfZone.Invoke(checkingObject.Ragdoll2D.EstimatedSkeletonPosition);

                    if (isMatching)
                    {
                        checkingObject.OnGameFinished -= CheckingObject_OnGameFinished;
                        trackingObjects.Remove(checkingObject);
                    }
                }
            }
            else
            {
                if (checkingObject.Ragdoll2D.IsActive)
                {
                    bool isEnteredZone = !isOutOfZone.Invoke(checkingObject.Ragdoll2D.EstimatedSkeletonPosition);

                    if (isEnteredZone)
                    {
                        checkingObject.OnGameFinished += CheckingObject_OnGameFinished;
                        trackingObjects.Add(checkingObject);
                    }
                }
            }

            return isMatching;
        }


        #endregion



        #region Ctor

        public ExternalFieldCheckRule(Func<Vector3, bool> _check)
        {
            isOutOfZone = _check;
        }

        #endregion



        #region Events handlers

        private void CheckingObject_OnGameFinished(LevelObject finishedObject)
        {
            if (finishedObject is LevelTarget enemy)
            {
                if (trackingObjects.Contains(enemy))
                {
                    enemy.OnGameFinished -= CheckingObject_OnGameFinished;

                    trackingObjects.Remove(enemy);
                }
            }                
        }

        #endregion
    }
}

