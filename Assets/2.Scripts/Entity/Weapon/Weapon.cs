using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Item_Weapon WeaponData;

    [Header("IK 그립 포인트")]
    public Transform GripPoint_L; // 왼손 그립 위치

    public virtual void Init(Item_Weapon weaponData)
    {
        WeaponData = weaponData;

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
    }

    public virtual void Attack() { }
}
