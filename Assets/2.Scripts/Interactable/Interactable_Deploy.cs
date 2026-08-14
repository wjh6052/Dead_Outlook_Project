using UnityEngine;

// 출격 상호작용 — 은신처에서 필드 씬으로 전환
public class Interactable_Deploy : Interactable
{
    public override void Interact()
    {
        Mgr_Game.Inst.ChangeScene(EScene.Field);
    }
}
