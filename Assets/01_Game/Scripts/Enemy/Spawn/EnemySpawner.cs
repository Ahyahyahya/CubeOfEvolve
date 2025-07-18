using Assets.IGC2025.Scripts.GameManagers;
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

        _waves.Sort((a, b) => a.delaySecond.CompareTo(b.delaySecond));

        GameManager.Instance.CurrentGameState
            .Where(value => value == GameState.BATTLE)
            .Take(1)
            .Subscribe(_ =>
            {
                // ��Ԃ�BATTLE�̂Ƃ��̂�Wave�J�n
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
    private IEnumerator StartWave(SpawnWaveData wave)
    {
        // �o�g�����̂݃J�E���g����x������
        float elapsed = 0f;
        while (elapsed < wave.delaySecond)
        {
            if (GameManager.Instance.CurrentGameState.CurrentValue == GameState.BATTLE)
            {
                elapsed += Time.deltaTime;
            }
            yield return null;
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
            float loopElapsed = 0f;
            float nextSpawnTime = wave.interval;
            bool isInfinite = wave.duration < 0f;

            while (isInfinite || loopElapsed < wave.duration)
            {
                if (GameManager.Instance.CurrentGameState.CurrentValue == GameState.BATTLE)
                {
                    // �o���^�C�~���O�ɒB������
                    if (loopElapsed >= nextSpawnTime)
                    {
                        for (int i = 0; i < wave.spawnCount; i++)
                        {
                            Spawn(wave.enemyList[Random.Range(0, wave.enemyList.Count)]);
                        }

                        nextSpawnTime += wave.interval;
                    }

                    loopElapsed += Time.deltaTime;
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
