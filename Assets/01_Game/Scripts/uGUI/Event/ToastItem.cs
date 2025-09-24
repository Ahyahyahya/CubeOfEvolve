using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public sealed class ToastItem : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _name;
    [SerializeField] private TMP_Text _level;
    [SerializeField] private CanvasGroup _cg;
    [SerializeField] private RectTransform _rt;

    [SerializeField, Tooltip("レイアウト計算用に固定高さ Prefabと同じ値にすること")] private float _fixedHeight = 80f;

    public float Height => _fixedHeight;
    public RectTransform RT => _rt;

    Sequence _seq;

    void Reset()
    {
        _rt = GetComponent<RectTransform>();
        _cg = GetComponent<CanvasGroup>();
    }

    public void Setup(string displayName, string lv, Sprite icon)
    {
        if (_name) _name.text = displayName ?? "";
        if (_level) _level.text = lv ?? "";
        if (_icon) _icon.sprite = icon;
    }

    public void PrepareOffscreen(float startX, float targetY, float startAlpha = 0f)
    {
        KillTweens();
        _rt.anchoredPosition = new Vector2(startX, targetY);
        _cg.alpha = startAlpha;
    }

    public void KillTweens()
    {
        _seq?.Kill();
        _seq = null;
        DOTween.Kill(_rt);
        DOTween.Kill(_cg);
    }

    public Tween PlayIn(float inDuration, float targetX = 0f)
    {
        KillTweens();
        // Update(true) で Time.timeScale 非依存（ゲームがポーズでも動く）
        var move = _rt.DOAnchorPosX(targetX, inDuration).SetEase(Ease.OutCubic).SetUpdate(true);
        var fade = _cg.DOFade(1f, inDuration).SetUpdate(true);
        _seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
        _seq.Join(move).Join(fade);
        return _seq;
    }

    public Tween PlayOut(float outDuration, float outX)
    {
        KillTweens();
        var move = _rt.DOAnchorPosX(outX, outDuration).SetEase(Ease.InCubic).SetUpdate(true);
        var fade = _cg.DOFade(0f, outDuration).SetUpdate(true);
        _seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
        _seq.Join(move).Join(fade);
        return _seq;
    }

    public Tween MoveToY(float newY, float duration)
    {
        // Yだけスッと詰める
        return _rt.DOAnchorPosY(newY, duration).SetEase(Ease.OutCubic).SetUpdate(true);
    }
}
