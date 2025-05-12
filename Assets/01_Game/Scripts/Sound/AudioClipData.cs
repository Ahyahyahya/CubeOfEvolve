// �P��̃I�[�f�B�I�N���b�v�����T�E���h�f�[�^�B
using UnityEngine;

[CreateAssetMenu(fileName = "AudioClipData", menuName = "Sound/AudioClipData")]
public class AudioClipData : SoundData
{
    // �Đ�����I�[�f�B�I�N���b�v�B
    public AudioClip audioClip;

    // �I�[�f�B�I�N���b�v��Ԃ������B
    public override AudioClip GetAudioClip()
    {
        return audioClip;
    }

    // �T�E���h�̖��O��Ԃ������B
    public override string GetName()
    {
        return name;
    }
}