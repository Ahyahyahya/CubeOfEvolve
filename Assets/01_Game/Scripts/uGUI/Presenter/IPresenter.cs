namespace Assets.IGC2025.Scripts.Presenter
{
    public interface IPresenter
    {
        /// <summary>�������ς݂�</summary>
        bool IsInitialized { get; }

        /// <summary>Presenter�̏������B���f���������オ������ɌĂ΂��z��B</summary>
        void Initialize();
    }
}