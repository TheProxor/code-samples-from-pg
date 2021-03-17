using Drawmasters.ServiceUtil;


namespace Drawmasters.Levels
{
    public class ProjectilePetTrail : ProjectileTrail
    {
        #region Methods

        protected override string GetTrailFxKey()
        {
            PetSkinType petType = GameServices.Instance.PlayerStatisticService.PlayerData.CurrentPetSkin;
            string result = IngameData.Settings.pets.weaponSkinSettings.FindProjectileTrailFxsKey(petType);

            return result;
        }

        #endregion
    }
}
