using System.Collections.Generic;
using Drawmasters.Effects.Interfaces;
using Drawmasters.Pool.Interfaces;


namespace Drawmasters.Effects
{
    public class EffectAliveHandler : IEffectAliveHandler
    {
        #region Helpers

        public class Data
        {
            public float CheckDelay { get; }

            public Data(float checkDelay)
            {
                CheckDelay = checkDelay;
            }
        }

        #endregion



        #region Fields

        private readonly IPoolHelper<string, EffectHandler> poolHelper;

        private readonly List<EffectHandler> systems;

        private readonly Data data;

        private float checkTime;

        #endregion



        #region Ctor

        public EffectAliveHandler(IPoolHelper<string, EffectHandler> _poolHelper,
                                  Data _data)
        {
            poolHelper = _poolHelper;

            systems = new List<EffectHandler>(64);

            data = _data;

            checkTime = 0f;
        }

        #endregion



        #region IEffectAliveHandler

        public void BindComponent(EffectHandler component)
        {
            component.OnPushed += Component_OnPushed;

            systems.Add(component);
        }


        public void CustomUpdate(float deltaTime)
        {
            if (checkTime >= data.CheckDelay)
            {
                CheckAliveComponents();

                checkTime = 0f;
            }
            else
            {
                checkTime += deltaTime;
            }
        }

        #endregion



        #region Private methods

        private void CheckAliveComponents()
        {
            for (int i = systems.Count - 1; i >= 0; i--)
            {
                EffectHandler handler = systems[i];

                if (handler.IsNull())
                {
                    systems.RemoveAt(i);
                }
                else if (!handler.IsAlive)
                {
                    poolHelper.PushObject(handler);

                    RemoveComponent(handler);
                }
            }
        }

        private void RemoveComponent(EffectHandler component)
        {
            component.OnPushed -= Component_OnPushed;

            systems.Remove(component);
        }

        #endregion



        #region Events handlers

        private void Component_OnPushed(EffectHandler pushedComponent)
        {
            RemoveComponent(pushedComponent);
        }

        #endregion
    }
}

