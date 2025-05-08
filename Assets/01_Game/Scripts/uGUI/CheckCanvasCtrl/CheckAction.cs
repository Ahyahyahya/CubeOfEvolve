using UnityEngine;

[CreateAssetMenu(fileName = "QuitGameAction", menuName = "UI/Check Actions/Quit Game")]
public class QuitGameActionSO : ScriptableObject, ICheckAction
{
    /// <summary>
    /// �u�͂��v�{�^���������ꂽ�ۂ̏����F�Q�[�����I�����܂��B
    /// </summary>
    public void OnYes()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // �G�f�B�^�[��ł̎��s���~
#else
        Application.Quit(); // �r���h��̃A�v���P�[�V�������I��
#endif
        Debug.Log("�Q�[���I������");
    }

    /// <summary>
    /// �u�������v�{�^���������ꂽ�ۂ̏����F�Q�[���I�����L�����Z�����܂��B
    /// </summary>
    public void OnNo()
    {
        Debug.Log("�Q�[���I�����L�����Z��");
    }
}

[CreateAssetMenu(fileName = "ResetGameAction", menuName = "UI/Check Actions/Reset Game")]
public class ResetGameActionSO : ScriptableObject, ICheckAction
{
    /// <summary>
    /// �u�͂��v�{�^���������ꂽ�ۂ̏����F�Q�[���f�[�^�����Z�b�g���܂��B
    /// </summary>
    public void OnYes()
    {
        // �����ɃQ�[���f�[�^�̃��Z�b�g�������L�q���܂��B
        // ��FPlayerPrefs.DeleteAll(); ��A�Z�[�u�f�[�^�̃t�@�C�����폜����ȂǁB
    }

    /// <summary>
    /// �u�������v�{�^���������ꂽ�ۂ̏����F�Q�[���f�[�^�̃��Z�b�g���L�����Z�����܂��B
    /// </summary>
    public void OnNo()
    {
        Debug.Log("�Q�[���f�[�^���Z�b�g���L�����Z��");
    }
}