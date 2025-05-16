using R3;
using R3.Triggers;
using UnityEngine;

public class TestBullet : MonoBehaviour
{
    // ---------- Field
    private float _atk;
    private float _attackSpeed;
    private Vector3 _attackDir;
    private float _destroySecond = 20f;

    private void Start()
    {
        // ���R���Ŏ���
        Destroy(this, _destroySecond);

        // �ړ�
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                transform.Translate(_attackDir * _attackSpeed * Time.deltaTime);
            });

        // �Փˏ���
        this.OnTriggerEnterAsObservable()
            .Subscribe(collider =>
            {
                if(collider.TryGetComponent<IDamageble>(out var damageble))
                {
                    Debug.Log("�G�Ƀ_���[�W��^����");
                    damageble.TakeDamage(_atk);
                    Destroy(gameObject);
                }
            })
            .AddTo(this);
    }

    // ---------- Method
    public void Initialize(
        float atk,
        float attackSpeed,
        Vector3 attackDir)
    {
        _atk = atk;
        _attackSpeed = attackSpeed;
        _attackDir = attackDir;
    }
}
