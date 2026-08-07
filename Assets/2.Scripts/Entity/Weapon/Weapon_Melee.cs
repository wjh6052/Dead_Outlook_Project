using System.Collections.Generic;
using UnityEngine;

public class Weapon_Melee : Weapon
{
    private bool IsHitBoxOn = false; // 히트박스 활성 여부

    public override void Init(Item_Weapon weaponData, GameObject owner)
    {
        base.Init(weaponData, owner);
        OnCollider(false); // 초기 상태는 히트박스 비활성
    }

    private HashSet<Collider> HitTargets = new(); // 이번 스윙에서 이미 피격된 대상 — 중복 피격 방지

    // isOn true: 히트박스 활성화 + 피격 목록 초기화 / false: 히트박스 비활성화
    public void OnCollider(bool isOn)
    {
        IsHitBoxOn = isOn;
        if (isOn) HitTargets.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsHitBoxOn) return;                              // 히트박스 비활성 상태면 무시
        if (other.transform.IsChildOf(Owner.transform)) return; // 오너 자신은 무시
        if (HitTargets.Contains(other)) return;               // 이번 스윙에서 이미 피격된 대상이면 무시

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null) return;

        HitTargets.Add(other);
        damageable.TakeDamage(WeaponData.Damage, gameObject, Owner);
    }
}
