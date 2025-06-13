using UnityEngine;

[CreateAssetMenu(fileName = "QuitCheckAction", menuName = "UI/Check Action/Quit")]
public class QuitGameAction : ScriptableObject, ICheckAction
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
