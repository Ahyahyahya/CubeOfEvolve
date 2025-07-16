using App.GameSystem.Modules;
using UnityEngine;

public class Item_Blueprint : ItemBase
{
    public override void UseItem(PlayerCore playerCore)
    {
        RuntimeModuleManager.Instance.TriggerDropUI();

        // �K�C�h�\���i�݌v�}�A�C�e���������莞�j
        if (GuideManager.Instance.GuideEnabled.CurrentValue && !GuideManager.Instance.HasShown("Blueprint"))
        {
            GuideManager.Instance.TryShowGuide("Blueprint");
        }
    }
}