using Assets.AT;
using R3;
using R3.Triggers;
using UnityEngine;

public class Bullet_Beam : BulletBase
{
    // ---------------------------- SerializeField
    [SerializeField] private GameObject _hitEffect;

    // ---------------------------- Field
    private float _distance;        // ����
    private float _radius;          // ���a
    private Vector3 _direction;     // ����
    private float _attackInterval;        // �U���Ԋu
    private float _currentInterval; // �U���Ԋu�̌o�ߎ���

    // ---------------------------- UnityMessage
    private void Start()
    {
        // ���R���Ŏ���
        Destroy(gameObject, _destroySecond);

        // ���t���[���Ď�
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                // �Q�[����Ԃ��o�g�����łȂ���Ώ������Ȃ�
                if (GameManager.Instance.CurrentGameState.CurrentValue
                    != Assets.IGC2025.Scripts.GameManagers.GameState.BATTLE)
                {
                    return;
                }

                if (_currentInterval < _attackInterval)
                {
                    _currentInterval += Time.deltaTime;
                    return;
                }
                else
                {
                    // �C���^�[�o�������Z�b�g
                    _currentInterval = 0f;
                }

                Vector3 beamDir = transform.forward; // �e�������Ă������
                Ray ray = new Ray(transform.position, beamDir);

                Debug.DrawRay(transform.position, beamDir * _distance, Color.red);

                if (Physics.SphereCast(ray, _radius, out RaycastHit hit, _distance, _targetLayerMask))
                {
                    GameObject rootObj = hit.collider.transform.root.gameObject;

                    if ((_targetLayerMask.value & (1 << rootObj.layer)) != 0 &&
                        rootObj.TryGetComponent<IDamageble>(out var damageble))
                    {
                        damageble.TakeDamage(_attack);
                    }

                    // �q�b�g�G�t�F�N�g����
                    GameSoundManager.Instance.PlaySFX(_hitSEName, transform, _hitSEName);
                    var effect = Instantiate(_hitEffect, hit.point, Quaternion.identity);
                    effect.AddComponent<StopEffect>();
                }
            })
            .AddTo(this);
    }

    // ---------------------------- PublicMethod
    public void Initialize(
        LayerMask layerMask,
        float attack,
        float distance,
        float radius,
        Vector3 direction,
        float destroySecond,
        float interval)
    {
        _targetLayerMask = layerMask;
        _attack = attack;
        _distance = distance;
        _radius = radius;
        _direction = direction;
        _destroySecond = destroySecond;
        _attackInterval = interval;
    }
}
