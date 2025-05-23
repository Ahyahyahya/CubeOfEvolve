using R3;
using R3.Triggers;
using UnityEngine;

public class Bullet_Missile : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField, Tooltip("�U���Ώۂ̃^�O")] private string _targetTag;

    // ---------------------------- Field
    private float _atk;
    private float _attackSpeed;
    private float _destroySecond = 10f;

    private Vector3 _velocity;  // ����
    private Vector3 _position;
    private Transform _target;  // �ړI�n
    private float _period;      // �ړ�����

    // ---------------------------- UnityMessage
    public string TargetTag => _targetTag;

    // ---------------------------- UnityMessage
    private void Start()
    {
        // ���R���Ŏ���
        Destroy(gameObject, _destroySecond);

        _position = transform.position;

        // �ړ�
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                // ����
                var dis = _target.position - transform.position;

                var accelerator = Vector3.zero;
                accelerator += (dis - _velocity * _period) * 2f / (_period * _period);

                if (accelerator.magnitude > 50f)
                {
                    accelerator = accelerator.normalized * 50f;
                }

                _period -= Time.deltaTime;

                _velocity += accelerator * Time.deltaTime;
                _position += _velocity * Time.deltaTime;
                transform.position = _position;
            })
            .AddTo(this);

        // �Փˏ���
        this.OnTriggerEnterAsObservable()
            .Subscribe(other =>
            {
                if (other.transform.root.TryGetComponent<IDamageble>(out var damageble)
                && other.CompareTag(_targetTag))
                {
                    damageble.TakeDamage(_atk);
                    Destroy(gameObject);
                }

                if (other.CompareTag("Ground"))
                {
                    Destroy(gameObject);
                }
            })
            .AddTo(this);
    }

    // ---------------------------- PublicMethod
    public void Initialize(
        float atk,
        float attackSpeed,
        Vector3 attackDir,
        Transform target,
        float period)
    {
        _atk = atk;
        _attackSpeed = attackSpeed;
        _velocity = attackDir;
        _target = target;
        _period = period;
    }
}
