using System.Collections;
using UnityEngine;

public enum EFocusType
{
    Medical, // 치료 중
}

public class Character_Player : MonoBehaviour, IDamageable, IInventory
{
    #region 스텟 변수 ---------------------------------------

    [field: Header("스텟")]
    [field: Tooltip("최대 체력")]
    [field: SerializeField] public float MaxHP      { get; private set; } = 100f;
    [field: Tooltip("현재 체력")]
    [field: SerializeField] public float HP         { get; private set; }
    [Tooltip("아이템 사용 집중 중 여부")]
    public bool IsFocusing; // 아이템 사용 집중 중
    public EFocusType FocusType { get; private set; } // 아이템 사용 집중 중 — 집중 중인 행동 종류 (치료 등)


    #endregion


    #region 카메라 ---------------------------------------
    [Header("카메라")]
    [Tooltip("웅크렸을 때 카메라 높이")]
    public  float CameraCrouchHeight = 0.9f;
    [Tooltip("서기 ↔ 웅크리기 카메라 높이 전환 속도")]
    public  float CameraHeightSpeed  = 10f;
    private float CameraStandHeight;                    // 서 있을 때 카메라 높이 — 시작 시 CameraTarget 높이로 저장
    public Transform CameraTarget { get; private set; } // 카메라가 따라가는 지점 (자식 오브젝트 CameraTarget)
    #endregion 


    #region 컴포넌트 ---------------------------------------

    public PlayerMovement     Movement      { get; private set; } // 이동 · 자세 · 회전 · 루트 모션
    public PlayerAnimator     Animator      { get; private set; } // 애니메이터 파라미터 · 트리거
    public InventoryComponent InventoryComp { get; private set; } // 인벤토리 · 무기 장착
    public MeshComponent      MeshComp      { get; private set; } // 뼈 · 무기 소켓 · IK 참조

    public static Character_Player Inst { get; private set; } // 플레이어 싱글톤
    #endregion 

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


    #region IDamageable ---------------------------------------

    public void TakeDamage(float damage, GameObject attacker, GameObject attackerOwner)
    {
        HP = Mathf.Max(HP - damage, 0f);
        Debug.Log($"[{name}] {attacker.name}에게 {damage} 데미지 — 남은 HP: {HP}/{MaxHP}");
        if (HP <= 0f) OnDeath();
    }

    // 아이템 사용 — 타입별로 처리, 사용 성공 시 true 반환
    public bool UseItem(Item item)
    {
        switch (item.ItemType)
        {
            case EItemType.Food:
                return true;
            case EItemType.Medical:
                if(MaxHP <= HP) return false;
                Item_Medical medical = item as Item_Medical;
                StartCoroutine(HealCoroutine(medical.HealAmount, medical.UseTime, item));
                return true;
        }

        return false;
    }

    public void Heal(float amount)
    {
        HP = Mathf.Min(HP + amount, MaxHP);
    }

    // UseTime초 동안 집중 후 아이템 소모 + HealAmount만큼 회복
    private IEnumerator HealCoroutine(float healAmount, float useTime, Item item)
    {
        yield return StartCoroutine(FocusCoroutine(EFocusType.Medical, useTime, () =>
        {
            item.DecreaseAmount(1);
            Heal(healAmount);
        }));
    }

    // useTime초 동안 집중 타이머 UI를 표시하고, 완료 시 onComplete 호출
    // 도중에 IsFocusing이 false가 되면 취소
    private IEnumerator FocusCoroutine(EFocusType focusType, float useTime, System.Action onComplete = null)
    {
        if (IsFocusing) yield break; // 이미 집중 중이면 취소

        IsFocusing = true;
        FocusType  = focusType;
        Animator.UpdateFocus();

        // 집중 타이머 UI 활성화
        UI_FocusTimer focusTimer = Mgr_UI.Inst.FocusTimer;
        focusTimer.gameObject.SetActive(true);
        focusTimer.Init(useTime);

        float elapsed = 0f;
        while (elapsed < useTime)
        {
            if (!IsFocusing)                           // 외부에서 집중 취소 → UI 비활성 후 중단
            {
                focusTimer.gameObject.SetActive(false);
                Animator.UpdateFocus();
                yield break;
            }
            elapsed += Time.deltaTime;
            focusTimer.UpdateTimer(useTime - elapsed, useTime);
            yield return null;
        }
        focusTimer.gameObject.SetActive(false);
        IsFocusing = false;
        Animator.UpdateFocus();
        onComplete?.Invoke();
    }



    private void OnDeath()
    {
        Debug.Log($"플레이어 죽음 / 현재 체력:{HP}");
        InventoryComp.ClearInventory();          // 보유 아이템 전체 손실
        Mgr_Game.Inst.ChangeScene(EScene.Hideout); // 은신처로 귀환
    }

    #endregion


    #region 잡힘 ---------------------------------------

    // 좀비에게 잡힘 — AIZombieController.UpdateAttack에서 호출
    // zombie : 잡은 좀비 (플레이어 회전 방향 계산에 사용)
    public void OnGrabbed(Character_Zombie zombie)
    {
        Mgr_UI.Inst.Reset();                         // 열려 있던 인벤토리 · 컨텍스트 메뉴 · 상호작용 표시 닫기
        Mgr_Game.Inst.SetGameState(EGameState.Stop); // 조작 · 카메라 잠금

        // 하던 행동 취소
        if (IsAttack) OnAttackEnd(); // 공격 — 콤보 초기화 + Upper Body 레이어 비우기
        IsFocusing = false;          // 아이템 사용 — FocusCoroutine이 감지해 취소 (아이템 소모 없음)

        Movement.OnGrabbed(zombie.transform.position); // 잡힘 상태 · 자세 초기화 · 좀비 방향 회전 — 애니메이션보다 먼저
        Animator.PlayGrabbed();
    }

