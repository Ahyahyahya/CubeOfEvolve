using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using R3;
using R3.Triggers;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MVRP.AT.View
{
    /// <summary>
    /// �V���b�v��ʂ̃r���[��S������N���X�B
    /// ���W���[�����X�g�̕\���AUI�̕\���E��\���A�w���{�^���N���b�N�C�x���g�̒ʒm���s���܂��B
    /// </summary>
    public class Shop_View : MonoBehaviour
    {
        // ----- SerializedField
        [SerializeField] private GameObject _moduleItemPrefab; // �e���W���[���\���p�̃v���n�u (Detailed_View��Button���܂�)�B
        [SerializeField] private Transform _contentParent; // ���W���[�����X�g�̐eTransform�B
        [SerializeField] private ModuleDataStore _moduleDataStore; // �}�X�^�[�f�[�^���擾���邽�߂ɕK�v�B

        // ----- Events (Presenter��R3�ōw�ǂ���)
        public Subject<int> OnModulePurchaseRequested { get; private set; } = new Subject<int>(); // ���W���[���w�����N�G�X�g��ʒm����Subject�B
        public Subject<int> OnModuleHovered { get; private set; } = new Subject<int>(); // ���W���[���w�����N�G�X�g��ʒm����Subject�B

        // ----- Private Members (�����f�[�^)
        private List<GameObject> _instantiatedModuleItems = new List<GameObject>(); // �������ꂽ���W���[���A�C�e���̃��X�g�B
        private Dictionary<int, Button> _purchaseButtons = new Dictionary<int, Button>(); // ���W���[��ID�ƍw���{�^���̃}�b�s���O�B
        private CompositeDisposable _disposables = new CompositeDisposable(); // R3�w�ǊǗ��p�B

        // ----- UnityMessage
        
        private void Awake()
        {
            if (_moduleDataStore == null)
            {
                Debug.LogError("Shop_View: ModuleDataStore��Inspector�Őݒ肳��Ă��܂���I���W���[���̏ڍׂ�\���ł��܂���B", this);
                enabled = false;
            }
        }

        private void OnDestroy()
        {
            _disposables.Dispose(); // �I�u�W�F�N�g�j�����ɑS�Ă̍w�ǂ������B
        }

        // ----- Public Methods (Presenter����Ăяo�����)

        /// <summary>
        /// �V���b�v�ɕ\�����郂�W���[�����X�g��ݒ肵�AUI���X�V���܂��B
        /// ���x��1�ȏ�̃��W���[���̂݁A���ۂ̃����^�C���f�[�^�Ɋ�Â��ĕ\������܂��B
        /// </summary>
        /// <param name="shopRuntimeModules">�V���b�v�ɕ\������RuntimeModuleData�̃��X�g�B</param>
        public void DisplayShopModules(List<RuntimeModuleData> shopRuntimeModules)
        {
            // �����̃��W���[���A�C�e����S�ăN���A
            foreach (var item in _instantiatedModuleItems)
            {
                Destroy(item);
            }
            _instantiatedModuleItems.Clear();
            _purchaseButtons.Clear();
            _disposables.Clear(); // �V�����{�^���w�ǂ̂��߂Ɋ����̍w�ǂ��N���A�B

            // �e���W���[���f�[�^�����UI�v�f�𐶐��E�ݒ�
            foreach (var runtimeData in shopRuntimeModules)
            {
                if (runtimeData == null)
                {
                    Debug.LogWarning("Shop_View: �V���b�v�̃����^�C�����W���[�����X�g��null�f�[�^���񋟂���܂����B�X�L�b�v���܂��B", this);
                    continue;
                }

                // �Ή�����}�X�^�[�f�[�^���擾
                ModuleData masterData = _moduleDataStore.FindWithId(runtimeData.Id);
                if (masterData == null)
                {
                    Debug.LogError($"Shop_View: ModuleDataStore�Ƀ����^�C�����W���[��ID {runtimeData.Id} �̃}�X�^�[�f�[�^��������܂���B���W���[����\���ł��܂���B", this);
                    continue;
                }

                GameObject moduleItem = Instantiate(_moduleItemPrefab, _contentParent);
                _instantiatedModuleItems.Add(moduleItem);

                Detailed_View detailedView = moduleItem.GetComponent<Detailed_View>();
                Button purchaseButton = moduleItem.GetComponentInChildren<Button>(); // �q�v�f����{�^����T���B

                if (detailedView == null)
                {
                    Debug.LogError($"Shop_View: _moduleItemPrefab��Detailed_View�R���|�[�l���g������܂���B���W���[��ID: {masterData.Id}", moduleItem);
                    continue;
                }
                if (purchaseButton == null)
                {
                    Debug.LogError($"Shop_View: _moduleItemPrefab�̎q�v�f��Button�R���|�[�l���g������܂���B���W���[��ID: {masterData.Id}", moduleItem);
                    continue;
                }

                // ���ۂ�RuntimeModuleData��Detailed_View�ɓn��
                detailedView.SetInfo(masterData, runtimeData);

                // �w���{�^���ɃC�x���g��o�^
                _purchaseButtons.Add(masterData.Id, purchaseButton);
                int moduleId = masterData.Id; // �N���[�W���̂��߂ɃR�s�[�B
                purchaseButton.OnClickAsObservable()
                    .Subscribe(_ => OnModulePurchaseButtonClicked(moduleId))
                    .AddTo(_disposables); // _disposables �ɒǉ��B
                // OnEnter
                purchaseButton.OnPointerEnterAsObservable()
                    .Subscribe(_ => OnShopItemHovered(moduleId))
                    .AddTo(_disposables);

                // �{�^���̃e�L�X�g��ݒ� (��: "�w�� - 100G")
                TextMeshProUGUI buttonText = purchaseButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = $"�w�� - {masterData.BasePrice}G";
                }
            }
        }

        /// <summary>
        /// ����̃��W���[���̍w���{�^���̗L��/������؂�ւ��܂��B
        /// </summary>
        /// <param name="moduleId">�Ώۂ̃��W���[��ID�B</param>
        /// <param name="isInteractable">�{�^���𑀍�\�ɂ��邩�B</param>
        public void SetPurchaseButtonInteractable(int moduleId, bool isInteractable)
        {
            if (_purchaseButtons.TryGetValue(moduleId, out Button button))
            {
                button.interactable = isInteractable;
            }
        }

        // ----- Private Methods (UI�C�x���g�n���h��)

        /// <summary>
        /// ���W���[���w���{�^�����N���b�N���ꂽ�Ƃ��ɌĂяo�����n���h���ł��B
        /// </summary>
        /// <param name="moduleId">�w�������N�G�X�g���ꂽ���W���[����ID�B</param>
        private void OnModulePurchaseButtonClicked(int moduleId)
        {
            OnModulePurchaseRequested.OnNext(moduleId); // Presenter�ɒʒm�B
        }

        /// <summary>
        /// ���W���[���ɃJ�[�\�����d�˂��ۂ̃n���h���B
        /// </summary>
        /// <param name="moduleId"></param>
        private void OnShopItemHovered(int moduleId)
        {
            OnModuleHovered.OnNext(moduleId); // �I�����ꂽ���W���[��ID���C�x���g�Ƃ��Ĕ��΁B
        }
    }
}