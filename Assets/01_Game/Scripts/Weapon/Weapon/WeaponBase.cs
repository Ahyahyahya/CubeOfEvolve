using App.GameSystem.Modules;
using R3;
using R3.Triggers;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField, Tooltip("�f�[�^")] protected WeaponData _data;
    [SerializeField, Tooltip("�f�[�^")] protected int _id = -1;

    [Header("���G")]
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
            // ���x���A�b�v���̃X�e�[�^�X�ω�����
            RuntimeModuleManager.Instance.GetRuntimeModuleData(_id).Level
                .Subscribe(value =>
                {
                    _attack = _data.Attack * value;
                })
                    .AddTo(this);
        }
        if (transform.root.CompareTag("Enemy"))
        {
            _attack = _data.Attack;
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
        _layerSearch.Initialize(_data.SearchRange, _targetTag);
    }

    // ---------------------------- AbstractMethod
    protected abstract void Attack();
}
