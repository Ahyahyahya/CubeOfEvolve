using Assets.AT;
using Assets.IGC2025.Scripts.GameManagers;
using UnityEngine;

public class UIReferenceRelay : MonoBehaviour
{
    /// <summary>
    /// �V�[����{�^�������GameState�̕ύX(���O�̃X�e�[�g�ɖ߂�)
    /// </summary>
    /// <param name="state"></param>
    [EnumAction(typeof(GameState))]
    public void OnButtonChangeGameState()
    {
        GameManager.Instance.ChangeGameState(GameManager.Instance.PrevGameState);
    }

    /// <summary>
    /// �V�[����{�^�������GameState�̕ύX
    /// </summary>
    /// <param name="state"></param>
    [EnumAction(typeof(GameState))]
    public void OnButtonChangeGameState(int state)
    {
        GameManager.Instance.ChangeGameState(state);
    }

    /// <summary>
    /// �V�[����{�^������̃J�����̕ύX
    /// </summary>
    /// <param name="targetCameraKey"></param>
    public void OnButtonChangeCamera(string targetCameraKey)
    {
        CameraCtrlManager.Instance.ChangeCamera(targetCameraKey);
    }

    /// <summary>
    /// �V�[����{�^������́u��蒼���v�Ăяo��
    /// </summary>
    public void OnRetryButtonPressed()
    {
        var gameManager = GameManager.Instance;
        gameManager.RequestRetry();
        gameManager.SceneLoader.ReloadScene();
    }

    /// <summary>
    /// �V�[����{�^������́u�^�C�g���֖߂�v�Ăяo��
    /// </summary>
    public void OnButtonReloadScene()
    {
        var gameManager = GameManager.Instance;
        gameManager.RequestReturnToTitle();
        gameManager.SceneLoader.ReloadScene();
    }


}
