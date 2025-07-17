using System.Collections.Generic;
using UnityEngine;

public class LayerSearch : MonoBehaviour
{
    // ---------------------------- Field
    private float _range;                 // �T���͈�
    private LayerMask _layerMask;       // ���o�Ώۂ̃��C���[��

    private GameObject _nearestTargetObj; // �ł��߂��ΏۃI�u�W�F�N�g
    private readonly List<GameObject> _nearestTargetList = new(); // �͈͓��̓G�I�u�W�F�N�g�ꗗ

    // ---------------------------- Property
    /// <summary>
    /// �ł��߂��ΏۃI�u�W�F�N�g
    /// </summary>
    public GameObject NearestTargetObj
    {
        get
        {
            SearchEnemiesInRange();
            return _nearestTargetObj;
        }
    }

    /// <summary>
    /// �͈͓��̑ΏۃI�u�W�F�N�g�ꗗ
    /// </summary>
    public List<GameObject> NearestTargetList
    {
        get
        {
            SearchEnemiesInRange();
            return _nearestTargetList;
        }
    }

    // ---------------------------- PrivateMethod
    /// <summary>
    /// �͈͓��̓G�����o���A�ł��߂��G����肷��B
    /// </summary>
    private void SearchEnemiesInRange()
    {
        float nearestDistance = float.MaxValue;
        _nearestTargetObj = null;
        _nearestTargetList.Clear();

        // �w�背�C���[���ŁA���͈͓��ɂ���R���C�_�[���擾
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            _range,
            _layerMask);

        foreach (var hit in hits)
        {
            if (hit == null) continue;

            GameObject enemyRoot = hit.transform.root.gameObject;

            // ���X�g�ɖ��ǉ��Ȃ�ǉ�
            if (!_nearestTargetList.Contains(enemyRoot))
            {
                _nearestTargetList.Add(enemyRoot);
            }

            // �ł��߂��G���X�V
            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                _nearestTargetObj = hit.gameObject.transform.root.gameObject;
            }
        }
    }

    // ---------------------------- PublicMethod
    /// <summary>
    /// �T���ݒ���������B
    /// </summary>
    /// <param name="range">�T���͈�</param>
    /// <param name="layerName">�Ώۃ��C���[��</param>
    public void Initialize(float range, LayerMask layerMask)
    {
        _range = range;
        _layerMask = layerMask;
    }
}
