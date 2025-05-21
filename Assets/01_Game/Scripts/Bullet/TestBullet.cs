using R3;
using R3.Triggers;
using UnityEngine;

public class TestBullet : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField, Tooltip("�U���Ώۂ̃^�O")] private string _targetTag;

    // ---------------------------- Field
    private float _atk;
    private float _attackSpeed;
    private Vector3 _attackDir;
    private float _destroySecond = 20f;

    // ---------------------------- UnityMessage
    public string TargetTag => _targetTag;

    // ---------------------------- UnityMessage
    private void Start()
    {
        // ���R���Ŏ���
        Destroy(gameObject, _destroySecond);

        // �ړ�
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                transform.Translate(_attackDir * _attackSpeed * Time.deltaTime);
            })
            .AddTo(this);

        // �Փˏ���
        this.OnTriggerEnterAsObservable()
            .Subscribe(collider =>
            {
                if (collider.transform.parent == null) return;

                if (collider.transform.parent.TryGetComponent<IDamageble>(out var damageble)
                    && collider.CompareTag(_targetTag))
                {
                    damageble.TakeDamage(_atk);
                    Destroy(gameObject);
                }
            })
            .AddTo(this);
    }

    // ---------------------------- PublicMethod
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
