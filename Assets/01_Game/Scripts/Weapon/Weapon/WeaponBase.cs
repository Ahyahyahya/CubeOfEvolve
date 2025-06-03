using R3;
using R3.Triggers;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField, Tooltip("�f�[�^")] protected WeaponData _data;

    [Header("���G")]
    [SerializeField, Tooltip("���G�͈�")] protected float _scoutingRange;
    [SerializeField, Tooltip("�Ώی��m�p")] protected LayerSearch _layerSearch;

    [SerializeField, Tooltip("�U���Ώۂ̃^�O")] protected string _targetTag;

    // ---------------------------- Field
    protected float _attack;
    protected float _currentInterval;

    // ---------------------------- UnityMethod
    private void Start()
    {
        if (transform.root.CompareTag("Player"))
        {
            foreach (var i in WeaponLevelManager.Instance.PlayerWeaponLevels)
            {
                i.Subscribe(value =>
                {
                    _attack = _data.Attack * value;
                })
                    .AddTo(this);
            }
        }
        if (transform.root.CompareTag("Enemy"))
        {
            foreach (var level in WeaponLevelManager.Instance.EnemyWeaponLevels)
            {
                level.Subscribe(value =>
                {
                    _attack = _data.Attack * value;
                })
                    .AddTo(this);
            }
        }

        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                // �C���^�[�o�����Ȃ�
                if (_currentInterval < _data.Interval)
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
