using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using Assets.AT;
using R3;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.IGC2025.Scripts.View
{
    /// <summary>
    /// �V���b�v��ʂ̃r���[��S������N���X�B
    /// ���W���[�����X�g�̕\���AUI�̕\���E��\���A�w���ڍו\�����s���܂��B
    /// </summary>
    public class ViewShopCanvas : MonoBehaviour
    {
        [SerializeField] private GameObject _moduleItemPrefab; // �e���W���[���\���p�̃v���n�u�B
        [SerializeField] private Transform _contentParent; // ���W���[�����X�g�̐eTransform�B

        public Subject<int> OnModuleDetailRequested { get; private set; } = new Subject<int>();
        public Subject<int> OnModulePurchaseRequested { get; private set; } = new Subject<int>();

        private List<GameObject> _instantiatedModuleItems = new List<GameObject>();
        private Dictionary<int, Button> _purchaseButtons = new Dictionary<int, Button>();
        private CompositeDisposable _disposables = new CompositeDisposable();

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        /// <summary>
        /// �V���b�v�ɕ\�����郂�W���[�����X�g��ݒ肵�AUI���X�V���܂��B
        /// </summary>
        public void DisplayShopModules(List<RuntimeModuleData> shopRuntimeModules, ModuleDataStore moduleDataStore)
        {
            foreach (var item in _instantiatedModuleItems)
            {
                Destroy(item);
            }
            _instantiatedModuleItems.Clear();
            _purchaseButtons.Clear();
            _disposables.Clear();

            foreach (var runtimeData in shopRuntimeModules)
            {
                if (runtimeData == null)
                {
                    Debug.LogWarning("Shop_View: null�f�[�^���񋟂���܂����B", this);
                    continue;
                }

                ModuleData masterData = moduleDataStore.FindWithId(runtimeData.Id);
                if (masterData == null)
                {
                    Debug.LogError($"Shop_View: ID {runtimeData.Id} �̃}�X�^�[�f�[�^��������܂���B", this);
                    continue;
                }

                GameObject moduleItem = Instantiate(_moduleItemPrefab, _contentParent);
                _instantiatedModuleItems.Add(moduleItem);

                ViewInfo detailedView = moduleItem.GetComponent<ViewInfo>();
                Button purchaseButton = moduleItem.GetComponentInChildren<Button>();

                if (detailedView == null || purchaseButton == null)
                {
                    Debug.LogError($"Shop_View: �v���n�u�̃R���|�[�l���g���s�����Ă��܂��BID: {masterData.Id}", moduleItem);
                    continue;
                }

                detailedView.SetInfo(masterData, runtimeData);
                _purchaseButtons.Add(masterData.Id, purchaseButton);

                int moduleId = masterData.Id;
                purchaseButton.OnClickAsObservable()
                    .Subscribe(_ => OnModuleDetailRequested.OnNext(moduleId))
                    .AddTo(_disposables);
            }
        }

        /// <summary>
        /// ����̃��W���[���̍w���{�^���̗L��/������؂�ւ��܂��B
        /// </summary>
        public void SetPurchaseButtonInteractable(int moduleId, bool isInteractable)
        {
            if (_purchaseButtons.TryGetValue(moduleId, out Button button))
            {
                button.interactable = isInteractable;
            }
        }

        /// <summary>
        /// �ڍ�UI�̍w���{�^������Ă΂�郁�\�b�h�B
        /// </summary>
        public void RequestPurchase(int moduleId)
        {
            GameSoundManager.Instance.PlaySE("shop_buy1", "SE");
            GameSoundManager.Instance.PlaySE("shop_buy2", "SE");
            OnModulePurchaseRequested.OnNext(moduleId);
        }
    }
}
