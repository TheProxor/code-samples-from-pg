using System;
using System.Collections.Generic;


namespace Core
{
    public class GlobalEvent<TClass> : IEvent where TClass : GlobalEvent<TClass>, new()
    {
        static readonly TClass instance = new TClass();

        Action actions;


        public void Notify() => actions?.Invoke();


        public static void Subscribe(Action action) => instance.actions += action;

        public static void Unsubscribe(Action action) => instance.actions -= action;


        public static void Register() => NotifierEvents.Register(instance);
    }



    public class GlobalEvent<TClass, TArgument> : IEvent where TClass : GlobalEvent<TClass, TArgument>, new()
    {
        static readonly TClass instance = new TClass();

        Action<TArgument> actions;
        readonly Queue<TArgument> arguments = new Queue<TArgument>();


        public void Notify()
        {
            TArgument argument = arguments.Dequeue();
            actions?.Invoke(argument);
        }


        public static void Subscribe(Action<TArgument> action) => instance.actions += action;

        public static void Unsubscribe(Action<TArgument> action) => instance.actions -= action;


        public static void Register(TArgument argument)
        {
            instance.arguments.Enqueue(argument);
            NotifierEvents.Register(instance);
        }
    }


    /// <summary>
    /// A safe version of a global event. Adds only unique actions
    /// </summary>
    public class SingleSubscriptionEvent<TClass, TArgument> : IEvent where TClass : SingleSubscriptionEvent<TClass, TArgument>, new()
    {
        static readonly TClass instance = new TClass();

        Action<TArgument> actions;
        readonly Queue<TArgument> arguments = new Queue<TArgument>();


        public void Notify()
        {
            TArgument argument = arguments.Dequeue();
            actions?.Invoke(argument);
        }


        public static void Subscribe(Action<TArgument> action)
        {
            instance.actions -= action;
            instance.actions += action;
        }

        public static void Unsubscribe(Action<TArgument> action) => instance.actions -= action;


        public static void Register(TArgument argument)
        {
            instance.arguments.Enqueue(argument);
            NotifierEvents.Register(instance);
        }
    }


    public class GlobalEvent<TClass, TArgument1, TArgument2> : IEvent where TClass : GlobalEvent<TClass, TArgument1, TArgument2>, new()
    {
        static readonly TClass instance = new TClass();

        Action<TArgument1, TArgument2> actions;
        readonly Queue<TArgument1> arguments1 = new Queue<TArgument1>();
        readonly Queue<TArgument2> arguments2 = new Queue<TArgument2>();


        public void Notify()
        {
            TArgument1 argument1 = arguments1.Dequeue();
            TArgument2 argument2 = arguments2.Dequeue();
            actions?.Invoke(argument1, argument2);
        }


        public static void Subscribe(Action<TArgument1, TArgument2> action) => instance.actions += action;

        public static void Unsubscribe(Action<TArgument1, TArgument2> action) => instance.actions -= action;


        public static void Register(TArgument1 argument1, TArgument2 argument2)
        {
            instance.arguments1.Enqueue(argument1);
            instance.arguments2.Enqueue(argument2);
            NotifierEvents.Register(instance);
        }
    }



    public class GlobalEvent<TClass, TArgument1, TArgument2, TArgument3> : IEvent where TClass : GlobalEvent<TClass, TArgument1, TArgument2, TArgument3>, new()
    {
        static readonly TClass instance = new TClass();

        Action<TArgument1, TArgument2, TArgument3> actions;
        readonly Queue<TArgument1> arguments1 = new Queue<TArgument1>();
        readonly Queue<TArgument2> arguments2 = new Queue<TArgument2>();
        readonly Queue<TArgument3> arguments3 = new Queue<TArgument3>();


        public void Notify()
        {
            TArgument1 argument1 = arguments1.Dequeue();
            TArgument2 argument2 = arguments2.Dequeue();
            TArgument3 argument3 = arguments3.Dequeue();
            actions?.Invoke(argument1, argument2, argument3);
        }


        public static void Subscribe(Action<TArgument1, TArgument2, TArgument3> action) => instance.actions += action;

        public static void Unsubscribe(Action<TArgument1, TArgument2, TArgument3> action) => instance.actions -= action;


        public static void Register(TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            instance.arguments1.Enqueue(argument1);
            instance.arguments2.Enqueue(argument2);
            instance.arguments3.Enqueue(argument3);
            NotifierEvents.Register(instance);
        }
    }



    public class GlobalEvent<TClass, TArgument1, TArgument2, TArgument3, TArgument4> : IEvent where TClass : GlobalEvent<TClass, TArgument1, TArgument2, TArgument3, TArgument4>, new()
    {
        static readonly TClass instance = new TClass();

        Action<TArgument1, TArgument2, TArgument3, TArgument4> actions;
        readonly Queue<TArgument1> arguments1 = new Queue<TArgument1>();
        readonly Queue<TArgument2> arguments2 = new Queue<TArgument2>();
        readonly Queue<TArgument3> arguments3 = new Queue<TArgument3>();
        readonly Queue<TArgument4> arguments4 = new Queue<TArgument4>();


        public void Notify()
        {
            TArgument1 argument1 = arguments1.Dequeue();
            TArgument2 argument2 = arguments2.Dequeue();
            TArgument3 argument3 = arguments3.Dequeue();
            TArgument4 argument4 = arguments4.Dequeue();
            actions?.Invoke(argument1, argument2, argument3, argument4);
        }


        public static void Subscribe(Action<TArgument1, TArgument2, TArgument3, TArgument4> action) => instance.actions += action;

        public static void Unsubscribe(Action<TArgument1, TArgument2, TArgument3, TArgument4> action) => instance.actions -= action;


        public static void Register(TArgument1 argument1, TArgument2 argument2, TArgument3 argument3, TArgument4 argument4)
        {
            instance.arguments1.Enqueue(argument1);
            instance.arguments2.Enqueue(argument2);
            instance.arguments3.Enqueue(argument3);
            instance.arguments4.Enqueue(argument4);
            NotifierEvents.Register(instance);
        }
    }
}
