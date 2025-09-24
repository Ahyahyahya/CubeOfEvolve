// Copyright (C) 2015-2021 gamevanilla - All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement.
// A Copy of the Asset Store EULA is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.UI;
using R3;
using System;
using Assets.IGC2025.Scripts.View;

namespace Assets.AT
{
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Animator))]
    public class Switch : MonoBehaviour
    {
        public enum SourceType
        {
            GuideEnabled,       // GuideManager.Instance.GuideEnabled
            DropChoseEnabled    // ViewDropCanvas.DropChoseEnabled
        }

        [Header("Reactive Source")]
        [SerializeField] private SourceType _source = SourceType.GuideEnabled;
        [SerializeField, Tooltip("Source=DropChoseEnabled のときに参照（未指定なら FindObjectOfType で探索）")]
        private ViewDropCanvas _viewDropCanvas;

        private Button button;
        private Animator animator;

        private Image bgEnabledImage;
        private Image bgDisabledImage;

        private Image handleEnabledImage;
        private Image handleDisabledImage;

        private bool switchEnabled;

        private readonly CompositeDisposable _disposables = new();

        private void Awake()
        {
            button = GetComponent<Button>();
            animator = GetComponent<Animator>();

            bgEnabledImage = transform.GetChild(0).GetChild(0).GetComponent<Image>();
            bgDisabledImage = transform.GetChild(0).GetChild(1).GetComponent<Image>();
            handleEnabledImage = transform.GetChild(1).GetChild(0).GetComponent<Image>();
            handleDisabledImage = transform.GetChild(1).GetChild(1).GetComponent<Image>();
        }

        private void Start()
        {
            // 初期値の反映（SubscribeはOnEnableで張る）
            switch (_source)
            {
                case SourceType.GuideEnabled:
                    if (GuideManager.Instance != null && GuideManager.Instance.GuideEnabled != null)
                        switchEnabled = GuideManager.Instance.GuideEnabled.CurrentValue;
                    break;

                case SourceType.DropChoseEnabled:
                    var vdc = ResolveViewDropCanvas();
                    if (vdc != null && vdc.DropChoseEnabled != null)
                        switchEnabled = vdc.DropChoseEnabled.CurrentValue;
                    break;
            }
            UpdateObjects();
        }

        private void OnEnable()
        {
            // クリックの自己反転は行わない（見た目は購読で同期）
            // → インスペクタのButton.onClickに GuideManager/ ViewDropCanvas の既存関数を割当てて使う

            // 購読開始
            _disposables.Clear();
            switch (_source)
            {
                case SourceType.GuideEnabled:
                    if (GuideManager.Instance == null || GuideManager.Instance.GuideEnabled == null)
                    {
                        Debug.LogWarning("[Switch] GuideManager が見つからないため購読できません。");
                        return;
                    }
                    GuideManager.Instance.GuideEnabled
                        .Subscribe(v =>
                        {
                            switchEnabled = v;
                            UpdateObjects();
                        })
                        .AddTo(_disposables);
                    break;

                case SourceType.DropChoseEnabled:
                    var vdc = ResolveViewDropCanvas();
                    if (vdc == null || vdc.DropChoseEnabled == null)
                    {
                        Debug.LogWarning("[Switch] ViewDropCanvas が見つからないため購読できません。");
                        return;
                    }
                    vdc.DropChoseEnabled
                        .Subscribe(v =>
                        {
                            switchEnabled = v;
                            UpdateObjects();
                        })
                        .AddTo(_disposables);
                    break;
            }
        }

        private void OnDisable()
        {
            _disposables.Dispose(); // 購読解除
        }

        // （レガシー互換）外部から見た目だけ切り替えたい時用に残すが、通常は使わない。
        public void Toggle()
        {
            switchEnabled = !switchEnabled;
            UpdateObjects();
        }

        public bool IsToggled() => switchEnabled;

        private void UpdateObjects()
        {
            if (bgDisabledImage == null) return;

            if (switchEnabled)
            {
                bgDisabledImage.gameObject.SetActive(false);
                bgEnabledImage.gameObject.SetActive(true);
                handleDisabledImage.gameObject.SetActive(false);
                handleEnabledImage.gameObject.SetActive(true);
            }
            else
            {
                bgEnabledImage.gameObject.SetActive(false);
                bgDisabledImage.gameObject.SetActive(true);
                handleEnabledImage.gameObject.SetActive(false);
                handleDisabledImage.gameObject.SetActive(true);
            }
            if (animator != null)
                animator.SetTrigger(switchEnabled ? "Enable" : "Disable");
        }

        private ViewDropCanvas ResolveViewDropCanvas()
        {
            if (_viewDropCanvas != null) return _viewDropCanvas;
            _viewDropCanvas = FindObjectOfType<ViewDropCanvas>();
            return _viewDropCanvas;
        }
    }
}