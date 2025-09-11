using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using Game.Utils;
using ObservableCollections;
using R3;
using R3.Triggers;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour, IModuleID
{
    // ---------------------------- SerializeField
    [Header("�f�[�^")]
    [SerializeField, Tooltip("����̊�{�f�[�^")]
    protected ModuleData _data;

    [Header("��")]
    [SerializeField, Tooltip("���ˎ���SE��")]
    protected string _fireSEName;

    [Header("�ǔ�")]
    [SerializeField, Tooltip("�ǔ����x")]
    private float _rotationSpeed = 90f;

    [SerializeField, Tooltip("������ǔ����邩")]
    protected bool _isHorizontalAxisTracking = true;
    [SerializeField, Tooltip("���탂�f��")]
    protected GameObject _horizontalModel;

    [SerializeField, Tooltip("�c����ǔ����邩")]
    protected bool _isVerticalAxisTracking = true;
    [SerializeField, Tooltip("���탂�f��")]
    protected GameObject _verticalModel;

    [Header("���G")]
    [SerializeField, Tooltip("���G�p�R���|�[�l���g")]
    protected LayerSearch _layerSearch;
    [SerializeField, Tooltip("�U���Ώۃ��C���[�}�X�N")]
    protected LayerMask _targetLayerMask;

    [Header("�G�̏ꍇ")]
    [SerializeField, Tooltip("�G�����������ꍇ�̍U���͔{��")]
    private float _enemyRate = 1;

    // ---------------------------- Field
    protected float _attackStatusEffects;   // �X�e�[�^�X���ʂɂ��ǉ��U����
    protected float _currentAttack;         // ���ۂ̍U����
    protected float _currentInterval;       // �U���Ԋu�̌o�ߎ���

    // ---------------------------- Property
    /// <summary>
    /// ����̃��W���[��ID
    /// </summary>
    public int Id => _data.Id;

    // ---------------------------- UnityMessage
    private void Start()
    {
        // ������
        Initialize();

        // ���x���A�b�v��X�e�[�^�X���ʂ��Ď�
        ObserveLevel();
        ObserveStatusEffects();

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

                ProcessingFaceEnemyOrientation();

                // �U���Ԋu�̌o�ߎ��Ԃ����Z
                if (_currentInterval < _data.ModuleState.Interval)
                {
                    _currentInterval += Time.deltaTime;
                }
                else if (!_isHorizontalAxisTracking && !_isVerticalAxisTracking)
                {
                    // �U�������i�h���N���X�Ŏ����j
                    Attack();

                    _currentInterval = 0f;
                }
                // �U���\��ԂɂȂ�A�^�[�Q�b�g�����݂���ꍇ
                else if (_layerSearch.NearestTargetObj != null)
                {
                    // �U�������i�h���N���X�Ŏ����j
                    Attack();

                    // �C���^�[�o�������Z�b�g
                    _currentInterval = 0f;
                }
            })
            .AddTo(this);

        // �����U���͂��v�Z
        UpdateAttackStatus();
    }

    // ---------------------------- Initialization
    /// <summary>
    /// ����������
    /// </summary>
    protected virtual void Initialize()
    {
        _layerSearch.Initialize(_data.ModuleState.SearchRange, _targetLayerMask);
    }

    /// <summary>
    /// ���x���̊Ď�����
    /// </summary>
    private void ObserveLevel()
    {
        if (transform.root.CompareTag("Cube"))
        {
            // Cube�̏ꍇ�̓����^�C���̃��W���[���f�[�^���Ď�
            RuntimeModuleManager.Instance.GetRuntimeModuleData(_data.Id).Level
                .Subscribe(level =>
                {
                    UpdateAttackStatus();
                })
                .AddTo(this);
        }
        else if (transform.root.CompareTag("Enemy"))
        {
            // �G�̏ꍇ�͔{�����������Œ�l���g�p
            _currentAttack = _data.ModuleState.Attack * _enemyRate;
        }
    }

    /// <summary>
    /// �X�e�[�^�X���ʂ̊Ď�
    /// </summary>
    private void ObserveStatusEffects()
    {
        if (!transform.root.CompareTag("Cube")) return;

        // ���ʒǉ��E�폜�̊Ď��X�g���[��
        var addStream = RuntimeModuleManager.Instance.CurrentCurrentStatusEffectList
            .ObserveAdd(destroyCancellationToken)
            .Select(_ => Unit.Default);

        var removeStream = RuntimeModuleManager.Instance.CurrentCurrentStatusEffectList
            .ObserveRemove(destroyCancellationToken)
            .Select(_ => Unit.Default);

        // �ǂ��炩������������U���͂��X�V
        addStream.Merge(removeStream)
            .Subscribe(_ =>
            {
                UpdateAttackStatus();
            })
            .AddTo(this);
    }

    /// <summary>
    /// �U���͂��v�Z�E�X�V  
    /// ���x�������ƃX�e�[�^�X���ʂ��l�����Čv�Z����
    /// </summary>
    private void UpdateAttackStatus()
    {
        if (!transform.root.CompareTag("Cube")) return;

        // �X�e�[�^�X���ʂɂ��ǉ��U���͂����Z�b�g
        _attackStatusEffects = 0;

        // ���݂̑S�X�e�[�^�X���ʂ����Z
        foreach (var effect in RuntimeModuleManager.Instance.CurrentCurrentStatusEffectList)
        {
            _attackStatusEffects += effect.Attack;
        }

        // ���x�����擾
        var level = RuntimeModuleManager.Instance.GetRuntimeModuleData(_data.Id).Level.CurrentValue;

        // ��{�U���͂����x���ɉ����Čv�Z�i�ő�50%�̐����j
        _currentAttack = StateValueCalculator.CalcStateValue(
            baseValue: _data.ModuleState.Attack,
            currentLevel: level,
            maxLevel: 5,
            maxRate: 0.5f
        );

        // �X�e�[�^�X���ʂ𔽉f�i%���Z�j
        _currentAttack *= 1f + (_attackStatusEffects / 100);
    }

    /// <summary>
    /// �^�[�Q�b�g�̕����Ƀ��f������]������i�����E�����ʐ���j
    /// </summary>
    private void ProcessingFaceEnemyOrientation()
    {
        if (!_isHorizontalAxisTracking && !_isVerticalAxisTracking) return;
        if (_layerSearch.NearestTargetObj == null) return;

        var target = _layerSearch.NearestTargetObj.transform;

        // ��������
        if (_isHorizontalAxisTracking && _horizontalModel != null)
        {
            Vector3 lookDir = target.position - _horizontalModel.transform.position;
            lookDir.y = 0f; // �����͖������Đ����x�N�g������

            if (lookDir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);
                _horizontalModel.transform.rotation = Quaternion.RotateTowards(
                    _horizontalModel.transform.rotation,
                    targetRot,
                    _rotationSpeed * Time.deltaTime
                );
            }
        }

        // ��������
        if (_isVerticalAxisTracking && _verticalModel != null)
        {
            Vector3 lookDir = target.position - _verticalModel.transform.position;

            if (lookDir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);

                // ���݂̉�]
                Quaternion current = _verticalModel.transform.rotation;

                // Pitch�����ڕW�l�ɍX�V���AYaw��Roll�͈ێ�
                Quaternion newRot = Quaternion.Euler(
                    targetRot.eulerAngles.x, // Pitch
                    current.eulerAngles.y,   // Yaw�ێ�
                    current.eulerAngles.z    // Roll�ێ�
                );

                _verticalModel.transform.rotation = Quaternion.RotateTowards(
                    current,
                    newRot,
                    _rotationSpeed * Time.deltaTime
                );
            }
        }
    }


    // ---------------------------- Abstract Method
    /// <summary>
    /// �U������
    /// </summary>
    protected abstract void Attack();
}
