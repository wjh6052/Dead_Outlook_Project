using UnityEngine;

// 무기 — EquipItem을 상속, 무기 데이터 · 왼손 그립 · 손에 쥐기 · 히트박스 콜라이더
public class EquipItem_Weapon : EquipItem
{
    public Item_Weapon WeaponData => ItemData as Item_Weapon; // 이 무기의 아이템 데이터 — 부모의 ItemData를 무기 데이터로 캐스팅

    [Header("IK 그립 포인트")]
    [Tooltip("왼손 그립 위치 — 이름이 GripPoint_L인 자식을 자동으로 찾음")]
    public Transform GripPoint_L; // 왼손 그립 위치

    // Instantiate 직후 바로 호출됨 — 생성 직후 장착 처리에서 쓰므로 Start가 아닌 Awake에서 연결
    protected override void Awake()
    {
        base.Awake(); // HolsterPoint 연결 (EquipItem)

        // 자식 오브젝트 중 변수와 같은 이름을 찾아 연결 — 인스펙터에서 직접 넣은 값이 있으면 그대로 사용
        if (GripPoint_L  == null) GripPoint_L  = FindChild(nameof(GripPoint_L));
    }

    // 무기 콜라이더는 끄지 않고 트리거로 — 히트박스 감지용
    protected override void SetupColliders()
    {
        // 오브젝트의 모든 콜라이더를 트리거로 설정 — 히트박스 감지용
        foreach (Collider col in GetComponentsInChildren<Collider>())
            col.isTrigger = true;
    }

    // 손 소켓에 부착 — 무기 원점(손잡이)을 소켓에 맞춤
    public void AttachToHand(Transform handSocket)
    {
        AttachToSocket(handSocket); // 기준점 없이 원점 기준 — 등에서 보정된 위치가 남지 않도록 초기화됨
    }
}
