using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;

public class BubbleToggleAnimator : MonoBehaviour
{
    [Header(" �����o��UI (RectTransform)")]
    [SerializeField] private RectTransform bubbleRect;

    [Header(" �؂�ւ��{�^�� (Button)")]
    [SerializeField] private Button toggleButton;

    [Header(" �\���ʒu�iB�j")]
    [SerializeField] private Vector2 showPosition;

    [Header(" ��\���ʒu�iA�j")]
    [SerializeField] private Vector2 hidePosition;

    [Header(" �A�j���[�V�������ԁi�b�j")]
    [SerializeField] private float moveDuration = 0.3f;

    [Header(" �{�^���̌����ڔ��] (Z��180�x)")]
    [SerializeField] private RectTransform buttonIconRect; // ��������]���镔��

    private bool isShown = false;
    private CancellationTokenSource cancelTokenSource;

    private void Awake()
    {
        if (bubbleRect == null) Debug.LogError(" bubbleRect �����ݒ�ł�");
        if (toggleButton == null) Debug.LogError(" toggleButton �����ݒ�ł�");
        if (buttonIconRect == null) Debug.LogWarning(" buttonIconRect �����ݒ�ł��iZ��]�͖����ɂȂ�܂��j");
    }

    private void Start()
    {
        if (bubbleRect == null || toggleButton == null) return;

        // �ŏ��ɔ�\���ʒu�ɔz�u
        bubbleRect.anchoredPosition = hidePosition;
        isShown = false;
        UpdateButtonVisual();

        toggleButton.onClick.AddListener(() => ToggleAsync().Forget());
    }

    private async UniTaskVoid ToggleAsync()
    {
        cancelTokenSource?.Cancel();
        cancelTokenSource = new CancellationTokenSource();

        isShown = !isShown;
        Vector2 targetPos = isShown ? showPosition : hidePosition;

        Debug.Log($"[Moving to] {targetPos} (TimeScale={Time.timeScale})");

        await bubbleRect.DOAnchorPos(targetPos, moveDuration)
            .SetEase(Ease.InOutQuad)
            .SetUpdate(true) // �� ���ꂪ�d�v�I
            .WithCancellation(cancelTokenSource.Token);

        UpdateButtonVisual();
    }





    private void UpdateButtonVisual()
    {
        if (buttonIconRect != null)
        {
            float z = isShown ? 180f : 0f;
            buttonIconRect.localRotation = Quaternion.Euler(0f, 0f, z);
        }
    }

    private void OnDestroy()
    {
        cancelTokenSource?.Cancel();
    }
}
