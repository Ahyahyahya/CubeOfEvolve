using UnityEngine;
using DG.Tweening;

public class UIScaleLoop : MonoBehaviour
{
    [SerializeField] private RectTransform target; // 対象UI
    [SerializeField] private float minScale = 1.0f;
    [SerializeField] private float maxScale = 1.5f;
    [SerializeField] private float duration = 0.5f; // 拡大・縮小にかかる時間

    private Tween scaleTween;

    private void Start()
    {
        if (target == null)
        {
            target = GetComponent<RectTransform>();
        }

        // 無限ループのスケールアニメーション
        scaleTween = target.DOScale(maxScale, duration)
            .SetLoops(-1, LoopType.Yoyo) // 無限に拡大縮小を繰り返す
            .SetEase(Ease.InOutSine);    // 滑らかな動きにする
    }

    private void OnDestroy()
    {
        // シーン転換やオブジェクト破棄時にTweenを停止
        if (scaleTween != null && scaleTween.IsActive())
        {
            scaleTween.Kill();
        }
    }
}
