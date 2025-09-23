using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public sealed class ViewAutoSelectToast : MonoBehaviour
{
    [Header("Container (右端中央アンカー)")]
    [SerializeField] private RectTransform _container;

    [Header("Toast Prefab")]
    [SerializeField] private ToastItem _toastPrefab;

    [Header("Layout")]
    [SerializeField, Min(0f)] private float _gapY = 8f;
    [SerializeField] private float _xOffset = -24f; // 画面右端からの内側オフセット（負値で内側）

    [Header("Timings")]
    [SerializeField, Min(0f)] private float _inDuration = 0.18f;
    [SerializeField, Min(0f)] private float _outDuration = 0.18f;
    [SerializeField, Min(0f)] private float _holdSeconds = 2.0f;
    [SerializeField, Min(0f)] private float _packDuration = 0.18f;

    [Header("Limits")]
    [SerializeField, Min(1)] private int _maxVisible = 5;

    class Entry
    {
        public ToastItem item;
        public float targetY;
        public CancellationTokenSource cts;
    }

    readonly List<Entry> _active = new();
    readonly Stack<ToastItem> _pool = new();

    void Reset()
    {
        _container = GetComponent<RectTransform>();
    }

    ToastItem Rent()
    {
        var it = _pool.Count > 0 ? _pool.Pop() : Instantiate(_toastPrefab, _container);
        it.gameObject.SetActive(true);
        return it;
    }

    void Return(ToastItem it)
    {
        if (it == null) return;
        it.KillTweens();
        it.gameObject.SetActive(false);
        _pool.Push(it);
    }

    float CalcYByIndex(int index, float itemHeight)
    {
        // 右端「中央」を基点に、下方向をプラスとする
        return -index * (itemHeight + _gapY);
    }

    // 外向けAPI：名前とアイコンを渡して2秒ポップ
    public void Show(string displayName, string level, Sprite icon)
    {
        // 古いものを強制クローズ（最大数制限）
        if (_active.Count >= _maxVisible)
        {
            ForceClose(_active[0]);
        }

        var item = Rent();
        item.Setup(displayName, level, icon);

        var entry = new Entry { item = item, cts = new CancellationTokenSource() };
        var index = _active.Count;
        var height = item.Height;
        entry.targetY = CalcYByIndex(index, height);

        // 右端外から入場
        var rt = _container;
        var width = (item.RT.rect.width > 0f) ? item.RT.rect.width : 360f; // 未レイアウト時の保険
        float startX = _xOffset + width; // 右外へ
        item.PrepareOffscreen(startX, entry.targetY, 0f);

        _active.Add(entry);

        RunLifecycle(entry, startX).Forget(); // 火だけ付けて即return（非同期で管理）
    }

    async UniTaskVoid RunLifecycle(Entry entry, float offscreenX)
    {
        var item = entry.item;
        try
        {
            // 入場
            await item.PlayIn(_inDuration, _xOffset).AsyncWaitForCompletion();

            // 表示保持（TimeScale非依存）
            await UniTask.Delay(TimeSpan.FromSeconds(_holdSeconds), DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update, entry.cts.Token);

            // 退出
            await item.PlayOut(_outDuration, offscreenX).AsyncWaitForCompletion();
        }
        catch (OperationCanceledException)
        {
            // キャンセルでも問題なし
        }
        finally
        {
            // リストから外してプールへ
            int removedIndex = _active.IndexOf(entry);
            if (removedIndex >= 0) _active.RemoveAt(removedIndex);
            Return(item);
            entry.cts?.Dispose();

            // 残りを上へ詰める
            PackFrom(removedIndex);
        }
    }

    void ForceClose(Entry entry)
    {
        if (entry == null) return;
        entry.cts?.Cancel();
        // 即座にフェードアウト演出を入れたい場合はここで PlayOut を呼ぶ
    }

    void PackFrom(int removedIndex)
    {
        // 取り除かれた位置より下にあるものを上へ詰める
        if (removedIndex < 0) removedIndex = 0;

        for (int i = removedIndex; i < _active.Count; i++)
        {
            var e = _active[i];
            var h = e.item.Height;
            var newY = CalcYByIndex(i, h);
            e.targetY = newY;
            e.item.MoveToY(newY, _packDuration);
        }
    }

    // 明示的全クリア（シーン遷移等）
    public void ClearAll()
    {
        foreach (var e in _active)
        {
            e.cts?.Cancel();
            Return(e.item);
        }
        _active.Clear();
    }

    void OnDestroy() => ClearAll();
}
