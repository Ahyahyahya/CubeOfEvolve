using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnWave", menuName = "ScriptableObjects/Data/SpawnWave")]
public class SpawnWaveData : ScriptableObject
{
    public enum SpawnPatternType
    {
        [InspectorName("��x��������")]
        Event,
        [InspectorName("���[�v")]
        Loop,
    }

    public float delaySecond;               // �J�n����
    public float duration;                  // ��������
    public int spawnCount;                  // ������
    public List<GameObject> enemyList;      // �G�̎��
    public SpawnPatternType patternType;    // �������@
    public float interval;                  // ���[�v�p(�����Ԋu)
}
