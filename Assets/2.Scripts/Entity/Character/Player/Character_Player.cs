using System.Collections;
using UnityEngine;


public class Character_Player : MonoBehaviour, IDamageable, IInventory
{
    #region 스텟 변수 ---------------------------------------
    [Header("스텟")]
    [field: SerializeField] public float MaxHP { get; private set; } = 100f;
    [field: SerializeField] public float HP { get; private set; }

    #endregion ---------------------------------------


    #region 카메라 ---------------------------------------
    [Header("카메라")]
    private float CameraStandHeight;
    public  float CameraCrouchHeight = 0.9f;
    public  float CameraHeightSpeed  = 10f;
    public Transform CameraTarget { get; private set; }
    #endregion ---------------------------------------


    #region 컴포넌트 ---------------------------------------

    public PlayerMovement     Movement      { get; private set; }
    public PlayerAnimator     Animator      { get; private set; }
    public InventoryComponent InventoryComp { get; private set; }
    
    public static Character_Player Inst { get; private set; }
    #endregion ---------------------------------------

    private void Awake()
    {
        Inst          = this;
        Movement      = GetComponent<PlayerMovement>();
        Animator      = GetComponent<PlayerAnimator>();
        InventoryComp = GetComponent<InventoryComponent>();
        MeshComp      = GetComponent<MeshComponent>();
        CameraTarget  = transform.Find("CameraTarget");
        HP            = MaxHP;
        CameraStandHeight = CameraTarget.localPosition.y;
    }

    private void Start()
    {
        Mgr_PlayerController.Inst.SetPlayer(this);
    }


    public void TakeDamage(float damage)
    {
        HP = Mathf.Max(HP - damage, 0f);
        if (HP <= 0f) OnDeath();
    }

    public void Heal(float amount)
    {
        HP = Mathf.Min(HP + amount, MaxHP);
    }

    private void OnDeath()
    {
        Debug.Log($"플레이어 죽음 / 현재 체력:{HP}");
    }


    #region 웅크림
    // PlayerMovement에서 웅크림 상태 전달 — 카메라 높이 코루틴 시작
    public void SetCrouching(bool isCrouching)
    {
        float target = isCrouching ? CameraCrouchHeight : CameraStandHeight;
        StopAllCoroutines();
        StartCoroutine(LerpCameraHeight(target));
    }

    private IEnumerator LerpCameraHeight(float targetHeight)
    {
        while (Mathf.Abs(CameraTarget.localPosition.y - targetHeight) > 0.001f)
        {
            Vector3 pos = CameraTarget.localPosition;
            pos.y = Mathf.Lerp(pos.y, targetHeight, CameraHeightSpeed * Time.deltaTime);
            CameraTarget.localPosition = pos;
            yield return null;
        }

        Vector3 final = CameraTarget.localPosition;
        final.y = targetHeight;
        CameraTarget.localPosition = final;
    }
    #endregion


    #region IInventory ---------------------------------------

    [field: SerializeField] public int CurrentWeaponSlot { get; private set; } // 현재 선택된 무기 슬롯 (1·2=무기, 3=맨손)
    public Weapon CurrentWeapon { get; private set; } // 현재 손에 들고 있는 무기
    public MeshComponent MeshComp { get; private set; }


    // 무기 슬롯 선택 — 1·2번은 해당 슬롯 무기 장착, 3번은 해제
    public void OnWeaponSlot(int slot)
    {
        if (CurrentWeaponSlot == slot) return;

        CurrentWeaponSlot = slot;

        WeaponSlotUpdate();
    }

    public void WeaponSlotUpdate()
    {
        // 현재 손에 무기가 있으면 홀스터 애니메이션 먼저 재생
        // → 애니메이션 끝 이벤트(OnHolsterEnd)에서 무기를 소켓으로 복귀 후 EquipCurrentSlot() 호출
        if (CurrentWeapon != null)
        {
            Animator.PlayHolsterWeapon();
            return;
        }

        // 맨손 상태에서 교체 → 바로 장착
        EquipCurrentSlot();
    }