    // 잡힘 종료 — Player_Push 애니메이션 마지막 프레임 이벤트로 호출
    public void OnGrabEnd()
    {
        Movement.OnGrabEnd();                        // 잡힘 상태 해제
        Mgr_Game.Inst.SetGameState(EGameState.Play); // 조작 · 카메라 잠금 해제
    }

    #endregion


    #region 웅크림 ---------------------------------------
    private Coroutine LerpCameraCoroutine; // LerpCameraHeight 코루틴 참조

    // PlayerMovement에서 웅크림 상태 전달 — 카메라 높이 코루틴 시작
    public void SetCrouching(bool isCrouching)
    {
        float target = isCrouching ? CameraCrouchHeight : CameraStandHeight;
        if (LerpCameraCoroutine != null) StopCoroutine(LerpCameraCoroutine);
        LerpCameraCoroutine = StartCoroutine(LerpCameraHeight(target));
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

    [field: Tooltip("현재 선택된 무기 슬롯 (1·2 = 무기, 3 = 맨손)")]
    [field: SerializeField] public int CurrentWeaponSlot { get; private set; } // 현재 선택된 무기 슬롯 (1·2=무기, 3=맨손)
    public EquipItem_Weapon CurrentWeapon { get; private set; } // 현재 손에 들고 있는 무기


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
            InventoryComp.Weapon_1.AttachToHolster(MeshComp.WeaponSocket_1);
        if (InventoryComp.Weapon_2 != null)
            InventoryComp.Weapon_2.AttachToHolster(MeshComp.WeaponSocket_2);

        switch (CurrentWeaponSlot)
        {
            case 1: CurrentWeapon = InventoryComp.Weapon_1; if (CurrentWeapon != null) Animator.PlayTakeWeapon(); break;
            case 2: CurrentWeapon = InventoryComp.Weapon_2; if (CurrentWeapon != null) Animator.PlayTakeWeapon(); break;
            case 3: CurrentWeapon = null;                                                                          break;
        }

        if (CurrentWeapon == null)
            Animator.SetWeaponType(0, 0); // 맨손
    }


    #endregion IInventory ---------------------------------------


    [Header("콤보")]
    [Tooltip("현재 콤보 단계 (0 = 미공격, 1 = 첫 번째, 2 = 두 번째 ...)")]
    public int  ComboCount = 0; // 현재 콤보 단계 (0 = 미공격, 1 = 첫 번째, 2 = 두 번째 ...)
    [Tooltip("공격 중 여부")]
    public bool IsAttack = false; // 콤보 입력 윈도우가 열려있는지
    [Tooltip("다음 콤보 예약 여부")]
    public bool NextCombo = false; // 다음 콤보가 예약됐는지

    // 공격 입력 — Mgr_PlayerController에서 호출
    public void Attack()
    {
        if (CurrentWeapon == null) return;

        if (IsAttack == false)
        {
            // 공격 중이 아님 → 첫 번째 공격 시작
            IsAttack = true;
            ComboCount = 1;
            Animator.PlayAttack(CurrentWeapon.WeaponData, ComboCount);
        }
        else
        {
            // 콤보 윈도우 안에서 입력 → 다음 콤보 예약
            NextCombo = true;
        }
        // CanCombo == false 인 동안의 입력은 무시
    }


    #region 애니메이션 이벤트 ---------------------------------------

    // 공격 애니메이션 시작 프레임 이벤트로 호출
    // → 콤보 입력 윈도우를 닫아 공격 시작 직후 연타 방지
    public void OnAttackStart()
    {

    }

    // 공격 애니메이션 중간 프레임 이벤트로 호출
    // → 콤보 입력 윈도우를 열고, 이미 예약된 콤보가 있으면 즉시 다음 공격 실행
    // → 예약된 콤보가 없으면 공격 종료
    public void OnAttackCombo()
    {
        // 다음 콤보 예약이 없으면 공격 종료
        if (NextCombo == false)
        {
            OnAttackEnd();
            return;
        }

        // Attack()에서 미리 예약한 콤보 → 즉시 다음 공격
        NextCombo = false;
        ComboCount++;
        if (!Animator.PlayAttack(CurrentWeapon.WeaponData, ComboCount))
            OnAttackEnd(); // 다음 콤보 애니메이션이 없으면 (마지막 콤보) 공격 종료
    }

    // 공격 종료 — OnAttackCombo(콤보가 이어지지 않음) · OnGrabbed(잡혀서 공격 취소)에서 호출
    // → 콤보 초기화 + Upper Body 레이어 비우기
    public void OnAttackEnd()
    {
        ComboCount = 0;
        IsAttack = false;
        NextCombo  = false;
        Animator.PlayEmpty(EAnimatorLayer.Upper);
    }

    // 애니메이션 이벤트 — "true": 히트박스 활성화 / "false": 히트박스 비활성화
    public void OnCollider(string value) => (CurrentWeapon as EquipItem_Weapon_Melee)?.OnCollider(value == "true");


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
            CurrentWeapon.AttachToHolster(socket);

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
            CurrentWeapon.AttachToHand(MeshComp.HandSocket_R);

            int weaponType = (int)CurrentWeapon.WeaponData.WeaponType + 1;
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
            CurrentWeapon.AttachToHolster(socket);
            Animator.SetWeaponType(0, 0);

            MeshComp.Target_Hand_L.SetParent(MeshComp.StartTarget_Hand_L, false);
            MeshComp.Target_Hand_L.localPosition = Vector3.zero;
            MeshComp.Target_Hand_L.localRotation = Quaternion.identity;
        }

    }


    #endregion 애니메이션 이벤트 ------------------------------------
}
