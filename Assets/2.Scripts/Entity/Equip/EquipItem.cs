using UnityEngine;

// 장착 아이템 공통 부모 — 무기 · 가방 · 조끼 · 벨트처럼 캐릭터 소켓에 붙는 아이템 오브젝트
// - 장착 초기화 (줍기 · 물리 · 콜라이더 · 레이어), 소켓 부착, 이름으로 자식 찾기
public class EquipItem : MonoBehaviour
{
    [Tooltip("이 오브젝트의 아이템 데이터")]
    public Item       ItemData; // 이 오브젝트의 아이템 데이터
    [Tooltip("이 아이템을 장착한 캐릭터 오브젝트")]
    public GameObject Owner; // 이 아이템을 장착한 캐릭터 오브젝트

    [Header("홀스터 포인트")]
    [Tooltip("캐릭터 소켓에 붙을 때 맞출 위치 — 이름이 HolsterPoint인 자식을 자동으로 찾음")]
    public Transform HolsterPoint; // 소켓에 붙을 때 맞출 위치 (비어 있으면 원점 기준)

    // Instantiate 직후 바로 호출됨 — 생성 직후 AttachToHolster에서 HolsterPoint를 쓰므로 Start가 아닌 Awake에서 연결
    protected virtual void Awake()
    {
        // 자식 오브젝트 중 변수와 같은 이름을 찾아 연결 — 인스펙터에서 직접 넣은 값이 있으면 그대로 사용
        if (HolsterPoint == null) HolsterPoint = FindChild(nameof(HolsterPoint));
    }

    // 장착 초기화 — 바닥 아이템 기능을 끄고 캐릭터에 붙을 수 있는 상태로 만듦 (자식 클래스에서 덮어쓰기)
    public virtual void InitEquip(Item item, GameObject owner)
    {
        ItemData = item;
        Owner    = owner;

        // 장착된 아이템은 월드 상호작용 불가
        Interactable_Item interactable = GetComponent<Interactable_Item>();
        if (interactable != null) interactable.IsDisabled = true;

        // 물리 비활성화 — 소켓에 부착된 상태에서 떠내려가지 않도록
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity  = false;
            rb.isKinematic = true;
        }

        SetupColliders();

        // 자식 포함 모든 오브젝트 레이어를 Default(0)로 설정
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
            child.gameObject.layer = 0;
    }

    // 장착 시 콜라이더 처리 — 기본은 비활성화 (플레이어 이동 · 시야 판정에 걸리지 않도록), 자식 클래스에서 덮어쓰기
    protected virtual void SetupColliders()
    {
        foreach (Collider col in GetComponentsInChildren<Collider>())
            col.enabled = false;
    }

    // 소켓에 부착 — point가 있으면 point가 소켓 위치 · 방향에 오도록 보정, 없으면 원점을 소켓에 맞춤
    public void AttachToSocket(Transform socket, Transform point = null)
    {
        transform.SetParent(socket, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (point == null) return; // 기준점이 없으면 원점 기준

        // point의 방향이 소켓과 같아지도록 오브젝트를 반대로 회전
        Quaternion pointRotation = Quaternion.Inverse(socket.rotation) * point.rotation;
        transform.localRotation  = Quaternion.Inverse(pointRotation);

        // 회전 후 point가 소켓 원점에 오도록 오브젝트를 반대로 이동
        transform.localPosition = -socket.InverseTransformPoint(point.position);
    }

    // 몸 소켓에 부착 — HolsterPoint가 소켓 위치 · 방향에 오도록 보정
    public void AttachToHolster(Transform socket)
    {
        AttachToSocket(socket, HolsterPoint); // 홀스터 포인트가 없으면 원점 기준
    }

    // 하위 전체에서 이름이 같은 오브젝트를 찾음 (깊이 상관없음) — 없으면 null
    protected Transform FindChild(string childName)
    {
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
            if (child.name == childName) return child;

        return null;
    }
}