    // CurrentWeaponSlot에 맞는 무기를 꺼냄 — WeaponSlotUpdate와 OnHolsterEnd에서 호출
    private void EquipCurrentSlot()
    {
        // TakeWeapon 애니메이션 시작 전 모든 무기를 소켓 위치로 초기화
        if (InventoryComp.Weapon_1 != null)
            InventoryComp.Weapon_1.transform.SetParent(MeshComp.WeaponSocket_1, false);
        if (InventoryComp.Weapon_2 != null)
            InventoryComp.Weapon_2.transform.SetParent(MeshComp.WeaponSocket_2, false);

        switch (CurrentWeaponSlot)
        {
            case 1: CurrentWeapon = InventoryComp.Weapon_1; if (CurrentWeapon != null) Animator.PlayTakeWeapon(); break;
            case 2: CurrentWeapon = InventoryComp.Weapon_2; if (CurrentWeapon != null) Animator.PlayTakeWeapon(); break;
            case 3: CurrentWeapon = null;                                                                          break;
        }

        if (CurrentWeapon == null)
            Animator.SetWeaponType(0, 0); // 맨손
    }

    // HolsterWeapon 애니메이션 끝 프레임 이벤트로 호출
    // → 손에 든 무기를 소켓으로 복귀 후 새 슬롯 장착
    public void OnHolsterEnd()
    {
        // 손에 있던 무기를 원래 소켓으로 복귀 + IK 리셋
        if (CurrentWeapon != null)
        {
            Transform socket = CurrentWeapon == InventoryComp.Weapon_1
                ? MeshComp.WeaponSocket_1
                : MeshComp.WeaponSocket_2;
            CurrentWeapon.transform.SetParent(socket, false);

            MeshComp.Target_Hand_L.SetParent(MeshComp.StartTarget_Hand_L, false);
            MeshComp.Target_Hand_L.localPosition = Vector3.zero;
            MeshComp.Target_Hand_L.localRotation = Quaternion.identity;

            Animator.SetWeaponType(0, 0);
            CurrentWeapon = null;
        }

        // CurrentWeaponSlot에 맞는 다음 무기 장착 (슬롯 3이면 맨손으로 종료)
        EquipCurrentSlot();
    }

    // 애니메이션 이벤트로 호출 — "true" 무기를 손으로 이동 + 애니메이터 파라미터 갱신 / "false" 홀스터로 복귀
    public void TakeWeapon(string attach)
    {
        if (attach == "true")
        {
            if (CurrentWeapon == null) return;
            CurrentWeapon.transform.SetParent(MeshComp.HandSocket_R, false);

            int weaponType      = (int)CurrentWeapon.WeaponData.WeaponType + 1;
            int weaponMeleeType = CurrentWeapon.WeaponData is Item_Weapon_Melee melee ? (int)melee.WeaponMeleeType : 0;
            Animator.SetWeaponType(weaponType, weaponMeleeType);

            if (CurrentWeapon == null || CurrentWeapon.GripPoint_L == null) return;
            MeshComp.Target_Hand_L.SetParent(CurrentWeapon.GripPoint_L, false);
            MeshComp.Target_Hand_L.localPosition = Vector3.zero;
            MeshComp.Target_Hand_L.localRotation = Quaternion.identity;
        }
        else
        {
            if (CurrentWeapon == null) return;
            Transform socket = CurrentWeaponSlot == 1 ? MeshComp.WeaponSocket_1 : MeshComp.WeaponSocket_2;
            CurrentWeapon.transform.SetParent(socket, false);
            Animator.SetWeaponType(0, 0);

            MeshComp.Target_Hand_L.SetParent(MeshComp.StartTarget_Hand_L, false);
            MeshComp.Target_Hand_L.localPosition = Vector3.zero;
            MeshComp.Target_Hand_L.localRotation = Quaternion.identity;
        }

    }


    #endregion ---------------------------------------



}
