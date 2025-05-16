using R3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawn_Loop : EnemySpawnBase
{
    // ---------------------------- SerializeField
    [Header("������"), SerializeField] private float _spawnValue;

    [Header("�����Ԋu"), SerializeField] private float _spawnInterval;

    [Header("�Ԋu�����x"), SerializeField] private float _intervalAccelerate;


    // ---------------------------- Field
    private List<GameObject> _currentSpawnObjList = new();

    // ---------------------------- OverrideMethod
    public override void StartMethod()
    {
        foreach (var spawnType in _spawnType)
        {
            _timeManager.CurrentTimeSecond
                .Where(value => spawnType.delaySecond <= value)
                .Take(1)
                .Subscribe(value =>
                {
                    _currentSpawnObjList = spawnType.enemyList;
                })
                .AddTo(this);
        }

        // �E�F�[�u��i�߂鏈��
        StartCoroutine(SpawnCoroutine());
    }

    // ---------------------------- PrivateMethod
    /// <summary>
    /// �����R���[�`��
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            // �����Ԋu �~ ����(�o�ߎ��� )
            yield return new WaitForSeconds(_spawnInterval);

            // �G�𐶐����鏈��
            for (int i = 0; i < _spawnValue; i++)
            {
                int num = Random.Range(0, _currentSpawnObjList.Count);
                Spawn(_currentSpawnObjList[num]);
            }
        }
    }
}
