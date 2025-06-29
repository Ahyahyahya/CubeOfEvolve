using R3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField] private List<SpawnWaveData> _waves;
    [SerializeField] private float _playerDistance = 10f;

    // ---------------------------- Field
    private GameObject _target;

    // ---------------------------- UnityMessage
    private void Start()
    {
        _target = PlayerMonitoring.Instance.PlayerObj;

        GameManager.Instance.CurrentGameState
            .Where(value => value == Assets.IGC2025.Scripts.GameManagers.GameState.BATTLE)
            .Take(1)
            .Subscribe(_ =>
            {
                // delaySecond �̏����ɕ��ёւ��Ă���
                _waves.Sort((a, b) => a.delaySecond.CompareTo(b.delaySecond));
                StartCoroutine(SpawnWaveSequence());
            })
            .AddTo(this);
    }

    // ---------------------------- PrivateMethod
    /// <summary>
    /// ������SpawnWaveData�����Ԃɏ���
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpawnWaveSequence()
    {
        foreach (var wave in _waves)
        {
            // ���݂̌o�ߎ��Ԃ���Wave�J�n�܂ł̑ҋ@���Ԃ��v�Z
            float waitTime = wave.delaySecond - GameManager.Instance.TimeManager.CurrentTimeSecond.CurrentValue;

            if (waitTime > 0)
            {
                // �w�莞�Ԃ����ҋ@����Wave�̊J�n��x�点��
                yield return new WaitForSeconds(waitTime);
            }

            // �C�x���g
            if (wave.patternType == SpawnWaveData.SpawnPatternType.Event)
            {
                for (int i = 0; i < wave.spawnCount; i++)
                {
                    Spawn(wave.enemyList[Random.Range(0, wave.enemyList.Count)]);
                }
            }
            // ���[�v
            else if (wave.patternType == SpawnWaveData.SpawnPatternType.Loop)
            {
                // Wave�̊J�n����
                float startTime = Time.time;
                // Wave�̏I������
                float endTime = startTime + wave.duration;
                // ���ɃX�|�[�����s������
                float nextSpawnTime = startTime;

                // Wave�̏I�����Ԃ܂Ń��[�v�������p��
                while (Time.time < endTime)
                {
                    // ���̃X�|�[���\�莞�ԂɒB�������𔻒�
                    if (Time.time >= nextSpawnTime)
                    {
                        // �w�肳�ꂽ�������G�𐶐�
                        for (int i = 0; i < wave.spawnCount; i++)
                        {
                            Spawn(wave.enemyList[Random.Range(0, wave.enemyList.Count)]);
                        }

                        // ���̃X�|�[�����Ԃ��X�V
                        nextSpawnTime += wave.interval;
                    }

                    // ���t���[��1�񃋁[�v���p���A���׌y���̂��� WaitForSeconds �͎g��Ȃ�
                    yield return null;
                }
            }
        }
    }


    /// <summary>
    /// ��������
    /// </summary>
    /// <param name="enemyPrefab">��������G�̃v���n�u</param>
    private void Spawn(GameObject enemyPrefab)
    {
        // 0�`360�x�̃����_���Ȋp�x�𐶐�
        float spawnAngle = Random.Range(0, 360);

        // �p�x�����W�A���ɕϊ�
        float radians = spawnAngle * Mathf.Deg2Rad;

        // ���W�A������XZ���ʂ̕����x�N�g�����v�Z�iY��0�ɌŒ�j
        Vector3 direction = new Vector3(Mathf.Sin(radians), 0, Mathf.Cos(radians)).normalized;

        // �v���C���[�ʒu����w�苗���̕����փI�t�Z�b�g���Đ����ʒu������
        Vector3 spawnPos = _target.transform.position + direction * _playerDistance;

        // �G�I�u�W�F�N�g�𐶐����A�ʒu�Ɖ�]��ݒ�
        var enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        // ������ɓG�̏��������\�b�h���Ăяo���iEnemyStatus�R���|�[�l���g������΁j
        enemy.GetComponent<EnemyStatus>()?.EnemySpawn();
    }
}
