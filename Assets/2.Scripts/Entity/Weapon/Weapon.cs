using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Item_Weapon WeaponData;
    public GameObject  Owner; // 이 무기를 장착한 캐릭터 오브젝트

    [Header("IK 그립 포인트")]
    public Transform GripPoint_L; // 왼손 그립 위치

    public virtual void Init(Item_Weapon weaponData, GameObject owner)
    {
        WeaponData = weaponData;
        Owner      = owner;

        // 장착된 무기는 월드 상호작용 불가
        Interactable_Item interactable = GetComponent<Interactable_Item>();
        if (interactable != null) interactable.IsDisabled = true;

        // 물리 비활성화 — 소켓에 부착된 상태에서 떠내려가지 않도록
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity  = false;
            rb.isKinematic = true;
        }

        // 오브젝트의 모든 콜라이더를 트리거로 설정 — 히트박스 감지용
        foreach (Collider col in GetComponentsInChildren<Collider>())
            col.isTrigger = true;

        // 자식 포함 모든 오브젝트 레이어를 Default(0)로 설정
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
            child.gameObject.layer = 0;
    }

}
