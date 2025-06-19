using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using R3;
using R3.Triggers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.IGC2025.Scripts.View
{
    public class ViewBuildCanvas : MonoBehaviour
    {
        // ----- SerializedField
        [SerializeField] private GameObject _moduleItemPrefab; // �e���W���[���\���p�̃v���n�u (Detailed_View��Button���܂�)�B
        [SerializeField] private Transform _contentParent; // ���W���[�����X�g�̐eTransform�B

        // ----- Events
        public Subject<int> OnModuleChoiceRequested { get; private set; } = new Subject<int>(); // ���W���[���w�����N�G�X�g��ʒm����Subject�B
        public Subject<int> OnModuleHovered { get; private set; } = new Subject<int>(); // ���W���[���w�����N�G�X�g��ʒm����Subject�B

        // ----- Private Members (�����f�[�^)
        private List<GameObject> _instantiatedModuleItems = new List<GameObject>(); // �������ꂽ���W���[���A�C�e���̃��X�g�B
        private Dictionary<int, Button> _choiceButtons = new Dictionary<int, Button>(); // ���W���[��ID�ƍw���{�^���̃}�b�s���O�B
        private CompositeDisposable _disposables = new CompositeDisposable(); // R3�w�ǊǗ��p�B

        // ----- UnityMessage

        private void OnDestroy()
        {
            _disposables.Dispose(); // �I�u�W�F�N�g�j�����ɑS�Ă̍w�ǂ������B
        }

        // ----- Public
        /// <summary>
        /// �V���b�v�ɕ\�����郂�W���[�����X�g��ݒ肵�AUI���X�V���܂��B
        /// 1�ȏ㏊�����Ă���̃��W���[���̂݁A���ۂ̃����^�C���f�[�^�Ɋ�Â��ĕ\������܂��B
        /// </summary>
        /// <param name="buildRuntimeModules">�V���b�v�ɕ\������RuntimeModuleData�̃��X�g�B</param>
        public void DisplayBuildModules(List<RuntimeModuleData> buildRuntimeModules, ModuleDataStore moduleDataStore)
        {
            // �����̃��W���[���A�C�e����S�ăN���A
            foreach (var item in _instantiatedModuleItems)
            {
                Destroy(item);
            }
            _instantiatedModuleItems.Clear();
            _choiceButtons.Clear();
            _disposables.Clear(); // �V�����{�^���w�ǂ̂��߂Ɋ����̍w�ǂ��N���A�B

            // �e���W���[���f�[�^�����UI�v�f�𐶐��E�ݒ�
            foreach (var runtimeData in buildRuntimeModules)
            {
                if (runtimeData == null)
                {
                    Debug.LogWarning("Build_View: �V���b�v�̃����^�C�����W���[�����X�g��null�f�[�^���񋟂���܂����B�X�L�b�v���܂��B", this);
                    continue;
                }

                // �Ή�����}�X�^�[�f�[�^���擾
                ModuleData masterData = moduleDataStore.FindWithId(runtimeData.Id);
                if (masterData == null)
                {
                    Debug.LogError($"Build_View: ModuleDataStore�Ƀ����^�C�����W���[��ID {runtimeData.Id} �̃}�X�^�[�f�[�^��������܂���B���W���[����\���ł��܂���B", this);
                    continue;
                }

                GameObject moduleItem = Instantiate(_moduleItemPrefab, _contentParent);
                _instantiatedModuleItems.Add(moduleItem);

                ViewInfo InfoView = moduleItem.GetComponent<ViewInfo>();
                Button choiceButton = moduleItem.GetComponentInChildren<Button>(); // �q�v�f����{�^����T���B

                if (InfoView == null)
                {
                    Debug.LogError($"Build_View: _moduleItemPrefab��Detailed_View�R���|�[�l���g������܂���B���W���[��ID: {masterData.Id}", moduleItem);
                    continue;
                }
                if (choiceButton == null)
                {
                    Debug.LogError($"Build_View: _moduleItemPrefab�̎q�v�f��Button�R���|�[�l���g������܂���B���W���[��ID: {masterData.Id}", moduleItem);
                    continue;
                }

                // ���ۂ�RuntimeModuleData��Detailed_View�ɓn��
                InfoView.SetInfo(masterData, runtimeData);

                // �w���{�^���ɃC�x���g��o�^
                _choiceButtons.Add(masterData.Id, choiceButton);
                int moduleId = masterData.Id; // �N���[�W���̂��߂ɃR�s�[�B
                choiceButton.OnClickAsObservable()
                    .Subscribe(_ => OnModuleChoiceButtonClicked(moduleId))
                    .AddTo(_disposables); // _disposables �ɒǉ��B
                // OnEnter
                choiceButton.OnPointerEnterAsObservable()
                    .Subscribe(_ => OnBuildItemHovered(moduleId))
                    .AddTo(_disposables);

            }
        }

        /// <summary>
        /// ����̃��W���[���̑I���{�^���̗L��/������؂�ւ��܂��B
        /// </summary>
        /// <param name="moduleId">�Ώۂ̃��W���[��ID�B</param>
        /// <param name="isInteractable">�{�^���𑀍�\�ɂ��邩�B</param>
        public void SetChoiceButtonInteractable(int moduleId, bool isInteractable)
        {
            if (_choiceButtons.TryGetValue(moduleId, out Button button))
            {
                button.interactable = isInteractable;
            }
        }

        // ----- Private

        /// <summary>
        /// ���W���[���I���{�^�����N���b�N���ꂽ�Ƃ��ɌĂяo�����n���h���ł��B
        /// </summary>
        /// <param name="moduleId">�I�������N�G�X�g���ꂽ���W���[����ID�B</param>
        private void OnModuleChoiceButtonClicked(int moduleId)
        {
            OnModuleChoiceRequested.OnNext(moduleId); // Presenter�ɒʒm�B
        }

        /// <summary>
        /// ���W���[���ɃJ�[�\�����d�˂��ۂ̃n���h���B
        /// </summary>
        /// <param name="moduleId"></param>
        private void OnBuildItemHovered(int moduleId)
        {
            OnModuleHovered.OnNext(moduleId); // �I�����ꂽ���W���[��ID���C�x���g�Ƃ��Ĕ��΁B
        }
    }
}