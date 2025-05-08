// �쐬���F   250508
// �X�V���F   250508
// �쐬�ҁF ���� ���l

// �T�v����(AI�ɂ��쐬)�F
// ���̃X�N���v�g�́A�ėp�I�Ȋm�F�_�C�A���O��UI�𐧌䂵�܂��B
// �\�����郁�b�Z�[�W�AYes/No�{�^���̃e�L�X�g�A�����Yes�{�^���������ꂽ�ۂ̏����́A
// �O���� ScriptableObject (CheckDialogConfig) ��ʂ��Đݒ肳��܂��B
// ����ɂ��A�Q�[�����̗l�X�Ȋm�F�V�[���ŋ��ʂ�UI�v���n�u���ė��p�ł��A
// �g��������ѕێ琫�����サ�܂��B

// �g���������F
// �X�^�[�g�֐��ŃN���X:CheckCanvasCtrl �̃A�N�Z�X���擾���A���\�b�h:ShowCheckCanvas ���A�C�ӂ̃{�^����AddListener���ĂˁB
// ���̔C�ӂ̃{�^��������2�ɓn���ƁA����Ƃ��ɃJ�[�\�����߂�܂��B

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CheckCanvasCtrl : MonoBehaviour
{
    // ---------------------------- SerializeField

    [SerializeField] private TextMeshProUGUI _checkText; // �m�F���b�Z�[�W�\���p�e�L�X�g
    [SerializeField] private Button _YesButton;       // �u�͂��v�{�^��
    [SerializeField] private Button _NoButton;        // �u�������v�{�^��

    // ---------------------------- Field

    private Canvas _checkCanvas;             // ����GameObject�ɃA�^�b�`���ꂽCanvas�R���|�[�l���g
    private TextMeshProUGUI _YesButtonText; // �u�͂��v�{�^���̎q�ɂ���e�L�X�g�R���|�[�l���g
    private TextMeshProUGUI _NoButtonText;  // �u�������v�{�^���̎q�ɂ���e�L�X�g�R���|�[�l���g

    private Button _lastSelectedButton;// �L�����Z�����Ƀt�H�[�J�X��߂���̃{�^�����ꎞ�I�ɕێ�
    private ICheckAction _currentAction;  // Yes/No�{�^���������ꂽ�ۂɎ��s���鏈�����`�����C���^�[�t�F�[�X�̃C���X�^���X

    // ---------------------------- UnityMessage

    private void Start()
    {
        _checkCanvas = GetComponent<Canvas>();
        _checkCanvas.enabled = false; // ������Ԃł͔�\��

        // �e�{�^���̎q����TextMeshProUGUI�R���|�[�l���g���擾
        _YesButtonText = _YesButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        _NoButtonText = _NoButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    // ---------------------------- Public

    /// <summary>
    /// �m�F�_�C�A���O��\�����܂��B
    /// </summary>
    /// <param name="config">Assets>01_Games>Scripts>uGUI>SO �ɂ�����̂��g���ĂˁB</param>
    /// <param name="focusOnCancel">�L�����Z�����Ƀt�H�[�J�X��߂��{�^�� (�C��)</param>
    public void ShowCheckCanvas(CheckDialogConfig config, Button focusOnCancel = null)
    {
        _checkText.text = config.message;             // �m�F���b�Z�[�W��ݒ�
        _YesButtonText.text = config.yesButtonText;   // �u�͂��v�{�^���̃e�L�X�g��ݒ�
        _NoButtonText.text = config.noButtonText;     // �u�������v�{�^���̃e�L�X�g��ݒ�
        _lastSelectedButton = focusOnCancel;        // �L�����Z�����Ƀt�H�[�J�X��߂��{�^����ۑ�

        // CheckDialogConfig�ɐݒ肳�ꂽ ICheckAction ���擾
        _currentAction = config.actionReference?.GetAction();

        // �u�͂��v�{�^���̃N���b�N���X�i�[��o�^�i�����̃��X�i�[���N���A���Ă���ǉ��j
        _YesButton.onClick.RemoveAllListeners();
        _YesButton.onClick.AddListener(() =>
        {
            _currentAction?.OnYes(); // �ݒ肳�ꂽYes�A�N�V���������s
            CloseCheckCanvas();      // �_�C�A���O�����
        });

        // �u�������v�{�^���̃N���b�N���X�i�[��o�^�i�����̃��X�i�[���N���A���Ă���ǉ��j
        _NoButton.onClick.RemoveAllListeners();
        _NoButton.onClick.AddListener(() =>
        {
            _currentAction?.OnNo();  // �ݒ肳�ꂽNo�A�N�V���������s
            CloseCheckCanvas();       // �_�C�A���O�����
        });

        _NoButton.Select();           // �����I�����u�������v�{�^���ɂ���
        _checkCanvas.enabled = true;  // Canvas��L���ɂ��ĕ\��
    }

    // ---------------------------- Private

    /// <summary>
    /// �m�F�_�C�A���O����܂��B
    /// </summary>
    private void CloseCheckCanvas()
    {
        _checkCanvas.enabled = false;           // Canvas�𖳌��ɂ��Ĕ�\��
        _YesButton.onClick.RemoveAllListeners(); // �u�͂��v�{�^���̃��X�i�[���N���A
        _NoButton.onClick.RemoveAllListeners();  // �u�������v�{�^���̃��X�i�[���N���A
        if (_lastSelectedButton != null)
        {
            _lastSelectedButton.Select();       // �L�����Z���O�ɑI������Ă����{�^���Ƀt�H�[�J�X��߂�
            _lastSelectedButton = null;        // �ꎞ�ϐ����N���A
        }
        _currentAction = null;                // ���݂̃A�N�V�������N���A
    }

}