using R3;
using R3.Triggers;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField, Tooltip("�f�[�^")] protected WeaponDataBase _data;

    [SerializeField, Tooltip("ID")] private int _id;

    [SerializeField, Tooltip("�e��")] protected float _bulletSpeed;
    [SerializeField, Tooltip("�U���Ԋu")] protected float _interval;

    [Header("���G")]
    [SerializeField, Tooltip("���G�͈�")] protected float _scoutingRange;
    [SerializeField, Tooltip("�Ώی��m�p")] protected LayerSearch _layerSearch;

    [SerializeField, Tooltip("�U���Ώۂ̃^�O")] protected string _targetTag;

    // ---------------------------- Field
    protected float _attack;
    protected float _currentInterval;
    protected Transform _nearestEnemyTransform;

    // ---------------------------- UnityMethod
    private void Start()
    {
        _data.weaponDataList[_id].Level.
            Subscribe(value =>
            {
                _attack = _data.weaponDataList[_id].Attack * _data.weaponDataList[_id].Level.CurrentValue;
            }).
            AddTo(this);

        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                // �C���^�[�o�����Ȃ�
                if (_currentInterval < _interval)
                {
                    _currentInterval += Time.deltaTime;
                }
                // �C���^�[�o���I�����G��������
                else if (_layerSearch.NearestEnemyObj != null)
                {
                    Attack();
                    _currentInterval = 0f;
                }
            })
            .AddTo(this);

        Initialize();
    }

    // ---------------------------- AbstractMethod
    protected virtual void Initialize()
    {

    }

    // ---------------------------- AbstractMethod
    protected abstract void Attack();
}
