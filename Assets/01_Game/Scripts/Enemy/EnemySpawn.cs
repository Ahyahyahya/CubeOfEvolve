using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    [Serializable]
    private class Wave
    {
        public float delaySecond;
        public List<GameObject> enemyList = new();
    }

    // ---------------------------- SerializeField
    [Header("�E�F�[�u"), SerializeField] private List<Wave> _waveList = new();

    [Header("�����̍���"), SerializeField] private float _startHeight;
    [Header("��������b����"), SerializeField] private float _spawnHeight;


    // ---------------------------- Field
    //private List<EnemyStatus> _enemyStatusList = new();

    private int _nextWave;

    private Coroutine _coroutine;
    private float _currentDelay;

    // ---------------------------- Property
    private void Awake()
    {
        // �o�^���ꂽ�G�̃X�e�[�^�X��ۑ�
        foreach (var wave in _waveList)
        {
            foreach (var enemy in wave.enemyList)
            {
                enemy.transform.position
                = new Vector3(enemy.transform.position.x, _startHeight, enemy.transform.position.z);
                //_enemyStatusList.Add(enemy.GetComponent<EnemyStatus>());
            }
        }

        _nextWave = 0;

        // ���̃E�F�[�u�̑҂�����
        _currentDelay = _waveList[_nextWave].delaySecond;

        // �E�F�[�u��i�߂鏈��
        _coroutine = StartCoroutine(SpawnCoroutine());
    }

    // ---------------------------- UnityMessage


    // ---------------------------- PublicMethod


    // ---------------------------- PrivateMethod
    /// <summary>
    /// �G�𐶐����鏈��
    /// </summary>
    private void EnemySpawnProcess()
    {
        foreach (var enemy in _waveList[_nextWave].enemyList)
        {
            enemy.transform.position
                = new Vector3(enemy.transform.position.x, _spawnHeight, enemy.transform.position.z);

            enemy.GetComponent<EnemyStatus>().EnemySpawn();
        }

        _nextWave++;

        if (_nextWave > _waveList.Count) return;

        // ���̃E�F�[�u�̑҂�����
        _currentDelay = _waveList[_nextWave].delaySecond;
    }

    private IEnumerator SpawnCoroutine()
    {
        while (_nextWave < _waveList.Count)
        {
            yield return new WaitForSeconds(_currentDelay);

            // �G�𐶐����鏈��
            EnemySpawnProcess();
        }

        yield break;
    }
}
