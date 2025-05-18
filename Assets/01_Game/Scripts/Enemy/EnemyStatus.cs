using R3;
using UnityEngine;

public class EnemyStatus : MonoBehaviour, IDamageble
{
    // ---------------------------- SerializeField
    [Header("�X�e�[�^�X")]
    [SerializeField, Tooltip("�ړ����x")] private float _speed;
    [SerializeField, Tooltip("�̗�")] private float _maxHp;
    [SerializeField, Tooltip("�o���l")] private int _dropExp;
    [SerializeField, Tooltip("����")] private int _dropMoney;

    // ---------------------------- Field
    public enum ActionPattern                              // �s���p�^�[��
    {
        IDLE,           // �ҋ@
        MOVE,           // �ړ�
        WAIT,           // �s������U��~
        ATTACK,         // ��~���čU��
        MOVEANDATTACK,  // �ړ����Ȃ���U��
        AVOID,          // ���
    }

    private ReactiveProperty<ActionPattern> _currentAct = new();    // ���݂̍s��

    private ReactiveProperty<float> _hp = new();            // �̗�


    // ---------------------------- Property
    public ReadOnlyReactiveProperty<ActionPattern> CurrentAct => _currentAct;
    public float MaxHp => _maxHp;
    public ReadOnlyReactiveProperty<float> Hp => _hp;
    public float Speed => _speed;


    // ---------------------------- UnityMessage
    private void Awake()
    {
        _currentAct.Value = ActionPattern.WAIT;

        _hp.Value = _maxHp;

        // ���̔���
        _hp.Where(value => value <= 0)
            .Subscribe(value =>
            {
                ItemDrop.Instance.DropExp(transform.position, _dropExp);
                ItemDrop.Instance.DropMoney(transform.position, _dropMoney);

                Destroy(gameObject);
            })
            .AddTo(this);
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.D))
        {
            TakeDamage(1);
        }
    }

    // ---------------------------- PublicMethod
    /// <summary>
    /// �G�l�~�[����������鏈��
    /// </summary>
    public void EnemySpawn()
    {
        _currentAct.Value = ActionPattern.MOVE;
    }


    // ---------------------------- Interface
    public void TakeDamage(float damage)
    {
        _hp.Value -= damage;
    }
}
