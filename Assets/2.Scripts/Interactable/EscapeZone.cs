using UnityEngine;

// 필드 → 은신처 귀환 존 — 플레이어가 일정 시간 머물면 씬 전환
public class EscapeZone : MonoBehaviour
{
    [SerializeField] private FTimer Escape_Timer; // 귀환 타이머 (Max = 대기 시간)

    private bool IsPlayerInZone = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        IsPlayerInZone = true;
        Mgr_UI.Inst.EscapeTimer.Init(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        IsPlayerInZone = false;
        Escape_Timer.Reset();
        Mgr_UI.Inst.EscapeTimer.Init(false);
    }

    private void Update()
    {
        if (!IsPlayerInZone) return;

        Escape_Timer.Tick();
        Mgr_UI.Inst.EscapeTimer.UpdateTimer(Escape_Timer);

        if (Escape_Timer.IsReached)
        {
            IsPlayerInZone = false;
            Escape_Timer.Reset();
            Mgr_UI.Inst.EscapeTimer.Init(false);
            Mgr_Game.Inst.ChangeScene(EScene.Hideout);
        }
    }
}
