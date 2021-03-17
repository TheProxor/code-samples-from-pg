using Drawmasters.Ui.Enums;


namespace Drawmasters.Ui
{
    public interface IMainMenuBehaviour : IUiBehaviour
    {
        bool IsAnyForceProposeActive { get; set; }
        
        bool IsMechanicAvailable { get; }
        
        MainMenuScreenState ScreenState { get; }
        
        CameraOffsetSettings CameraOffsetSettings { get; }
    }
}