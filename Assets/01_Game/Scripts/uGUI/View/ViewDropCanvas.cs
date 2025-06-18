using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using R3;
using R3.Triggers;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.IGC2025.Scripts.View
{
    public class ViewDropCanvas : MonoBehaviour
    {
        // ----- SerializedField (Unity Inspector�Őݒ�)
        [SerializeField] private GameObject[] _moduleOptionObjects = new GameObject[3]; // �e���W���[���I�����̃��[�gGameObject�B

        // ----- Private Members (�����f�[�^)
        private List<Button> _buttons = new List<Button>(); // �e�I�v�V�����̃{�^�����X�g�B
        private List<ViewInfo> _detailedViews = new List<ViewInfo>(); // �e�I�v�V�����̏ڍו\���r���[���X�g�B
        private List<int> _currentDisplayedModuleIds = new List<int>(); // ���ݕ\�����Ă��郂�W���[����ID���X�g�B

        // R3�̍w�ǂ��Ǘ����邽�߂�CompositeDisposable
        private CompositeDisposable _disposables = new CompositeDisposable();

        // ----- Events (Presenter��R3�ōw�ǂ���)
        public Subject<int> OnModuleSelected { get; private set; } = new Subject<int>(); // ���[�U�[�����W���[����I�������ۂɁA�I�����ꂽ���W���[����ID��ʒm����Subject�B
        public Subject<int> OnModuleHovered { get; private set; } = new Subject<int>(); // �J�[�\�������킹�����W���[����ID��ʒm����Subject�B

        // ----- UnityMessage

        private void Awake()
        {
            InitializeOptionComponents();
        }

        private void OnDestroy()
        {
            _disposables.Dispose(); // �I�u�W�F�N�g���j�������ۂɁA���ׂĂ̍w�ǂ�����
            OnModuleSelected.Dispose(); // Subject���Y�ꂸ��Dispose
            OnModuleHovered.Dispose();  // Subject���Y�ꂸ��Dispose
        }

        // ----- Private Methods (��������)
        /// <summary>
        /// �e���W���[���I�v�V������View�R���|�[�l���g���擾���܂��B
        /// </summary>
        private void InitializeOptionComponents()
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
                ViewInfo detailedView = obj.GetComponent<ViewInfo>();

                if (button == null) Debug.LogError($"_moduleOptionObjects[{i}]��Button�R���|�[�l���g��������܂���B");
                if (detailedView == null) Debug.LogError($"_moduleOptionObjects[{i}]��Detailed_View�R���|�[�l���g��������܂���B");

                if (button != null && detailedView != null)
                {
                    _buttons.Add(button);
                    _detailedViews.Add(detailedView);
                }
            }
        }

        /// <summary>
        /// ���W���[����I�������ۂ̃n���h���B
        /// </summary>
        /// <param name="index">�N���b�N���ꂽ�{�^���̃C���f�b�N�X�B</param>
        private void OnButtonClicked(int index)
        {
            if (index < 0 || index >= _currentDisplayedModuleIds.Count)
            {
                Debug.LogWarning($"�����ȃI�v�V�����C���f�b�N�X���N���b�N����܂���: {index}");
                return;
            }

            int selectedModuleId = _currentDisplayedModuleIds[index];
            OnModuleSelected.OnNext(selectedModuleId); // �I�����ꂽ���W���[��ID���C�x���g�Ƃ��Ĕ��΁B

            // �A�C�e���I����A��ʂ����钼�O�ɍw�ǂ�����
            HideModuleView();
        }

        /// <summary>
        /// ���W���[���ɃJ�[�\�����d�˂��ۂ̃n���h���B
        /// </summary>
        /// <param name="index"></param>
        private void OnModuleOptionHovered(int index)
        {
            if (index < 0 || index >= _currentDisplayedModuleIds.Count)
            {
                Debug.LogWarning($"�����ȃI�v�V�����C���f�b�N�X���N���b�N����܂���: {index}");
                return;
            }

            int selectedModuleId = _currentDisplayedModuleIds[index];
            OnModuleHovered.OnNext(selectedModuleId); // �I�����ꂽ���W���[��ID���C�x���g�Ƃ��Ĕ��΁B
        }

        // ----- Public Methods (Presenter����Ăяo�����)
        /// <summary>
        /// �h���b�v�I��UI��\�����܂��B
        /// Presenter����񋟂���郂�W���[���f�[�^�Ɋ�Â���UI���X�V���܂��B
        /// </summary>
        /// <param name="moduleDatas">�\�����郂�W���[���̃f�[�^���X�g�iModuleData��RuntimeModuleData�����������f�[�^�j�B</param>
        public void UpdateModuleView(List<(ModuleData master, RuntimeModuleData runtime)> moduleDatas)
        {
            // �O��̍w�ǂ����ׂĉ���
            _disposables.Clear();

            _currentDisplayedModuleIds.Clear();

            // �n���ꂽ�f�[�^�Ɋ�Â��Ċe�I�v�V����UI��ݒ�
            for (int i = 0; i < _moduleOptionObjects.Length; i++) // _moduleOptionObjects�̒����Ń��[�v
            {
                GameObject obj = _moduleOptionObjects[i];
                if (obj == null) continue; // null�`�F�b�N

                // �܂��͑S�Ĕ�\���ɂ���
                obj.SetActive(false);

                if (i < moduleDatas.Count) // �f�[�^������ꍇ�̂ݐݒ�
                {
                    var (master, runtime) = moduleDatas[i];
                    if (master != null && runtime != null)
                    {
                        obj.SetActive(true);
                        _detailedViews[i].SetInfo(master, runtime);
                        _currentDisplayedModuleIds.Add(master.Id);

                        // �����Ń{�^���C�x���g���čw��
                        // �N���[�W���̂��߂ɃC���f�b�N�X���L���v�`��
                        int index = i;
                        _buttons[i].OnClickAsObservable()
                                 .Subscribe(_ => OnButtonClicked(index))
                                 .AddTo(_disposables); // CompositeDisposable�ɒǉ�
                        _buttons[i].OnPointerEnterAsObservable()
                                 .Subscribe(_ => OnModuleOptionHovered(index))
                                 .AddTo(_disposables); // CompositeDisposable�ɒǉ�
                    }
                }
            }
        }

        /// <summary>
        /// �h���b�v�I��UI���\���ɂ��A�w�ǂ��������܂��B
        /// ��ʂ�������ۂ�A�s�v�ɂȂ����ۂɌĂяo���Ă��������B
        /// </summary>
        public void HideModuleView()
        {
            _disposables.Clear(); // �w�ǂ����ׂĉ���
            foreach (GameObject obj in _moduleOptionObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(false); // UI���\���ɂ���
                }
            }
            _currentDisplayedModuleIds.Clear();
        }
    }
}