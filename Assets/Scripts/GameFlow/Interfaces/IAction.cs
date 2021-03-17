using System;


namespace Drawmasters
{
    public interface IAction
    {
        void Run(Action ended);
    }
}