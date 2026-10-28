using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EFocusType
{
    Medical, // 치료 중
    Food,    // 먹는 중
}

// 과적 상태
public enum EOverweightState
{
    Normal,           // 보통 (과적 아님)
    Overweight,       // 과적
    SevereOverweight, // 초과적
}

public class Character_Player : MonoBehaviour, IDamageable, IInventory
{
    #region 스텟 변수 ---------------------------------------

    #region 체력
    [Header("체력")]
    [Tooltip("체력 (Max = 최대 체력, Current = 현재 체력)")]
    public FStat HP = new FStat { Max = 100f };
    #endregion


    #region 스태미나
    [Header("스태미나")]
    [Tooltip("스태미나 (Max = 최대 스태미나, Current = 현재 스태미나)")]
    public FStat Stamina = new FStat { Max = 100f };
    [Tooltip("뛸 때 초당 스태미나 소모량")]
    public float StaminaRunCost = 15f;
    [Tooltip("점프 1회당 스태미나 소모량 — 남은 스태미나가 이보다 적으면 점프 불가")]
    public float StaminaJumpCost = 10f;
    [Tooltip("공격 1회당 스태미나 소모량 — 남은 스태미나가 이보다 적으면 공격 불가")]
    public float StaminaAttackCost = 10f;
    [Tooltip("초당 스태미나 회복량")]
    public float StaminaRegen = 20f;

    [Tooltip("스태미나 회복 대기 타이머 (Max = 소모를 멈춘 뒤 회복이 시작되기까지 시간)")]
    public FTimer StaminaRegen_Timer = new FTimer { Max = 1f };
    [Tooltip("탈진 후 다시 뛸 수 있는 스태미나 비율 (0~1)")]
    public float StaminaExhaustRecover = 0.3f;
    [field: Tooltip("탈진 여부 — 스태미나가 0이 되면 StaminaExhaustRecover 비율까지 회복될 때까지 뛸 수 없음")]
    [field: SerializeField] public bool IsExhausted { get; private set; }
    #endregion


    #region 배고픔
    [Header("배고픔")]
    [Tooltip("배고픔 (Max = 최대 포만감, Current = 현재 포만감) — 0에 가까울수록 배고픔")]
    public FStat Hunger = new FStat { Max = 100f };
    [Tooltip("초당 배고픔 감소량")]
    public float HungerDecay = 0.05f;
    [Tooltip("배고픔이 0일 때 초당 체력 감소량 — 체력이 0이 되면 사망")]
    public float HungerDamage = 5f;
    #endregion


    #region 목마름
    [Header("목마름")]
    [Tooltip("목마름 (Max = 최대 수분, Current = 현재 수분) — 0에 가까울수록 목마름")]
    public FStat Thirst = new FStat { Max = 100f };
    [Tooltip("초당 목마름 감소량")]
    public float ThirstDecay = 0.08f;
    #endregion


    #region 디버프
    [Header("디버프")]
    [Tooltip("디버프 하나당 스태미나 회복량 배율 (배고픔 · 목마름 · 과적) — 겹치면 곱함")]
    public float DebuffStaminaRegen = 0.5f;
    [Tooltip("디버프 하나당 이동 속도 배율 (배고픔 · 초과적) — 겹치면 곱함")]
    public float DebuffMoveSpeed = 0.7f;
    #endregion


    #region 무게
    [Header("무게")]
    [Tooltip("과적 기준 무게 (kg) — 소지 무게가 이 값을 넘으면 과적")]
    public float OverweightLimit = 30f;
    [field: Tooltip("현재 과적 상태 (보통 / 과적 / 초과적)")]
    [field: SerializeField] public EOverweightState OverweightState { get; private set; }
    [Tooltip("현재 소지 무게 (kg)")]
    [SerializeField] private float CarryWeightValue; // CarryWeight의 실제 값 — 인스펙터 표시용

