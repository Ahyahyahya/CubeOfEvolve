using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace ScreenPocket
{
    public class Outline8 : Outline
    {
        protected Outline8()
        { }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
                return;

            //Outline�̏������I��点�Ă���
            base.ModifyMesh(vh);

            var verts = ListPool<UIVertex>.Get();
            vh.GetUIVertexStream(verts);

            var outline4VertexCount = verts.Count;
            //�{�`��̒��_����ێ�
            var baseCount = outline4VertexCount / 5;

            var neededCapacity = baseCount * 9;//9��(�{�`��+8����)�̃L���p���擾
            if (verts.Capacity < neededCapacity)
                verts.Capacity = neededCapacity;

            //���炵�����擾���Ă���
            var length = effectDistance.magnitude;

            //��
            var start = outline4VertexCount - baseCount;
            var end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, end, 0f, length);

            //�E
            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, end, length, 0f);

            //��
            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, end, 0f, -length);

            //��
            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, end, -length, 0f);

            vh.Clear();
            vh.AddUIVertexTriangleStream(verts);
            ListPool<UIVertex>.Release(verts);
        }
    }
}

