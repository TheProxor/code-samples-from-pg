using Drawmasters;
using Drawmasters.Levels;
using UnityEngine;


public static class PhysicsSolutions
{
    public static void ApplySmashSettings(ref Projectile projectile, Vector2 smashPosition, CollidableObjectType smashType)
    {
        var settings = IngameData.Settings.projectileSmashSettings;

        float savedMagnitude = projectile.MainRigidbody2D.velocity.magnitude * settings.FindSavedVelocityPart(smashType);
        Vector2 velocity = savedMagnitude * projectile.MainRigidbody2D.velocity.normalized;

        projectile.MainRigidbody2D.velocity = velocity;
        projectile.MainRigidbody2D.angularVelocity = default;

        Vector2 forceDirection = (projectile.PreviousFrameRigidbody2D.Position.ToVector2() - smashPosition).normalized;
        projectile.MainRigidbody2D.AddTorque(settings.FindAdditionalTorque(smashType), ForceMode2D.Impulse);
        projectile.MainRigidbody2D.AddForce(settings.FindAdditionalSmashForce(smashType) * forceDirection, ForceMode2D.Impulse);

        var gravityAnimation = settings.FindSmashGravityScaleAnimation(smashType);

        Projectile savedProjectile = projectile;
        gravityAnimation.Play((value) => savedProjectile.MainRigidbody2D.gravityScale = value, projectile);
    }
}
