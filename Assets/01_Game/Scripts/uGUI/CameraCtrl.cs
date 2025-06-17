using Unity.Cinemachine;
using System.Collections.Generic;
using UnityEngine;
using R3; // R3 (UniRx.Async) ���g�p���邽��
using System.Linq; // LINQ���g�p���邽��


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AT.CameraSystem // �K�؂�Namespace�ɕύX
{
    /// <summary>
    /// �V�[�����CinemachineCamera�C���X�^���X���Ǘ����A�؂�ւ��⏉�������s�����߂̃R���|�[�l���g�B
    /// Singleton�p�^�[���ɂ��A�ǂ�����ł��A�N�Z�X�\�ł��B
    /// </summary>
    public class CameraCtrl : MonoBehaviour
    {
        // --- �萔
        private const int BASE_PRIORITY = 10; // �J�����̊�{�v���C�I���e�B
        private const int ACTIVE_CAMERA_PRIORITY_OFFSET = 100; // �A�N�e�B�u�J�����ɉ��Z����v���C�I���e�B

        // --- Inspector Control
        [System.Serializable]
        private class CameraEntry
        {
            public string key;
            public CinemachineCamera Camera; // Unity.Cinemachine�ł͂Ȃ�CinemachineCamera

            public CameraEntry(CinemachineCamera cam)
            {
                key = cam.name; // �J������GameObject�����L�[�Ƃ���
                this.Camera = cam;
            }
        }

        // --- Field
        [SerializeField]
        private List<CameraEntry> _cameraEntries = new List<CameraEntry>(); // �J�����G���g���̃��X�g

        [Header("�����ݒ�")]
        [SerializeField]
        private string _initialActiveCameraKey = ""; // �����\������J�����̃L�[

        private CinemachineBrain _cinemachineBrain;
        private float _cameraBlendTime; // CinemachineBrain����擾�����u�����h����
        private string _currentActiveCameraKey = null; // ���݃A�N�e�B�u�ȃJ�����̃L�[

        // --- Singleton Pattern
        public static CameraCtrl Instance { get; private set; }

        // �J�����؂�ւ��̑ҋ@���ԁi�u�����h���ԁj���O������擾���邽�߂̃v���p�e�B
        public float CameraBlendTime => _cameraBlendTime;

        // --- UnityMessage
        private void Awake()
        {
            // Singleton�̃C���X�^���X�ݒ�
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("CameraCtrl: ���ɕʂ̃C���X�^���X�����݂��܂��B���̃I�u�W�F�N�g�͔j������܂��B", this);
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // CinemachineBrain�̎擾�ƃG���[�n���h�����O
            _cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
            if (_cinemachineBrain == null)
            {
                Debug.LogError("���C���J������CinemachineBrain��������܂���B�J�������䂪�������@�\���Ȃ��\��������܂��B", this);
                return;
            }

            // CinemachineBrain����u�����h���Ԃ��擾
            _cameraBlendTime = _cinemachineBrain.DefaultBlend.BlendTime;

            // Optional: Inspector�Őݒ肳��Ă��Ȃ��ꍇ�A�G�f�B�^�Ŏ����I��Setup�𑖂点��
            if (_cameraEntries == null || _cameraEntries.Count == 0)
            {
                SetupCameras();
            }
            else
            {
                // �����^�C���`�F�b�N�Fnull�ɂȂ��Ă���G���g�����t�B���^�����O
                _cameraEntries = _cameraEntries.Where(entry => entry.Camera != null).ToList();
            }
        }

        private void Start()
        {
            InitializeCameras();
        }

        // --- Private Methods

        /// <summary>
        /// �V�[�����CinemachineCamera�R���|�[�l���g�����o���A�Ǘ����X�g�ɓo�^���܂��B
        /// �����̃��X�g�G���g�����X�V���A�����ȎQ�Ƃ��폜���܂��B
        /// ��ɃG�f�B�^�́uSetup Cameras�v�{�^������Ăяo����܂��B
        /// </summary>
        private void SetupCameras()
        {
            // CinemachineCamera ���擾
            // Unity.Cinemachine.CinemachineCamera �� CinemachineCamera �̊��N���X�����A
            // ����̓I�� CinemachineCamera �𒼐ڈ���������ʓI
            CinemachineCamera[] sceneVirtualCameras = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);

            if (_cameraEntries == null)
            {
                _cameraEntries = new List<CameraEntry>();
            }

            // �V����VirtualCamera��ǉ�
            foreach (CinemachineCamera cam in sceneVirtualCameras)
            {
                // ���Ƀ��X�g�ɑ��݂��Ȃ�VirtualCamera�݂̂�ǉ�
                if (_cameraEntries.FindIndex(x => x.Camera == cam) < 0)
                {
                    _cameraEntries.Add(new CameraEntry(cam));
                }
            }

            // �V�[�����疳���Ȃ��Ă���VirtualCamera�����X�g����폜
            for (int i = _cameraEntries.Count - 1; i >= 0; i--)
            {
                if (_cameraEntries[i].Camera == null)
                {
                    _cameraEntries.RemoveAt(i);
                }
                // �L�[���I�u�W�F�N�g���ɍX�V����i���O�ύX�ɑΉ����邽�߁j
                else
                {
                    _cameraEntries[i].key = _cameraEntries[i].Camera.name;
                }
            }

            // �L�[�ɂ��A�N�Z�X��e�Ղɂ��邽�߁A�L�[�Ń\�[�g����i�C�Ӂj
            _cameraEntries = _cameraEntries.OrderBy(entry => entry.key).ToList();

            Debug.Log($"CameraCtrl: Setup�����B�Ǘ��Ώۂ�CinemachineCamera��: {_cameraEntries.Count}");
#if UNITY_EDITOR
            EditorUtility.SetDirty(this); // �G�f�B�^��ŕύX��ۑ�
#endif
        }

        /// <summary>
        /// �J�����̏����ݒ���s���܂��B
        /// �e���z�J�����̃v���C�I���e�B��ݒ肵�A�����J�������A�N�e�B�u�ɂ��܂��B
        /// </summary>
        private void InitializeCameras()
        {
            // ���z�J�������X�g�̃o���f�[�V����
            if (_cameraEntries == null || _cameraEntries.Count == 0)
            {
                Debug.LogWarning("���z�J�������ݒ肳��Ă��܂���BCameraCtrl�͋@�\���܂���B", this);
                return;
            }

            // �S�ẴJ�������A�N�e�B�u�ȏ�ԁi�Ⴂ�v���C�I���e�B�j�ɂ���
            foreach (var entry in _cameraEntries)
            {
                if (entry.Camera != null)
                {
                    // �e�J�����̊�{�v���C�I���e�B��ݒ�B�ォ��ύX�ł���悤BASE_PRIORITY�ɃI�t�Z�b�g�����Z���Ȃ�
                    entry.Camera.Priority = BASE_PRIORITY;
                }
                else
                {
                    Debug.LogWarning($"_cameraEntries���X�g��null�̗v�f������܂��B", this);
                }
            }

            // �����J�������A�N�e�B�u�ɂ���
            if (!string.IsNullOrEmpty(_initialActiveCameraKey))
            {
                SetActiveCamera(_initialActiveCameraKey);
            }
            else
            {
                Debug.LogWarning("�����A�N�e�B�u�J�����̃L�[���ݒ肳��Ă��܂���B�ŏ��̃J�������f�t�H���g�ŃA�N�e�B�u�ɂ��܂��B", this);
                // �L�[���ݒ肳��Ă��Ȃ��ꍇ�́A���X�g�̍ŏ��̗L���ȃJ�������A�N�e�B�u�ɂ���
                if (_cameraEntries.Count > 0 && _cameraEntries[0].Camera != null)
                {
                    SetActiveCamera(_cameraEntries[0].key);
                }
                else
                {
                    Debug.LogError("�A�N�e�B�u�ɂł���J����������܂���B", this);
                }
            }
        }

        /// <summary>
        /// �w�肳�ꂽ�L�[�̃J�������A�N�e�B�u�ɂ��A_currentActiveCameraKey���X�V���܂��B
        /// </summary>
        /// <param name="key">�A�N�e�B�u�ɂ���J�����̃L�[�B</param>
        private void SetActiveCamera(string key)
        {
            CinemachineCamera targetCam = GetCamera(key);

            if (targetCam != null)
            {
                // ���݃A�N�e�B�u�ȃJ����������΃v���C�I���e�B�����ɖ߂�
                if (!string.IsNullOrEmpty(_currentActiveCameraKey))
                {
                    CinemachineCamera prevCam = GetCamera(_currentActiveCameraKey);
                    if (prevCam != null)
                    {
                        prevCam.Priority = BASE_PRIORITY;
                    }
                }

                // �V�����J�������A�N�e�B�u�ɂ���
                targetCam.Priority = BASE_PRIORITY + ACTIVE_CAMERA_PRIORITY_OFFSET;
                _currentActiveCameraKey = key;
                Debug.Log($"�J���� '{key}' ���A�N�e�B�u�ɂȂ�܂����B");
            }
            else
            {
                Debug.LogError($"�����ȃJ�����L�[���w�肳��܂���: '{key}'�B�܂��̓J������������܂���B", this);
            }
        }

        /// <summary>
        /// �L�[�Ɋ�Â���CinemachineCamera���擾���܂��B
        /// </summary>
        /// <param name="key">�擾����CinemachineCamera�̃L�[�B</param>
        /// <returns>�Ή�����CinemachineCamera�C���X�^���X�A�܂��͌�����Ȃ��ꍇ��null�B</returns>
        public CinemachineCamera GetCamera(string key)
        {
            if (_cameraEntries == null)
            {
                Debug.LogError("CameraCtrl: _cameraEntries��null�ł��BSetupCameras�����s����Ă��܂��񂩁H", this);
                return null;
            }

            CameraEntry entry = _cameraEntries.Find(x => x.key == key);
            if (entry == null || entry.Camera == null)
            {
                Debug.LogWarning($"CameraCtrl: �L�[ '{key}' �ɑΉ�����CinemachineCamera��������Ȃ����A�Q�Ƃ��؂�Ă��܂��B", this);
                return null;
            }
            return entry.Camera;
        }

        // --- Public Methods

        /// <summary>
        /// �w�肳�ꂽ�L�[�̃J�����ɐ؂�ւ��܂��B
        /// R3 (UniRx.Async) ���g�p���āA�J�����؂�ւ��Ƒҋ@��񓯊��ɏ������܂��B
        /// </summary>
        /// <param name="targetCameraKey">�؂�ւ��Ώۂ̃J�����L�[�B</param>
        public void ChangeCamera(string targetCameraKey)
        {
            // �����ȃL�[�܂��͌��݂̃J�����Ɠ����ꍇ�͉������Ȃ�
            if (targetCameraKey == _currentActiveCameraKey || string.IsNullOrEmpty(targetCameraKey))
            {
                return;
            }

            CinemachineCamera targetCam = GetCamera(targetCameraKey);
            if (targetCam == null)
            {
                Debug.LogWarning($"CameraCtrl: �؂�ւ��Ώۂ̃J���� '{targetCameraKey}' ��������Ȃ����߁A�؂�ւ��𒆎~���܂��B", this);
                return;
            }

            // �V�����J�������A�N�e�B�u�ɂ���
            SetActiveCamera(targetCameraKey);

            // �J�����̃u�����h���Ԃ����ҋ@
            Observable.Timer(System.TimeSpan.FromSeconds(_cameraBlendTime))
                .Subscribe(_ =>
                {
                    // �J�����؂�ւ�������̏������K�v�ł���΂����ɋL�q
                    // ��: Debug.Log($"�J�������L�[: '{targetCameraKey}' �ɐ؂�ւ��܂����B");
                })
                .AddTo(this); // GameObject���j�����ꂽ�Ƃ��ɍw�ǂ�����
        }

        /// <summary>
        /// ���݃A�N�e�B�u�ȃJ�����̃L�[���擾���܂��B
        /// </summary>
        public string GetCurrentActiveCameraKey()
        {
            return _currentActiveCameraKey;
        }


        // --- Editor Integration
#if UNITY_EDITOR
        [CustomEditor(typeof(CameraCtrl))]
        public class CameraCtrlEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                // �f�t�H���g��Inspector��\��
                DrawDefaultInspector();

                CameraCtrl cameraCtrl = (CameraCtrl)target;

                EditorGUILayout.Space();

                if (GUILayout.Button("Setup Cameras (Find All Scene Virtual Cameras)"))
                {
                    cameraCtrl.SetupCameras();
                }

                EditorGUILayout.HelpBox(
                    "�uSetup Cameras�v�{�^���������ƁA�V�[����̑S�Ă�CinemachineCamera���������o���ă��X�g�ɒǉ����܂��B\n" +
                    "�L�[�͎����I��GameObject�����ݒ肳��܂��BGameObject�����ύX���ꂽ�ꍇ�������ōX�V����܂��B\n" +
                    "�Q�Ƃ��؂ꂽ�J�����͎����I�Ƀ��X�g����폜����܂��B",
                    MessageType.Info
                );
            }
        }
#endif
    }
}