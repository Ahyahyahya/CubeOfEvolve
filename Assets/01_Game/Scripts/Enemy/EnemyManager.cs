using R3;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    // �V���O���g��
    public static EnemyManager Instance;

    // ---------------------------- SerializeField
    [Header("�v���C���[")]
    [SerializeField, Tooltip("�v���C���[")] private GameObject _playerObj;


    // ---------------------------- Field
    private List<EnemyStatus> _enemyList = new();


    // ---------------------------- Property
    public GameObject PlayerObj { get { return _playerObj; } }


    // ---------------------------- UnityMessage
    private void Awake()
    {
        // �V���O���g��
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // �q�I�u�W�F�N�g��ۑ�
        for (int i = 0; i < transform.childCount; i++)
        {
            var enemyObj = transform.GetChild(i).gameObject;
            _enemyList.Add(enemyObj.GetComponent<EnemyStatus>());
        }
    }
    private void Start()
    {
        // ���̔���
        foreach (var enemy in _enemyList)
        {
            enemy.Hp
                .Where(value => value <= 0)
                .Subscribe(value =>
                {
                    Destroy(enemy.gameObject);

                    _enemyList.Remove(enemy);
                })
                .AddTo(enemy.gameObject);
        }
    }
}
