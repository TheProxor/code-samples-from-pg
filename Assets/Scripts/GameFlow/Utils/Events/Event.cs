using System;
using System.Collections.Generic;


namespace Core
{
    public class Event : IEvent
    {
        private Action actions;

        public void Notify()
        {
            actions?.Invoke();
        }



        public void Subscribe(Action action)
        {
            actions += action;
        }


        public void Unsubscribe(Action action)
        {
            actions -= action;
        }


        public void Register()
        {
            NotifierEvents.Register(this);
        }
    }



    public class Event<TArgument> : IEvent
    {
        private Action<TArgument> actions;
        private readonly Queue<TArgument> arguments = new Queue<TArgument>();


        public void Notify()
        {
            actions?.Invoke(arguments.Dequeue());
        }



        public void Subscribe(Action<TArgument> action)
        {
            actions += action;
        }


        public void Unsubscribe(Action<TArgument> action)
        {
            actions -= action;
        }


        public void Register(TArgument argument)
        {
            NotifierEvents.Register(this);
            arguments.Enqueue(argument);
        }
    }
}
