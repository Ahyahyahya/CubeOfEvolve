using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "CheckDialogConfig", menuName = "UI/Check Dialog Config")]
public class CheckDialogConfig : ScriptableObject
{
    public string message;
    public string yesButtonText = "�͂�";
    public string noButtonText = "������";
    public CheckActionReference actionReference; // �C���^�[�t�F�[�X�����������N���X�ւ̎Q��
}