// �T�E���h�f�[�^�̊��N���X�iScriptableObject�j�B
// �e�T�E���h�f�[�^�͂��̃N���X���p�����ċ�̓I�ȃf�[�^�����B
using UnityEngine;

public abstract class SoundData : ScriptableObject
{
    // �T�E���h�̖��O�i���ʎq�j�B
    public string name;

    // �T�E���h�̃I�[�f�B�I�N���b�v���擾���钊�ۃ��\�b�h�B
    public abstract AudioClip GetAudioClip();

    // �T�E���h�̖��O���擾���钊�ۃ��\�b�h�B
    public abstract string GetName();
}

