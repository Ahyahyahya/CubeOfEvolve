using UnityEngine;

[CreateAssetMenu(fileName = "ResetCheckAction", menuName = "UI/Check Action/Reset")]
public class ResetGameAction : ScriptableObject, ICheckAction
{
    /// <summary>
    /// �u�͂��v�{�^���������ꂽ�ۂ̏����F�Q�[���f�[�^�����Z�b�g���܂��B
    /// </summary>
    public void OnYes()
    {
        // �����ɃQ�[���f�[�^�̃��Z�b�g�������L�q���܂��B
        var gameManager = GameManager.Instance;
        gameManager.RequestResetAll();
        gameManager.SceneLoader.ReloadScene();
    }

    /// <summary>
    /// �u�������v�{�^���������ꂽ�ۂ̏����F�Q�[���f�[�^�̃��Z�b�g���L�����Z�����܂��B
    /// </summary>
    public void OnNo()
    {
        Debug.Log("�Q�[���f�[�^���Z�b�g���L�����Z��");
    }
}