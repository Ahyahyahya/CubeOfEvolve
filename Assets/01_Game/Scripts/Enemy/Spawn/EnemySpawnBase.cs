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

        [Header("�����������G�̎��")]
        public List<GameObject> enemyList = new();
    }

    // ---------------------------- SerializeField
    [Header("�G"), SerializeField] protected SpawnType[] _spawnType;

    [Header("��������"), SerializeField] private float _playerDistance;

    [Header(""), SerializeField] protected TimeManager _timeManager;


    // ---------------------------- Field
    private GameObject _targetObj;                  // �U���Ώ�

    // ---------------------------- UnityMessage
    private void Start()
    {
        _targetObj = EnemyManager.Instance.PlayerObj;

        StartMethod();
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
        var obj = Instantiate(enemyObj, EnemyManager.Instance.transform);
        obj.transform.position = spawnPos;

        // �G�̃X�e�[�^�X�̏����ݒ菈��
        obj.GetComponent<EnemyStatus>().EnemySpawn();
    }
}
