using System;
using System.Collections.Generic;
using UnityEngine;


namespace Core
{
    public static class InputKeys
    {
        public class TypeEventInputKey : FilteredEvent<KeyCode>
        {
            public override void Subscribe(KeyCode key, Action action)
            {
                base.Subscribe(key, action);
                _keys.Add(key);
            }

            public override void Unsubscribe(KeyCode key, Action action)
            {
                base.Unsubscribe(key, action);
                _keys.Remove(key);
            }
        }

        public class TypeEventInputKeyDown : FilteredEvent<KeyCode>
        {
            public override void Subscribe(KeyCode key, Action action)
            {
                base.Subscribe(key, action);
                _keysDown.Add(key);
            }

            public override void Unsubscribe(KeyCode key, Action action)
            {
                base.Unsubscribe(key, action);
                _keysDown.Remove(key);
            }
        }


        public class TypeEventInputKeyUp : FilteredEvent<KeyCode>
        {
            public override void Subscribe(KeyCode key, Action action)
            {
                base.Subscribe(key, action);
                _keysDown.Add(key);
            }

            public override void Unsubscribe(KeyCode key, Action action)
            {
                base.Unsubscribe(key, action);
                _keysDown.Remove(key);
            }
        }
        

        public static TypeEventInputKey EventInputKey = new TypeEventInputKey();
        public static TypeEventInputKeyDown EventInputKeyDown = new TypeEventInputKeyDown();
        public static TypeEventInputKeyUp EventInputKeyUp = new TypeEventInputKeyUp(); 

        static InputKeys()
        {
            MonoBehaviourLifecycle.OnUpdate += Update;
        }

        private static void Update(float deltaTime)
        {
            if (_keys.Count > 0 && Input.anyKey)
            {
                foreach (KeyCode key in _keys)
                {
                    if (!Input.GetKey(key)) continue;
                    EventInputKey.Register(key);
                }
            }

            if (_keysDown.Count > 0 && Input.anyKeyDown)
            {
                foreach (KeyCode key in _keysDown)
                {
                    if (!Input.GetKeyDown(key)) continue;
                    EventInputKeyDown.Register(key);
                }
            }

            if (_keysDown.Count > 0)
            {
                foreach (KeyCode key in _keysDown)
                {
                    if (Input.GetKeyUp(key))
                    {
                        EventInputKeyUp.Register(key);
                    }
                }
            }
        }

        private static HashSet<KeyCode> _keys = new HashSet<KeyCode>();
        private static HashSet<KeyCode> _keysDown = new HashSet<KeyCode>();
    }
}
