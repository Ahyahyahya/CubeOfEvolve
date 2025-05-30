using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using R3;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �h���b�v�I����ʂ̃r���[��S������N���X�B
/// ���W���[���I�v�V�����̕\���AUI�̕\���E��\���A�I���{�^���N���b�N�C�x���g�̒ʒm���s���܂��B
/// </summary>
public class Drop_View : MonoBehaviour
{
    // ----- SerializedField (Unity Inspector�Őݒ�)
    [SerializeField] private GameObject[] _moduleOptionObjects = new GameObject[3]; // �e���W���[���I�����̃��[�gGameObject�B
    [SerializeField] private TextMeshProUGUI _instructionsText; // �������e�L�X�g�B

    // ----- Private Members (�����f�[�^)
    private List<Button> _buttons = new List<Button>(); // �e�I�v�V�����̃{�^�����X�g�B
    private List<Detailed_View> _detailedViews = new List<Detailed_View>(); // �e�I�v�V�����̏ڍו\���r���[���X�g�B
    private List<int> _currentDisplayedModuleIds = new List<int>(); // ���ݕ\�����Ă��郂�W���[����ID���X�g�B

    // ----- Events (Presenter��R3�ōw�ǂ���)
    public Subject<int> OnModuleSelected { get; private set; } = new Subject<int>(); // ���[�U�[�����W���[����I�������ۂɁA�I�����ꂽ���W���[����ID��ʒm����Subject�B

    // ----- MonoBehaviour Lifecycle (MonoBehaviour���C�t�T�C�N��)
    /// <summary>
    /// Awake�̓X�N���v�g�C���X�^���X�����[�h���ꂽ�Ƃ��ɌĂяo����܂��B
    /// �e�I�v�V����UI�̃R���|�[�l���g���擾���A�C�x���g���w�ǂ��܂��B
    /// </summary>
    private void Awake()
    {
        InitOptionViews();
    }

    // ----- Private Methods (��������)
    /// <summary>
    /// �e���W���[���I�v�V������View�R���|�[�l���g�����������A�{�^���C�x���g���w�ǂ��܂��B
    /// </summary>
    private void InitOptionViews()
    {
        _buttons.Clear();
        _detailedViews.Clear();

        for (int i = 0; i < _moduleOptionObjects.Length; i++)
        {
            GameObject obj = _moduleOptionObjects[i];
            if (obj == null)
            {
                Debug.LogError($"_moduleOptionObjects[{i}]��null�ł��BInspector�Ŋ��蓖�ĂĂ��������B");
                continue;
            }

            Button button = obj.GetComponent<Button>();
            Detailed_View detailedView = obj.GetComponent<Detailed_View>();

            if (button == null) Debug.LogError($"_moduleOptionObjects[{i}]��Button�R���|�[�l���g��������܂���B");
            if (detailedView == null) Debug.LogError($"_moduleOptionObjects[{i}]��Detailed_View�R���|�[�l���g��������܂���B");

            if (button != null && detailedView != null)
            {
                _buttons.Add(button);
                _detailedViews.Add(detailedView);

                // �{�^���N���b�N�C�x���g��R3�ōw��
                int index = i; // �N���[�W���̂��߂ɃC���f�b�N�X���L���v�`���B
                button.OnClickAsObservable()
                      .Subscribe(_ => OnOptionButtonClicked(index))
                      .AddTo(this); // �I�u�W�F�N�g�j�����ɍw�ǂ������B
            }
        }
    }

    /// <summary>
    /// �I�v�V�����{�^�����N���b�N���ꂽ�ۂ̃n���h���ł��B
    /// </summary>
    /// <param name="index">�N���b�N���ꂽ�{�^���̃C���f�b�N�X�B</param>
    private void OnOptionButtonClicked(int index)
    {
        if (index < 0 || index >= _currentDisplayedModuleIds.Count)
        {
            Debug.LogWarning($"�����ȃI�v�V�����C���f�b�N�X���N���b�N����܂���: {index}");
            return;
        }

        int selectedModuleId = _currentDisplayedModuleIds[index];
        OnModuleSelected.OnNext(selectedModuleId); // �I�����ꂽ���W���[��ID���C�x���g�Ƃ��Ĕ��΁B

        // UI���\���ɂ���Ȃǂ̌㏈�����K�v�ł���΁A�����ŌĂяo��
        //Hide();
    }

    // ----- Public Methods (Presenter����Ăяo�����)
    /// <summary>
    /// �h���b�v�I��UI��\�����܂��B
    /// Presenter����񋟂���郂�W���[���f�[�^�Ɋ�Â���UI���X�V���܂��B
    /// </summary>
    /// <param name="moduleDatas">�\�����郂�W���[���̃f�[�^���X�g�iModuleData��RuntimeModuleData�����������f�[�^�j�B</param>
    /// <param name="showDefaultOption">��փI�v�V������\�����邩�ǂ����B</param>
    public void Show(List<(ModuleData master, RuntimeModuleData runtime)> moduleDatas, bool showDefaultOption)
    {
        _currentDisplayedModuleIds.Clear(); // �\��ID���X�g���N���A�B

        // �n���ꂽ�f�[�^�Ɋ�Â��Ċe�I�v�V����UI��ݒ�
        for (int i = 0; i < moduleDatas.Count && i < _detailedViews.Count; i++)
        {
            var (master, runtime) = moduleDatas[i];
            if (master != null && runtime != null)
            {
                _moduleOptionObjects[i].SetActive(true);
                _detailedViews[i].SetInfo(master, runtime); // MasterData��RuntimeData�̗�����n���B
                _currentDisplayedModuleIds.Add(master.Id); // �\�����̃��W���[��ID���L�^�B
            }
        }

        // ��փI�v�V�����̕\�� (���݃R�����g�A�E�g����Ă��邽�߁A�K�v�ɉ����Ď���)
        if (showDefaultOption)
        {
            // �Ⴆ�΁A_moduleOptionObjects�̍Ō�̗v�f���փI�v�V�����Ƃ��Ďg�p����ꍇ
            // if (_moduleOptionObjects.Length > moduleDatas.Count)
            // {
            //     _moduleOptionObjects[moduleDatas.Count].SetActive(true);
            //     // ��փI�v�V�����̃e�L�X�g�Ȃǂ�ݒ�
            //     _instructionsText.text = "�A�b�v�O���[�h�\�ȃ��W���[��������܂���B����ɃR�C�����l�����܂��B";
            //     // ���̃{�^�����N���b�N���ꂽ�Ƃ���-1��OnModuleSelected�ɔ��s����悤�ɂ���
            //     _buttons[moduleDatas.Count].OnClickAsObservable()
            //         .Subscribe(_ => OnModuleSelected.OnNext(-1))
            //         .AddTo(this);
            // }
            Debug.Log("Drop_View: ��փI�v�V�����\���̃��W�b�N���L���ɂȂ�܂������AUI�̎����͂܂��ł��B");
            _instructionsText.text = "�A�b�v�O���[�h�\�ȃ��W���[��������܂���B";
        }
        else
        {
            // �ʏ�̎w���e�L�X�g��\��
            _instructionsText.text = "�A�b�v�O���[�h���郂�W���[����I�����Ă��������B";
        }

        // UI�S�̂�\��
        gameObject.SetActive(true);
    }

    /// <summary>
    /// �h���b�v�I��UI���\���ɂ��܂��B
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}