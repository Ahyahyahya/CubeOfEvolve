// App.GameSystem.UI/Shop_View.cs
using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using R3;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace App.GameSystem.UI
{
    /// <summary>
    /// �V���b�v��ʂ̃r���[��S������N���X�B
    /// ���W���[�����X�g�̕\���AUI�̕\���E��\���A�w���{�^���N���b�N�C�x���g�̒ʒm���s���B
    /// </summary>
    public class Shop_View : MonoBehaviour
    {
        // ----- SerializeField
        [SerializeField] private GameObject _moduleItemPrefab; // �e���W���[���\���p�̃v���n�u (Detailed_View��Button���܂�)
        [SerializeField] private Transform _contentParent; // ���W���[�����X�g�̐eTransform
        [SerializeField] private TextMeshProUGUI _playerMoneyText; // �v���C���[�̏����R�C���\���p�e�L�X�g
        [SerializeField] private ModuleDataStore _moduleDataStore; // MasterData���擾���邽�߂ɕK�v

        // ----- Events (Presenter �� R3 �ōw�ǂ���)
        public Subject<int> OnModulePurchaseRequested { get; private set; } = new Subject<int>();
        // OnShopCloseRequested ��Presenter�����ڍw�ǂ��Ȃ����ߍ폜�i�O����Shop_View.Hide()���Ăԑz��j

        // ----- Field
        private List<GameObject> _instantiatedModuleItems = new List<GameObject>();
        private Dictionary<int, Button> _purchaseButtons = new Dictionary<int, Button>(); // ���W���[��ID�ƍw���{�^���̃}�b�s���O
        private CompositeDisposable _disposables = new CompositeDisposable(); // R3�w�ǊǗ��p

        // ----- UnityMessage
        private void Awake()
        {
            if (_moduleDataStore == null)
            {
                Debug.LogError("Shop_View: ModuleDataStore is not assigned in Inspector! Cannot display module details.", this);
                enabled = false;
            }
        }

        private void OnDestroy()
        {
            _disposables.Dispose(); // �I�u�W�F�N�g�j�����ɑS�Ă̍w�ǂ�����
        }

        // ----- Public Methods (Presenter ����Ăяo�����)

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
            _disposables.Clear(); // �V�����{�^���w�ǂ̂��߂Ɋ����̍w�ǂ��N���A

            // �e���W���[���f�[�^�����UI�v�f�𐶐��E�ݒ�
            foreach (var runtimeData in shopRuntimeModules)
            {
                if (runtimeData == null)
                {
                    Debug.LogWarning("Shop_View: Null RuntimeModuleData provided in shopRuntimeModules list. Skipping.", this);
                    continue;
                }

                // �Ή�����}�X�^�[�f�[�^���擾
                ModuleData masterData = _moduleDataStore.FindWithId(runtimeData.Id);
                if (masterData == null)
                {
                    Debug.LogError($"Shop_View: MasterData for RuntimeModule ID {runtimeData.Id} not found in ModuleDataStore. Cannot display module.", this);
                    continue;
                }

                GameObject moduleItem = Instantiate(_moduleItemPrefab, _contentParent);
                _instantiatedModuleItems.Add(moduleItem);

                Detailed_View detailedView = moduleItem.GetComponent<Detailed_View>();
                Button purchaseButton = moduleItem.GetComponentInChildren<Button>(); // �q�v�f����{�^����T��

                if (detailedView == null)
                {
                    Debug.LogError($"Shop_View: _moduleItemPrefab does not have Detailed_View component. Module ID: {masterData.Id}", moduleItem);
                    continue;
                }
                if (purchaseButton == null)
                {
                    Debug.LogError($"Shop_View: _moduleItemPrefab does not have Button component in children. Module ID: {masterData.Id}", moduleItem);
                    continue;
                }

                // ���ۂ�RuntimeModuleData��Detailed_View�ɓn��
                detailedView.SetInfo(masterData, runtimeData);

                // �w���{�^���ɃC�x���g��o�^
                _purchaseButtons.Add(masterData.Id, purchaseButton);
                int moduleId = masterData.Id; // �N���[�W���̂��߂ɃR�s�[
                purchaseButton.OnClickAsObservable()
                              .Subscribe(_ => OnModulePurchaseButtonClicked(moduleId))
                              .AddTo(_disposables); // _disposables �ɒǉ�

                // �{�^���̃e�L�X�g��ݒ� (��: "�w�� - 100G")
                TextMeshProUGUI buttonText = purchaseButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = $"�w�� - {masterData.BasePrice}G";
                }
            }
        }

        /// <summary>
        /// �v���C���[�̏����R�C����UI�ɕ\�����܂��B
        /// </summary>
        /// <param name="coins">�v���C���[�̌��݂̏����R�C���B</param>
        public void UpdatePlayerCoins(int coins)
        {
            if (_playerMoneyText != null)
            {
                _playerMoneyText.text = $"�����R�C��: {coins}";
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

        /// <summary>
        /// �V���b�vUI��\�����܂��B
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// �V���b�vUI���\���ɂ��܂��B
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        // ----- Private Methods (UI�C�x���g�n���h��)

        /// <summary>
        /// ���W���[���w���{�^�����N���b�N���ꂽ�Ƃ��ɌĂяo�����n���h���B
        /// </summary>
        /// <param name="moduleId">�w�������N�G�X�g���ꂽ���W���[����ID�B</param>
        private void OnModulePurchaseButtonClicked(int moduleId)
        {
            OnModulePurchaseRequested.OnNext(moduleId); // Presenter�ɒʒm
        }
    }
}