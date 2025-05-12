// ���[�v�Đ��ɑΉ������T�E���h�f�[�^�B
using UnityEngine;

[CreateAssetMenu(fileName = "SoundLoopData", menuName = "Sound/SoundLoopData")]
public class LoopSoundData : SoundData
{
    // �Đ�����I�[�f�B�I�N���b�v�B
    public AudioClip audioClip;

    // ���[�v�J�n�ʒu�i�T���v���P�ʁj�B
    public int loopStart;

    // ���[�v�I���ʒu�i�T���v���P�ʁj�B
    public int loopEnd;

    // �I�[�f�B�I�N���b�v�̃T���v�����O���g���B
    public int frequency = 44100;

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