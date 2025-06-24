using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class EnemySpawnBase : MonoBehaviour
{
    [Serializable]
    public class SpawnType
    {
        [Header("��������������")]
        public float delaySecond;

        [Header("������")]
        public int spawnValue;

        [Header("�����������G�̎��")]
        public List<GameObject> enemyList = new();
    }

    // ---------------------------- SerializeField
    [Header("�G"), SerializeField] protected SpawnType[] _spawnType;

    [Header("��������"), SerializeField] private float _playerDistance;


    // ---------------------------- Field
    protected SpawnType _currentSpawnType = new();
    private GameObject _targetObj;                  // �U���Ώ�

    // ---------------------------- UnityMessage
    private void Start()
    {
        _targetObj = PlayerMonitoring.Instance.PlayerObj;

        GameManager.Instance.CurrentGameState
            .Where(value => value == Assets.IGC2025.Scripts.GameManagers.GameState.BATTLE)
            .Take(1)
            .Subscribe(_ => StartMethod())
            .AddTo(this);
    }

    // ---------------------------- AbstractMethod
    public abstract void StartMethod();

    // ---------------------------- ProtectedMethod
    /// <summary>
    /// ��������
    /// </summary>
    protected void Spawn(GameObject enemyObj)
    {
        // 360�x���璊�I
        float spawnAngle = Random.Range(0, 360);
        // ���W�A���p�ɕύX
        float radians = spawnAngle * Mathf.Deg2Rad;
        // ����
        Vector3 direction = new Vector3(Mathf.Sin(radians), 0, Mathf.Cos(radians)).normalized;
        // �^�[�Q�b�g�����_�ɐ����ʒu������
        Vector3 spawnPos = _targetObj.transform.position + direction * _playerDistance;

        // �G�𐶐�
        var obj = Instantiate(enemyObj);
        obj.transform.position = spawnPos;

        // �G�̃X�e�[�^�X�̏����ݒ菈��
        obj.GetComponent<EnemyStatus>().EnemySpawn();
    }
}
