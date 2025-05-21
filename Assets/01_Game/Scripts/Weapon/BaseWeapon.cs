using R3;
using R3.Triggers;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public abstract class BaseWeapon : MonoBehaviour
{
    // ---------- SerializeField
    [SerializeField, Tooltip("�U����")] protected float _atk;
    [SerializeField, Tooltip("�U�����x")] protected float _attackSpeed;
    [SerializeField, Tooltip("�U���͈�")] protected float _range;
    [SerializeField, Tooltip("�U���Ԋu")] protected float _interval;
    [SerializeField, Tooltip("�Ώی��m�p")] protected SphereCollider _sphereCollider;

    [SerializeField, Tooltip("�U���Ώۂ̃^�O")] private string _targetTag;

    // ---------- Field
    protected float _currentInterval;
    protected List<Transform> _inRangeEnemies = new();
    protected Transform _nearestEnemyTransform;

    // ---------- UnityMethod
    private void Start()
    {
        _sphereCollider.radius = _range;

        this.OnTriggerEnterAsObservable()
            .Where(x => x.CompareTag(_targetTag))
            .Subscribe(x =>
            {
                _inRangeEnemies.Add(x.transform);
            })
            .AddTo(this);

        this.OnTriggerExitAsObservable()
            .Where(x => x.CompareTag(_targetTag))
            .Subscribe(x =>
            {
                _inRangeEnemies[_inRangeEnemies.IndexOf(x.transform)] = null;
            })
            .AddTo(this);


        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                var nearestEnemyDist = 0f;

                // ��ԋ߂��G���擾
                foreach (var enemyTransform in _inRangeEnemies)
                {
                    if (enemyTransform == null) continue;

                    var dist = Vector3.Distance(
                        transform.position,
                        enemyTransform.position);

                    if (nearestEnemyDist == 0f || dist < nearestEnemyDist)
                    {
                        nearestEnemyDist = dist;
                        _nearestEnemyTransform = enemyTransform;
                    }
                }

                // �͈͊O(null)�ɂȂ����v�f������
                if (_inRangeEnemies.Count > 0)
                {
                    _inRangeEnemies.RemoveAll(x => x == null);
                }

                // �C���^�[�o�����Ȃ�
                if (_currentInterval < _interval)
                {
                    _currentInterval += Time.deltaTime;
                }
                // �C���^�[�o���I�����G��������
                else
                {
                    if (_inRangeEnemies.Count <= 0) return;

                    Attack();
                    _currentInterval = 0f;
                }
            })
            .AddTo(this);
    }

    // ---------- AbstractMethod
    protected abstract void Attack();
}
