using App.GameSystem.Modules;
using Assets.IGC2025.Scripts.Presenter;
using Assets.IGC2025.Scripts.View;
using Game.Utils;
using ObservableCollections;
using R3;
using R3.Triggers;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    // ---------------------------- SerializeField
    [Header("�f�[�^")]
    [SerializeField, Tooltip("�f�[�^")] protected WeaponData _data;
    [SerializeField, Tooltip("�f�[�^ID")] protected int _id = -1;

    [Header("��")]
    [SerializeField, Tooltip("SE")] protected string _fireSEName;

    [Header("���G")]
    [SerializeField, Tooltip("�Ώی��m�p")] protected LayerSearch _layerSearch;
    [SerializeField, Tooltip("�U���Ώۂ̃^�O")] protected string _targetTag;

    [Header("�U����")]
    [SerializeField, Tooltip("�ő僌�x��")] private int _maxLevel = 5;

    [Header("�G�̏ꍇ")]
    [SerializeField, Tooltip("�U���͔{��")] private float _enemyRate = 1;

    // ---------------------------- Field
    protected float _attackStatusEffects;
    protected float _currentAttack;
    protected float _currentInterval;

    // ---------------------------- Property
    /// <summary>
    /// ID
    /// </summary>
    public int Id => _id;

    // ---------------------------- Unity Method
    private void Start()
    {
        Initialize();

        ObserveLevel();
        ObserveStatusEffects();

        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                if (_currentInterval < _data.Interval)
                {
                    _currentInterval += Time.deltaTime;
                }
                else if (_layerSearch.NearestTargetObj != null)
                {
                    Attack();
                    _currentInterval = 0f;
                }
            })
            .AddTo(this);

        // �U���͍X�V
        UpdateAttackStatus();
    }

    // ---------------------------- Initialization
    /// <summary>
    /// ������
    /// </summary>
    protected virtual void Initialize()
    {
        _layerSearch.Initialize(_data.SearchRange, _targetTag);
    }

    /// <summary>
    /// ���x�����Ď�
    /// </summary>
    private void ObserveLevel()
    {
        if (transform.root.CompareTag("Cube"))
        {
            RuntimeModuleManager.Instance.GetRuntimeModuleData(_id).Level
                .Subscribe(level =>
                {
                    UpdateAttackStatus();
                })
                .AddTo(this);
        }
        else if (transform.root.CompareTag("Enemy"))
        {
            _currentAttack = _data.Attack * _enemyRate;
        }
    }

    /// <summary>
    /// �I�v�V�������Ď�
    /// </summary>
    private void ObserveStatusEffects()
    {
        if (!transform.root.CompareTag("Cube")) return;

        var addStream = RuntimeModuleManager.Instance.CurrentCurrentStatusEffectList
        .ObserveAdd(destroyCancellationToken)
        .Select(_ => Unit.Default);

        var removeStream = RuntimeModuleManager.Instance.CurrentCurrentStatusEffectList
            .ObserveRemove(destroyCancellationToken)
            .Select(_ => Unit.Default);

        // �ǂ��炩�̃C�x���g���������������Ď�
        addStream.Merge(removeStream)
            .Subscribe(_ =>
            {
                UpdateAttackStatus();
            })
            .AddTo(this);
    }

    /// <summary>
    /// �U���͍X�V
    /// </summary>
    private void UpdateAttackStatus()
    {
        if (!transform.root.CompareTag("Cube")) return;

        _attackStatusEffects = 0;

        foreach (var effect in RuntimeModuleManager.Instance.CurrentCurrentStatusEffectList)
        {
            _attackStatusEffects += effect.Attack;
        }

        var level = RuntimeModuleManager.Instance.GetRuntimeModuleData(_id).Level.CurrentValue;

        // �U���͌v�Z
        _currentAttack = StateValueCalculator.CalcStateValue(
                baseValue: _data.Attack,
                currentLevel: level,
                maxLevel: 5,
                maxRate: 0.5f // �ő�+50%�̐���
            ) + _attackStatusEffects;
    }

    // ---------------------------- Abstract Method
    protected abstract void Attack();
}
