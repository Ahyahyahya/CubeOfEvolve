// �쐬���F250616
// �쐬�ҁFAT
// CanvasCtrl�������Ȃ������ߊǗ��p�ɍ쐬�B
// ���������A�ꊇ����Ɏg�p�B

using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Assets.IGC2025.Scripts.GameManagers;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AT.uGUI
{
    /// <summary>
    /// �V�[�����CanvasCtrl�C���X�^���X���Ǘ����A�ꊇ����⏉�������s�����߂̃��X�g�R���|�[�l���g�B
    /// Singleton�p�^�[���ɂ��A�ǂ�����ł��A�N�Z�X�\�ł��B
    /// </summary>
    public class CanvasCtrlManager : MonoBehaviour
    {
        // --- Inspector Control
        [System.Serializable]
        private class CanvasEntry
        {
            public string key;
            public CanvasCtrl canvasCtrl;

            public CanvasEntry(CanvasCtrl canvasCtrl)
            {
                key = $"{canvasCtrl.name}";
                this.canvasCtrl = canvasCtrl;
            }
        }

        // --- Field
        [SerializeField]
        private List<CanvasEntry> _canvasEntries;

        // --- Singleton Pattern
        public static CanvasCtrlManager Instance { get; private set; }

        // --- UnityMessage
        private void Awake()
        {
            // ���łɕʂ̃C���X�^���X�����݂���ꍇ�A�����j��
            if (Instance != null && Instance != this)
            {
                Debug.Log("[CanvasCtrlManager] �Â��C���X�^���X��j�����A�ŐV�̃C���X�^���X�ɍ����ւ��܂�", this);
                Destroy(Instance.gameObject);
            }

            // ���̃C���X�^���X���ŐV�Ƃ��ēo�^
            Instance = this;
            Debug.Log("[CanvasCtrlManager] �V�����C���X�^���X���ݒ肳��܂���", this);

            // Setup ���s
            if (_canvasEntries == null || _canvasEntries.Count == 0)
            {
                Setup();
            }
            else
            {
                _canvasEntries = _canvasEntries.Where(entry => entry.canvasCtrl != null).ToList();
            }

            Initialize();
        }

        private void Start()
        {
            ShowOnlyCanvas("TitleView");
            GameManager.Instance.ChangeGameState(GameState.TITLE);
        }

        // --- Private Methods

        /// <summary>
        /// �V�[�����CanvasCtrl�R���|�[�l���g�����o���A�Ǘ����X�g�ɓo�^���܂��B
        /// �����̃��X�g�G���g�����X�V���A�����ȎQ�Ƃ��폜���܂��B
        /// ��ɃG�f�B�^�́uSetup CanvasCtrls�v�{�^������Ăяo����܂��B
        /// </summary>
        private void Setup()
        {
            CanvasCtrl[] sceneCanvasCtrls = FindObjectsOfType<CanvasCtrl>();

            if (_canvasEntries == null)
            {
                _canvasEntries = new List<CanvasEntry>();
            }

            // �V����CanvasCtrl��ǉ�
            foreach (CanvasCtrl ctrl in sceneCanvasCtrls)
            {
                // ���Ƀ��X�g�ɑ��݂��Ȃ�CanvasCtrl�݂̂�ǉ�
                if (_canvasEntries.FindIndex(x => x.canvasCtrl == ctrl) < 0)
                {
                    _canvasEntries.Add(new CanvasEntry(ctrl));
                }
            }

            // �V�[�����疳���Ȃ��Ă���CanvasCtrl�����X�g����폜
            for (int i = _canvasEntries.Count - 1; i >= 0; i--)
            {
                if (_canvasEntries[i].canvasCtrl == null)
                {
                    _canvasEntries.RemoveAt(i);
                }
            }

            //debug.log($"CanvasCtrlList: Setup�����B�Ǘ��Ώۂ�CanvasCtrl��: {_canvasEntries.Count}");
#if UNITY_EDITOR
            EditorUtility.SetDirty(this); // �G�f�B�^��ŕύX��ۑ�
#endif
        }

        /// <summary>
        /// �ˑ��֘A�̎Q�Ɗm�F�B�S�Ă�Canvas�̔�\���B
        /// </summary>
        private void Initialize()
        {
            if (_canvasEntries == null)
            {
                Debug.LogError("CanvasCtrlList: _canvasEntries��null�ł��BSetup�����s����Ă��܂��񂩁H", this);
                return;
            }

            // �S�ẴL�����o�X���\���ɂ���
            foreach (CanvasEntry entry in _canvasEntries)
            {
                // CanvasCtrl��null�łȂ����Ƃ��m�F
                if (entry.canvasCtrl != null)
                {
                    entry.canvasCtrl.GetComponent<Canvas>().enabled = false;
                    //entry.canvasCtrl.OnCloseCanvas();
                }
            }
        }

        // --- Public Methods

        /// <summary>
        /// �L�[�Ɋ�Â���CanvasCtrl���擾���܂��B
        /// </summary>
        /// <param name="key">�擾����CanvasCtrl�̃L�[�B</param>
        /// <returns>�Ή�����CanvasCtrl�C���X�^���X�A�܂��͌�����Ȃ��ꍇ��null�B</returns>
        public CanvasCtrl GetCanvas(string key)
        {
            if (_canvasEntries == null)
            {
                Debug.LogError("CanvasCtrlList: _canvasEntries��null�ł��BSetup�����s����Ă��܂��񂩁H", this);
                return null;
            }

            CanvasEntry entry = _canvasEntries.Find(x => x.key == key);
            if (entry == null || entry.canvasCtrl == null)
            {
                // ������Ȃ��A�܂��͎Q�Ƃ�null�ɂȂ��Ă���ꍇ
                Debug.LogWarning($"CanvasCtrlList: �L�[ '{key}' �ɑΉ�����CanvasCtrl��������Ȃ����A�Q�Ƃ��؂�Ă��܂��B", this);
                return null;
            }
            return entry.canvasCtrl;
        }

        /// <summary>
        /// �S�Ă�CanvasCtrl���\���ɂ��܂��B
        /// </summary>
        public void HideAllCanvases()
        {
            if (_canvasEntries == null) return;

            foreach (var entry in _canvasEntries)
            {
                if (entry.canvasCtrl != null)
                {
                    //entry.canvasCtrl.GetComponent<Canvas>().enabled = false;
                    entry.canvasCtrl.OnCloseCanvas();
                }
            }
        }

        /// <summary>
        /// �w�肳�ꂽ�L�[��CanvasCtrl��\�����A����ȊO�̑S�Ă�CanvasCtrl���\���ɂ��܂��B
        /// </summary>
        /// <param name="keyToShow">�\������CanvasCtrl�̃L�[�B</param>
        public void ShowOnlyCanvas(string keyToShow)
        {
            if (_canvasEntries == null)
            {
                Debug.LogError("CanvasCtrlList: _canvasEntries��null�ł��BSetup�����s����Ă��܂��񂩁H", this);
                return;
            }

            CanvasCtrl canvasToActivate = null;

            foreach (var entry in _canvasEntries)
            {
                if (entry.canvasCtrl != null)
                {
                    if (entry.key == keyToShow)
                    {
                        canvasToActivate = entry.canvasCtrl;
                    }
                    else
                    {
                        //entry.canvasCtrl.OnCloseCanvas(); // ���̃L�����o�X���\��
                        entry.canvasCtrl.GetComponent<Canvas>().enabled = false;
                    }
                }
            }

            if (canvasToActivate != null)
            {
                canvasToActivate.OnOpenCanvas(); // �ړI�̃L�����o�X��\��
                //canvasToActivate.GetComponent<Canvas>().enabled = true;
            }
            else
            {
                Debug.LogWarning($"CanvasCtrlList: �L�[ '{keyToShow}' �ɑΉ�����CanvasCtrl��������܂���ł����B�\���ł��܂���ł����B", this);
            }
        }


        // --- Editor Integration
#if UNITY_EDITOR
        [CustomEditor(typeof(CanvasCtrlManager))]
        public class CanvasCtrlListEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                CanvasCtrlManager canvasList = target as CanvasCtrlManager;
                if (GUILayout.Button("Setup CanvasCtrls (Find All Scene CanvasCtrls)"))
                {
                    canvasList.Setup();
                }

                EditorGUILayout.HelpBox(
                    "�uSetup CanvasCtrls�v�{�^���������ƁA�V�[����̑S�Ă�CanvasCtrl���������o���ă��X�g�ɒǉ����܂��B\n" +
                    "�L�[�͎蓮�Őݒ肵�Ă��������B�Q�Ƃ��؂ꂽCanvasCtrl�͎����I�ɍ폜����܂��B",
                    MessageType.Info
                );

                base.OnInspectorGUI();
            }
        }
#endif
    }
}