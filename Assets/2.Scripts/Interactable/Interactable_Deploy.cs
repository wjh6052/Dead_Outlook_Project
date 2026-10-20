using UnityEngine;

// 필드 이동 상호작용 — 은신처에서 필드 씬으로 전환
public class Interactable_Deploy : Interactable
{
    [Header("필드 이동")]
    [SerializeField] private EScene TargetScene = EScene.Field; // 필드 이동시 이동할 씬

    public override void Interact()
    {
        Mgr_Game.Inst.ChangeScene(TargetScene);
    }
}
