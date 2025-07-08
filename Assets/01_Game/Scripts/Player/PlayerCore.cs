using Assets.IGC2025.Scripts.GameManagers;
using R3;
using System;
using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class PlayerCore : MonoBehaviour, IDamageble
{
    // ---------- RP
    [SerializeField, Tooltip("HP")] private SerializableReactiveProperty<float> _hp;
    [SerializeField, Tooltip("�ő�HP")] private SerializableReactiveProperty<float> _maxHp;
    [SerializeField, Tooltip("���G����")] private float _invincibleTime = 1.0f;
    [SerializeField, Tooltip("�ړ����x")] private SerializableReactiveProperty<float> _moveSpeed;
    [SerializeField, Tooltip("��]���x")] private SerializableReactiveProperty<float> _rotateSpeed;
    [SerializeField, Tooltip("���x��")] private SerializableReactiveProperty<int> _level;
    [SerializeField, Tooltip("�o���l")] private SerializableReactiveProperty<float> _exp;
    [SerializeField, Tooltip("�K�v�o���l")] private SerializableReactiveProperty<float> _requireExp;
    [SerializeField, Tooltip("�K�v�o���l�̑�����(�ݏ�)")] private float _AddRequireExpAmount;
    [SerializeField, Tooltip("�L���[�u��")] private SerializableReactiveProperty<int> _cubeCount;
    [SerializeField, Tooltip("�ő�L���[�u��")] private SerializableReactiveProperty<int> _maxCubeCount;
    [SerializeField, Tooltip("����")] private SerializableReactiveProperty<int> _money;
    [SerializeField, Tooltip("�e�X�g�X�L��")] private SerializableReactiveProperty<BaseSkill> _skill;

    public ReadOnlyReactiveProperty<float> Hp => _hp;
    public ReadOnlyReactiveProperty<float> MaxHp => _maxHp;
    public ReadOnlyReactiveProperty<float> MoveSpeed => _moveSpeed;
    public ReadOnlyReactiveProperty<float> RotateSpeed => _rotateSpeed;
    public ReadOnlyReactiveProperty<int> Level => _level;
    public ReadOnlyReactiveProperty<float> Exp => _exp;
    public ReadOnlyReactiveProperty<float> RequireExp => _requireExp;
    public ReadOnlyReactiveProperty<int> CubeCount => _cubeCount;
    public ReadOnlyReactiveProperty<int> MaxCubeCount => _maxCubeCount;
    public ReadOnlyReactiveProperty<int> Money => _money;
    public ReadOnlyReactiveProperty<BaseSkill> Skill => _skill;

    // ---------- Field
    private int _prevCubeCount;

    // ���G�t���O
    private bool _isInvincibled;

    // ---------- UnityMessage
    private void Start()
    {
        // �o���l����
        _exp
            .Where(x => x >= _requireExp.Value)
            .Subscribe(x =>
            {
                // ���Z�b�g���A�]��o���l�����Z
                _exp.Value = x - _requireExp.Value;

                // ���x�����グ��
                _level.Value++;
            })
            .AddTo(this);

        // ���x������(��)
        _level
            .Skip(1)
            .Subscribe(x =>
            {
                _maxCubeCount.Value += 3;
                _requireExp.Value += _AddRequireExpAmount;
                _AddRequireExpAmount *= 2;
            })
            .AddTo(this);

        // �O��̃L���[�u�̒l��������
        _prevCubeCount = _cubeCount.Value;

        // �L���[�u���̏���
        _cubeCount
            .Subscribe(x =>
            {
                // ��������
                if (x > _prevCubeCount)
                {
                    _maxHp.Value += 10;
                    _hp.Value += 10;
                }
                // ��������
                else if (x < _prevCubeCount)
                {
                    _maxHp.Value -= 10;
                    _hp.Value -= 10;
                }

                _prevCubeCount = x;
            })
            .AddTo(this);

        // HP�֘A����
        _hp
            .Where(x => x <= 0)
            .Take(1)
            .Subscribe(x =>
            {
                GameManager.Instance.ChangeGameState(GameState.GAMEOVER);
            })
            .AddTo(this);
    }

    // ---------- Interface
    public void TakeDamage(float damage)
    {
        // ���G���Ԓ��͏������Ȃ�
        if (_isInvincibled) return;

        // HP�����炷
        _hp.Value -= damage;

        // ���G�t���O�I��
        _isInvincibled = true;

        // ��e�������莞�Ԗ��G
        StartCoroutine(StartInvincible());
    }

    // ---------- Method
    public void ReceiveMoney(int amount)
    {
        _money.Value += amount;
    }
    public void PayMoney(int amount)
    {
        _money.Value -= amount;
    }
    public void AddCube()
    {
        _cubeCount.Value++;
    }
    public void RemoveCube()
    {
        _cubeCount.Value--;
    }
    public void ReceiveExp(int amount)
    {
        _exp.Value += amount;
    }
    public void RecoveryHp(int amount)
    {
        // HP�̍ő�l�𒴂��Ȃ��悤�ɉ��Z
        _hp.Value = Mathf.Min(_hp.Value + amount, _maxHp.Value);
    }

    /// <summary>
    /// ��莞�Ԍ�ɖ��G����������
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartInvincible()
    {
        yield return new WaitForSeconds(_invincibleTime);

        _isInvincibled = false;
    }
}
