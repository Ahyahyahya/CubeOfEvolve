using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using R3;
using R3.Triggers;
using DG.Tweening;
using System.Linq;

public class PlayerEffecter : BasePlayerComponent
{
    // ---------- SerializeField
    [SerializeField, Tooltip("�_���[�W�G�t�F�N�g�p�̃{�����[��")]
    private Volume _damageVolume;
    [SerializeField, Tooltip("�_���[�W�G�t�F�N�g�\������")]
    private float _damageEffectSec;

    // ---------- Method
    protected override void OnInitialize()
    {
        // �_���[�W�G�t�F�N�g�p��Vignette���擾
        _damageVolume.profile.TryGet(out Vignette damageVignette);

        // null�`�F�b�N
        if (damageVignette != null)
        {
            Debug.LogError("Vignette��������");
        }

        // ��_���[�W�G�t�F�N�g����
        Core.Hp
            .Chunk(2,1)
            .Where(x => x.Last() < x.First())
            .Subscribe(_ =>
            {
                // ��ʒ[���_���[�W�G�t�F�N�g�\�����ԕ��Ԃ��Ȃ�
                DOVirtual.Float(
                    0.0f,
                    1.0f,
                    _damageEffectSec,
                    value => damageVignette.smoothness.value = value
                    )
                .SetLoops(2, LoopType.Yoyo)
                .SetLink(this.gameObject);
            })
            .AddTo(this);
    }
}