    // 현재 소지 무게 (kg) — InventoryComponent.OnItemChanged에서 설정, 설정 시 과적 상태 갱신
    public float CarryWeight
    {
        get => CarryWeightValue;
        set
        {
            CarryWeightValue = value;

            if      (value >= OverweightLimit * 1.5f) OverweightState = EOverweightState.SevereOverweight; // 기준의 1.5배 이상 — 초과적
            else if (value >  OverweightLimit)        OverweightState = EOverweightState.Overweight;       // 기준 초과 — 과적
            else                                      OverweightState = EOverweightState.Normal;           // 기준 이하 — 보통
        }
    }
    #endregion


    #region 아이템 사용 집중
    [Tooltip("아이템 사용 집중 중 여부")]
    public bool IsFocusing; // 아이템 사용 집중 중
    public EFocusType FocusType { get; private set; } // 아이템 사용 집중 중 — 집중 중인 행동 종류 (치료 등)
    public int FocusSubType { get; private set; } // 집중 세부 종류 (음식: EEatType, 치료: 0)
    #endregion

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


    #region 잡힘 변수 ---------------------------------------
    private List<Character_Zombie> GrabbingZombies = new List<Character_Zombie>(); // 잡고 있는 좀비 (처음 잡은 좀비 + 합류한 좀비) — 밀칠 때 모두 밀려남
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
        InventoryComp.Init(this); // 인벤토리에 소유 플레이어 전달 — 무게 계산 결과를 CarryWeight에 적용하기 위해
        MeshComp      = GetComponent<MeshComponent>();
        CameraTarget  = transform.Find("CameraTarget");


