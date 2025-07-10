using Assets.IGC2025.Scripts.GameManagers;
using UnityEngine;

public class UIReferenceRelay : MonoBehaviour
{
    [EnumAction(typeof(GameState))]
    public void OnButtonChangeGameState(int state)
    {
        GameManager.Instance.ChangeGameState(state);
    }

    /// <summary>
    /// �V�[����{�^������́u��蒼���v�Ăяo��
    /// </summary>
    public void OnRetryButtonPressed()
    {
        GameManager.Instance.RequestRetry();
    }

    /// <summary>
    /// �V�[����{�^������́u�^�C�g���֖߂�v�Ăяo��
    /// </summary>
    public void OnButtonReloadScene()
    {
        GameManager.Instance.SceneLoader.ReloadScene();
    }
}
