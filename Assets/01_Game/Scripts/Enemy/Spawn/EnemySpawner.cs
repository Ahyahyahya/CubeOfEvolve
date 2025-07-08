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

                // �SWave���J�n
                foreach (var wave in _waves)
                {
                    StartCoroutine(StartWave(wave));
                }
            })
            .AddTo(this);
    }

    // ---------------------------- PrivateMethod
    /// <summary>
    /// �E�F�[�u���J�n����
    /// </summary>
    /// <param name="wave"></param>
    /// <returns></returns>
    private IEnumerator StartWave(SpawnWaveData wave)
    {
        float waitTime = wave.delaySecond - GameManager.Instance.TimeManager.CurrentTimeSecond.CurrentValue;

        if (waitTime > 0)
        {
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
            // �J�n���_
            float nextSpawnTime = startTime;
            // wave.duration �� -1 �̂Ƃ��͖������[�v�A����ȊO�͐������ԕt�����[�v
            bool isInfinite = wave.duration < 0;
            float endTime = isInfinite ? float.MaxValue : startTime + wave.duration;

            // ���ݎ������I���������߂���܂Ń��[�v�i�܂��͖����j
            while (Time.time < endTime)
            {
                // ���ݎ��������̃X�|�[���^�C�~���O�ɂȂ�����
                if (Time.time >= nextSpawnTime)
                {
                    for (int i = 0; i < wave.spawnCount; i++)
                    {
                        Spawn(wave.enemyList[Random.Range(0, wave.enemyList.Count)]);
                    }

                    // ����̃X�|�[�����Ԃ� interval �b��ɐݒ�
                    nextSpawnTime += wave.interval;
                }

                yield return null;
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
