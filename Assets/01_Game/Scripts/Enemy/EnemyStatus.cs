using R3;
using UnityEngine;

public class EnemyStatus : MonoBehaviour
{
    // ---------------------------- SerializeField
    [Header("�X�e�[�^�X")]
    [SerializeField, Tooltip("�̗�")] private float _maxHp;
    [SerializeField, Tooltip("�ړ����x")] private float _speed;


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

    private ReactiveProperty<ActionPattern> _currentAct;    // ���݂̍s��

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
    }

    private void OnTriggerEnter(Collider other)
    {
        // �_���[�W����
        if (other.CompareTag("PlayerAttack"))
        {
            _hp.Value--;
        }
    }


    // ---------------------------- PublicMethod
    /// <summary>
    /// �G�l�~�[�̃X�^�[�g����
    /// </summary>
    public void EnemyStart()
    {
        _currentAct.Value = ActionPattern.MOVE;
    }
}
