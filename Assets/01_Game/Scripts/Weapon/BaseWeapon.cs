using Assets.IGC2025.Scripts.GameManagers;
using R3;
using R3.Triggers;
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public abstract class BaseWeapon : MonoBehaviour
{
    // ---------- SerializeField
    [SerializeField, Tooltip("�U����")] protected float atk;
    [SerializeField, Tooltip("�U�����x")] protected float attackSpeed;
    [SerializeField, Tooltip("�U���͈�")] protected float range;
    [SerializeField, Tooltip("�U���Ԋu")] protected float interval;
    [SerializeField, Tooltip("�Ώی��m�p")] protected SphereCollider sphereCollider;

    // ---------- Field
    protected float currentInterval;
    protected List<Transform> inRangeEnemies = new();
    protected Transform nearestEnemyTransform;

    // ---------- UnityMethod
    private void Start()
    {
        sphereCollider.radius = range;

        this.OnTriggerEnterAsObservable()
            .Where(x => x.CompareTag("Enemy"))
            .Subscribe(x =>
            {
                inRangeEnemies.Add(x.transform);
            });

        this.OnTriggerExitAsObservable()
            .Where(x => x.CompareTag("Enemy"))
            .Subscribe(x =>
            {
                inRangeEnemies[inRangeEnemies.IndexOf(x.transform)] = null;
            });


        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                var nearestEnemyDist = 0f;

                // ��ԋ߂��G���擾
                foreach (var enemyTransform in inRangeEnemies)
                {
                    if (enemyTransform == null) continue;

                    var dist = Vector3.Distance(
                        transform.position,
                        enemyTransform.position);

                    if (nearestEnemyDist == 0f || dist < nearestEnemyDist)
                    {
                        nearestEnemyDist = dist;
                        nearestEnemyTransform = enemyTransform;
                    }
                }

                // �͈͊O(null)�ɂȂ����v�f������
                if (inRangeEnemies.Count > 0)
                {
                    inRangeEnemies.RemoveAll(x => x == null);
                }

                // �C���^�[�o�����Ȃ�
                if (currentInterval < interval)
                {
                    currentInterval += Time.deltaTime;
                }
                // �C���^�[�o���I�����G��������
                else
                {
                    if (inRangeEnemies.Count <= 0) return;

                    Debug.Log("��ԋ߂��G" + nearestEnemyTransform.gameObject.name);

                    Attack();
                    currentInterval = 0f;
                }
            });
    }

    // ---------- AbstractMethod
    protected abstract void Attack();
}