        HP.Fill();
        Stamina.Fill();
        Hunger.Fill();
        Thirst.Fill();
        Stamina.OnDecreased += () => StaminaRegen_Timer.Reset(); // 스태미나가 줄 때마다 회복 대기 타이머를 처음부터
        CameraStandHeight = CameraTarget.localPosition.y;
    }

    private void Update()
    {
        // 스태미나 회복 — 착지 상태에서 소모를 멈추고 대기 시간이 지나면 회복
        if (Movement.bIsGrounded)
        {
            StaminaRegen_Timer.Tick();
            if (StaminaRegen_Timer.IsReached)
                Stamina.Add(StaminaRegen * GetStaminaRegenMultiplier() * Time.deltaTime); // 디버프 배율 적용
        }

        // 탈진 — 0이 되면 탈진, 일정 비율까지 회복되면 해제
        if      (Stamina.IsEmpty)                        IsExhausted = true;
        else if (Stamina.Ratio >= StaminaExhaustRecover) IsExhausted = false;

        // 배고픔 · 목마름 — 시간이 지나면 감소
        Hunger.Add(-HungerDecay * Time.deltaTime);
        Thirst.Add(-ThirstDecay * Time.deltaTime);

        // 굶주림 — 배고픔이 0이면 체력 감소, 0이 되면 사망
        // TakeDamage를 쓰지 않음 — 매 프레임 데미지 로그 · 스태미나 회복 대기 타이머 리셋 방지
        if (Hunger.IsEmpty && !HP.IsEmpty)
        {
            HP.Add(-HungerDamage * Time.deltaTime);
            if (HP.IsEmpty) OnDeath();
        }
    }


    // 스태미나 회복량 배율 — 디버프가 겹치면 곱함
    public float GetStaminaRegenMultiplier()
    {
        float multiplier = 1f;
        if (Hunger.IsEmpty)                             multiplier *= DebuffStaminaRegen; // 배고픔
        if (Thirst.IsEmpty)                             multiplier *= DebuffStaminaRegen; // 목마름
        if (OverweightState != EOverweightState.Normal) multiplier *= DebuffStaminaRegen; // 과적 · 초과적 (한 번만 적용)
        return multiplier;
    }

    // 이동 속도 배율 — 디버프가 겹치면 곱함
    public float GetMoveSpeedMultiplier()
    {
        float multiplier = 1f;
        if (Hunger.IsEmpty)                                       multiplier *= DebuffMoveSpeed; // 배고픔
        if (OverweightState == EOverweightState.SevereOverweight) multiplier *= DebuffMoveSpeed; // 초과적
        return multiplier;
    }


    #region IDamageable ---------------------------------------

    public void TakeDamage(float damage, GameObject attacker, GameObject attackerOwner)
    {
        if (HP.IsEmpty) return; // 이미 사망 — 쓰러져 있는 동안 데미지 · 사망 처리 반복 방지

        HP.Add(-damage);
        StaminaRegen_Timer.Reset(); // 공격을 받으면 스태미나 회복 대기 타이머를 처음부터
        Debug.Log($"[{name}] {attacker.name}에게 {damage} 데미지 — 남은 HP: {HP.Current}/{HP.Max}");
        if (HP.IsEmpty) OnDeath();
    }


    public void Heal(float amount)
    {
        HP.Add(amount);
    }


    private void OnDeath()
    {
        Debug.Log($"플레이어 죽음 / 현재 체력:{HP}");

        // 하던 행동 취소
        if (IsAttack) OnAttackEnd(); // 공격 — 콤보 초기화 + Upper Body 레이어 비우기
        IsFocusing = false;          // 아이템 사용 — C_Focus이 감지해 취소

        Animator.PlayDeath();        // 사망 애니메이션
        Mgr_Game.Inst.PlayerDeath(); // 게임에 사망 신호
    }

    // 부활 — 사망 상태를 되돌림 (Mgr_Game.C_PlayerDeath에서 은신처 귀환 후 호출)
    // 플레이어 프리팹화 후 사망 시 재생성 방식으로 바꾸면 삭제 예정
    public void Revive()
    {
        HP.Fill(); // 체력 최대로
        Hunger.Fill(); // 배고픔 최대로
        Thirst.Fill(); // 목마름 최대로

        // 애니메이션 전체 초기화 — 모든 레이어 상태 · 파라미터를 기본값으로 (사망 자세에서 기본 상태로)
        UnityEngine.Animator anim = GetComponentInChildren<UnityEngine.Animator>(); // Animator는 자식 모델에 있음
        anim.Rebind();
        anim.Update(0f); // 기본 자세를 즉시 반영

        Movement.OnGrabEnd(); // 잡힌 채 죽었을 경우 잡힘 상태 해제
        GrabbingZombies.Clear(); // 잡힌 채 죽었을 경우 잡고 있는 좀비 목록 비우기 (필드 씬이 내려가 좀비가 사라지므로)
    }

    #endregion


    #region 잡힘 ---------------------------------------

    // 좀비에게 잡힘 — AIZombieController.UpdateAttack에서 호출
    // zombie : 잡은 좀비 (플레이어 회전 방향 계산에 사용)
    public void OnGrabbed(Character_Zombie zombie)
    {
        Mgr_UI.Inst.Reset();                         // 열려 있던 인벤토리 · 컨텍스트 메뉴 · 상호작용 표시 닫기
        Mgr_PlayerController.Inst.SetPlayState(EPlayState.Grabbed); // 조작 · 카메라 잠금

        // 하던 행동 취소
        if (IsAttack) OnAttackEnd(); // 공격 — 콤보 초기화 + Upper Body 레이어 비우기
        IsFocusing = false;          // 아이템 사용 — C_Focus이 감지해 취소 (아이템 소모 없음)

        Movement.OnGrabbed(zombie.transform.position); // 잡힘 상태 · 자세 초기화 · 좀비 방향 회전 — 애니메이션보다 먼저
        Animator.PlayGrabbed();

        GrabbingZombies.Add(zombie); // 밀칠 때 밀려나도록 등록
    }

    // 잡힘 종료 — Player_Push 애니메이션 마지막 프레임 이벤트로 호출
    public void OnGrabEnd()
    {
        OnPush();                                    // 밀치는 프레임 이후 늦게 합류한 좀비도 밀어냄
        Movement.OnGrabEnd();                        // 잡힘 상태 해제
        if (Mgr_PlayerController.Inst.PlayState == EPlayState.Grabbed)  // 잡힌 채 죽었으면 Dead 유지
            Mgr_PlayerController.Inst.SetPlayState(EPlayState.Play);   // 조작 · 카메라 잠금 해제
    }

    // 잡혀 있는 동안 다른 좀비가 합류 — AIZombieController.UpdateAttack에서 호출
    public void JoinGrab(Character_Zombie zombie)
    {
        GrabbingZombies.Add(zombie);
    }

    // 밀치기 — Player_Push 애니메이션의 밀치는 프레임 이벤트로 호출
    // 잡고 있는 좀비를 모두 밀려나게 함 (처음 잡은 좀비 + 합류한 좀비)
    public void OnPush()
    {
        foreach (Character_Zombie zombie in GrabbingZombies)
            zombie.ZombieAnim.PlayPushed();

        GrabbingZombies.Clear();
    }

    #endregion


    #region 웅크림 ---------------------------------------
    private Coroutine LerpCameraCoroutine; // C_LerpCameraHeight 코루틴 참조

    // PlayerMovement에서 웅크림 상태 전달 — 카메라 높이 코루틴 시작
    public void SetCrouching(bool isCrouching)
    {
        float target = isCrouching ? CameraCrouchHeight : CameraStandHeight;
        if (LerpCameraCoroutine != null) StopCoroutine(LerpCameraCoroutine);
        LerpCameraCoroutine = StartCoroutine(C_LerpCameraHeight(target));
    }

    private IEnumerator C_LerpCameraHeight(float targetHeight)
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


    #region 아이템 사용 ---------------------------------------

    // 아이템 사용 — 타입별로 처리, 사용 성공 시 true 반환
    public bool UseItem(Item item)
    {
        switch (item.ItemType)
        {
            case EItemType.Food:
                StartCoroutine(C_Eat(item as Item_Food)); // 먹기 시작 (배고픔 · 목마름이 가득이어도 먹을 수 있음)
                return true;
            case EItemType.Medical:
                if (HP.Current >= HP.Max) return false;
                StartCoroutine(C_Heal(item as Item_Medical));
                return true;
        }

        return false;
    }

    // UseTime초 동안 집중 후 아이템 소모 + HealAmount만큼 회복
    private IEnumerator C_Heal(Item_Medical medical)
    {
        yield return StartCoroutine(C_Focus(EFocusType.Medical, (int)medical.MedicalType, medical.UseTime, () =>
        {
            medical.DecreaseAmount(1);
            Heal(medical.HealAmount);
        }));
    }

    // UseTime초 동안 집중 후 아이템 소모 + 배고픔 · 목마름 회복
    private IEnumerator C_Eat(Item_Food food)
    {
        yield return StartCoroutine(C_Focus(EFocusType.Food, (int)food.EatType, food.UseTime, () =>
        {
            food.DecreaseAmount(1);
            Hunger.Add(food.HungerAmount);
            Thirst.Add(food.ThirstAmount);
        }));
    }

    // useTime초 동안 집중 타이머 UI를 표시하고, 완료 시 onComplete 호출
    // 도중에 IsFocusing이 false가 되면 취소
    private IEnumerator C_Focus(EFocusType focusType, int subType, float useTime, System.Action onComplete = null)
    {
        if (IsFocusing) yield break; // 이미 집중 중이면 취소

        IsFocusing = true;
        FocusType = focusType;
        FocusSubType = subType;
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

    #endregion



    #region 공격 ---------------------------------------

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
            if (Stamina.Current < StaminaAttackCost) return; // 스태미나 부족 — 공격 불가

            IsAttack = true;
            ComboCount = 1;
            Animator.PlayAttack(CurrentWeapon.WeaponData, ComboCount);
            Stamina.Add(-StaminaAttackCost); // 공격 스태미나 소모
        }
        else
        {
            // 콤보 윈도우 안에서 입력 → 다음 콤보 예약
            NextCombo = true;
        }
        // CanCombo == false 인 동안의 입력은 무시
    }

    #endregion



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

        if (Stamina.Current < StaminaAttackCost) { OnAttackEnd(); return; } // 스태미나 부족 — 콤보 종료

        ComboCount++;
        if (!Animator.PlayAttack(CurrentWeapon.WeaponData, ComboCount))
            OnAttackEnd(); // 다음 콤보 애니메이션이 없으면 (마지막 콤보) 공격 종료
        else
            Stamina.Add(-StaminaAttackCost); // 다음 콤보가 실제로 나갔을 때만 소모
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
