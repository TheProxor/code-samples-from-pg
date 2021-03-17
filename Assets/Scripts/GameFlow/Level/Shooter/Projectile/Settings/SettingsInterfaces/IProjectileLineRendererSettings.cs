using UnityEngine;

namespace Drawmasters.Levels
{
    public interface IProjectileLineRendererSettings
    {
        float BeginWidth { get; }

        float EndWidth { get; }

        Gradient LineGradient { get; }
    }
}
