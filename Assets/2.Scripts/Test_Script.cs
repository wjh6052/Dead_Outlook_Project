using UnityEngine;

// 테스트용 데미지 구역 — 콜라이더(Is Trigger) 안에 플레이어가 있는 동안 일정 간격으로 데미지
// - 들어가고 간격만큼 기다린 뒤 첫 데미지, 이후 간격마다 반복
public class Test_Script : MonoBehaviour
{
    [Header("데미지 구역")]
    [Tooltip("한 번에 주는 데미지")]
    [SerializeField] private float  Damage = 10f;
    [Tooltip("데미지 간격 타이머 (Max = 데미지를 주는 간격, 초)")]
    [SerializeField] private FTimer Damage_Timer;

    private Character_Player PlayerInZone; // 구역 안에 있는 플레이어 (없으면 null)

    private void OnTriggerEnter(Collider other)
    {
        Character_Player player = other.GetComponentInParent<Character_Player>();
        if (player == null) return;

        PlayerInZone = player;
        Damage_Timer.Reset(); // 들어간 순간부터 간격 재기
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<Character_Player>() != PlayerInZone) return;
        PlayerInZone = null;
    }

    private void Update()
    {
        if (PlayerInZone == null) return;

        Damage_Timer.Tick(); // 간격 시간 누적
        if (!Damage_Timer.IsReached) return;

        PlayerInZone.TakeDamage(Damage, gameObject, gameObject);
        Damage_Timer.Reset(); // 다음 데미지까지 다시 기다림
    }
}
