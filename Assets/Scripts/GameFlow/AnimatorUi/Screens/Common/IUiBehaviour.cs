namespace Drawmasters.Ui
{
    public interface IUiBehaviour : IDeinitializable
    {
        void Enable();
        void Disable();

        void InitializeButtons();
        void DeinitializeButtons();
    }
}
