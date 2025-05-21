using R3;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn_Event : EnemySpawnBase
{
    // ---------------------------- OverrideMethod
    public override void StartMethod()
    {
        foreach (var spawnType in _spawnType)
        {
            GameManager.Instance.TimeManager.CurrentTimeSecond
                .Where(value => spawnType.delaySecond <= value)
                .Take(1)
                .Subscribe(value =>
                {
                    _currentSpawnType = spawnType;

                    // �G�𐶐����鏈��
                    int num = Random.Range(0, _currentSpawnType.enemyList.Count);

                    // �G�𐶐����鏈��
                    for (int i = 0; i < _currentSpawnType.spawnValue; i++)
                    {
                        Spawn(_currentSpawnType.enemyList[num]);
                    }
                })
                .AddTo(this);
        }
    }
}
