using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using R3;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Drop_View : MonoBehaviour
{
    // -----
    // -----SerializeField
    [SerializeField] private GameObject[] _moduleOptionObjects = new GameObject[3]; // �e���W���[���I�����̃��[�gGameObject
    [SerializeField] private TextMeshProUGUI _instructionsText; // �������e�L�X�g

    // -----Field
    private List<Button> _buttons = new List<Button>();
    private List<Detailed_View> _detailedViews = new List<Detailed_View>();
    private List<int> _currentDisplayedModuleIds = new List<int>(); // ���ݕ\�����Ă��郂�W���[����ID���X�g

    // -----Events (Presenter��R3�ōw�ǂ���)
    // ���[�U�[�����W���[����I�������ۂɁA�I�����ꂽ���W���[����ID��ʒm
    // UniRx.ISubject<int> �ł͂Ȃ� R3.Subject<int> ���g�p
    public Subject<int> OnModuleSelected { get; private set; } = new Subject<int>(); // R3.Subject ��������

    // -----UnityMessage
    private void Awake()
    {
        // �e�I�v�V�����I�u�W�F�N�g����Button��Detailed_View���擾���A������
        InitOptionViews();

    }

    // -----Private
    private void InitOptionViews()
    {
        _buttons.Clear();
        _detailedViews.Clear();

        for (int i = 0; i < _moduleOptionObjects.Length; i++)
        {
            GameObject obj = _moduleOptionObjects[i];
            if (obj == null)
            {
                Debug.LogError($"_moduleOptionObjects[{i}] is null. Please assign it in the Inspector.");
                continue;
            }

            Button button = obj.GetComponent<Button>();
            Detailed_View detailedView = obj.GetComponent<Detailed_View>();

            if (button == null) Debug.LogError($"Button component not found on _moduleOptionObjects[{i}].");
            if (detailedView == null) Debug.LogError($"Detailed_View component not found on _moduleOptionObjects[{i}].");

            if (button != null && detailedView != null)
            {
                _buttons.Add(button);
                _detailedViews.Add(detailedView);

                // �{�^���N���b�N�C�x���g��R3�ōw��
                int index = i; // �N���[�W���̂��߂ɃC���f�b�N�X���L���v�`��
                button.OnClickAsObservable()
                      .Subscribe(_ => OnOptionButtonClicked(index))
                      .AddTo(this); // �I�u�W�F�N�g�j�����ɍw�ǂ�����
            }
            //obj.SetActive(false); // �e�I�v�V�����������͔�\��
        }
    }

    /// <summary>
    /// �I�v�V�����{�^�����N���b�N���ꂽ�ۂ̃n���h���B
    /// </summary>
    /// <param name="index">�N���b�N���ꂽ�{�^���̃C���f�b�N�X�B</param>
    private void OnOptionButtonClicked(int index)
    {
        if (index < 0 || index >= _currentDisplayedModuleIds.Count)
        {
            //// ��փI�v�V�������N���b�N���ꂽ�ꍇ�̏����Ȃ�
            //if (_defaultOptionObject.activeSelf && index == _moduleOptionObjects.Length) // ��: 3�̃I�v�V�����̌��ɑ�փI�v�V����������ꍇ
            //{
            //    // �����ő�փI�v�V�������I�����ꂽ�ꍇ�̃��W�b�N�����s
            //    // ��: �o���l�l���A�R�C���l���Ȃǂ�Presenter�ɒʒm
            //    Debug.Log("Default option selected.");
            //    OnModuleSelected.OnNext(-1); // ����-1���փI�v�V������ID�Ƃ���
            //}
            Debug.LogWarning($"Invalid option index clicked: {index}");
            return;
        }

        int selectedModuleId = _currentDisplayedModuleIds[index];
        OnModuleSelected.OnNext(selectedModuleId); // �I�����ꂽ���W���[��ID���C�x���g�Ƃ��Ĕ���

    }

    // -----Public
    /// <summary>
    /// �h���b�v�I��UI��\�����܂��B
    /// Presenter����񋟂���郂�W���[���f�[�^�Ɋ�Â���UI���X�V���܂��B
    /// </summary>
    /// <param name="moduleDatas">�\�����郂�W���[���̃f�[�^���X�g�iModuleData��RuntimeModuleData�����������f�[�^�j�B</param>
    /// <param name="showDefaultOption">��փI�v�V������\�����邩�ǂ����B</param>
    public void Show(List<(ModuleData master, RuntimeModuleData runtime)> moduleDatas, bool showDefaultOption)
    {

        _currentDisplayedModuleIds.Clear(); // �\��ID���X�g���N���A

        //// �܂��S�ẴI�v�V�������\����
        //foreach (var obj in _moduleOptionObjects) obj.SetActive(false);
        //_defaultOptionObject.SetActive(false);


        // �n���ꂽ�f�[�^�Ɋ�Â��Ċe�I�v�V����UI��ݒ�
        for (int i = 0; i < moduleDatas.Count && i < _detailedViews.Count; i++)
        {
            var (master, runtime) = moduleDatas[i];
            if (master != null && runtime != null)
            {
                _moduleOptionObjects[i].SetActive(true);
                _detailedViews[i].SetInfo(master, runtime); // MasterData��RuntimeData�̗�����n��
                _currentDisplayedModuleIds.Add(master.Id); // �\�����̃��W���[��ID���L�^
            }
        }

        // ��փI�v�V�����̕\��
        if (showDefaultOption)
        {
            //_defaultOptionObject.SetActive(true);
            // �K�v�ł����_defaultOptionObject����Detailed_View��e�L�X�g��ݒ�
        }
    }

}