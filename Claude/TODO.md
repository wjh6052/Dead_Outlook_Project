# 앞으로 해야할 일

## 발견된 버그


## 다음 작업

### 좀비 잡기 공격 — 남은 작업
- [ ] 여러 좀비 합류 — 이미 물고 있으면 다른 좀비도 합류해서 물고, 밀치는 타이밍에 같이 밀려남
- [ ] 잡고 있는 좀비가 데미지를 받으면 바로 풀림
- 아직 정할 것
  - 주 좀비(처음 잡은 좀비)가 데미지로 풀리면 잡기 전체 종료할지, 합류 좀비는 계속 물지
- 정해진 규칙
  - 잡기 조건: 공격 사거리 안 + 피격 중이 아님 (정면 조건은 제거 — 공격 시작 시 서로 마주 보도록 즉시 회전)
  - 잡혀 있는 동안 조작 · 카메라 잠금 (EGameState.Stop 재사용)

### 적 (좀비)
- [ ] 좀비 애니메이션 클립 정리 (Death · Hit) — 공격은 잡기 클립으로 대체됨


---

---

## 나중에

### 디테일 작업
- [ ] 잡기 애니메이션 중 좀비와 콜라이더가 겹치며 플레이어가 밀리는 문제 (좀비는 정상)
  - 원인: 플레이어 KCC가 좀비 콜라이더와 겹친 것을 감지하고 밀어냄
  - 방향: Character_Player에 잡은 좀비(GrabbedZombie) 저장 → PlayerMovement/IsColliderValidForCollisions에서 그 좀비만 false 반환 → Player_Push 끝 이벤트 OnGrabEnd에서 해제

### 적 (좀비)
- [ ] 플레이어 행동별 MakeNoise 연결 (걷기 · 달리기 · 공격 · 착지)
- [ ] 사망 후처리 — Destroy · 아이템 드롭

### 전투
- [ ] 데미지 임팩트 (피격 이펙트, 사운드, 히트스톱 등)
- [ ] 무기 베이스 클래스 설계 (총기) [모딩시스템] [탄약 관리]
- [ ] 총기 발사 / 재장전 세부 구현 + 애니메이션

### 플레이어
- [ ] 플레이어 스테미너 시스템
- [ ] 사망 연출 (눈 감기 → 페이드아웃)
- [ ] 감염 시스템 (회복 아이템에 감염 치료 / 감염 늦추기 변수 추가)

### 아이템 / 인벤토리
- [ ] InventoryData에 총 무게 계산 함수 추가
- [ ] 회복 아이템 클래스 설계 (Item_Food)
- [ ] 음식 아이템 추가 (Item_Food 구현 및 생존 스탯 연동, 사용 버튼)

### 월드
- [ ] 지도 (씬 전환)
- [ ] 맵 이동 시 로딩 동기화 — Mgr_Game/ChangeScene에서 씬 로드 완료 시점 받기 (SceneManager.sceneLoaded 이벤트 또는 LoadSceneAsync의 completed · progress)
- [ ] 저장 시스템
- [ ] 은신처 강화 시스템


### 모델링
- [ ] 아이템 3D 모델링 제작 (아이템 모델은 Read/Write 켜기 — 바닥에서 Outline이 메쉬를 읽음)

### 장착 아이템
- [ ] 조끼 · 벨트 장착 시 오브젝트 부착 — EquipItem_Container 상속 클래스, VestSocket · BeltSocket, InventoryComponent 장착 함수
  - 벨트는 EContainerType에 Belt 추가 + 인벤토리 UI 벨트 칸 필요 (지금 테스트 "밸트"는 Vest로 들어가 있음)
- [ ] 가방 · 조끼 · 벨트 장착 함수 일반화 검토 — SetBag 등을 EContainerType으로 구분하는 SetContainer 하나로


## 고민중
- [ ] 플레이어 생존 스탯 (배고픔, 목마름, 피로, 체온, 스트레스, 질병)

- [ ] 날카로운 날붙이가 있는 무기로 공격 및 타격시 특수 애니메이션이 나오도록

- [ ] 부상 시스템 (출혈, 골절)

- [ ] IK리깅
- [ ] 부위별 체력 시스템 (머리·몸통·팔·다리 개별 HP — 다리 손상 시 뛰기 불가, 팔 손상 시 반동 증가·조준 흔들림, 머리/몸통 HP 0 시 즉사)
- [ ] 부상시 애니메이션
- [ ] 앉은 상태에서 뛰기 제거

- [ ] 낮/밤 사이클
- [ ] 날씨 시스템

---

## 완료

### 해결된 버그
- [v] 이동 중 인벤토리 열기 시 이동 입력이 유지되어 계속 이동하는 버그 (Mgr_PlayerController/Update — Play가 아닐 때 MoveInput · bAim · bRunInput 초기화)
- [v] 좀비가 플레이어를 잊으면 예전 소음 위치 · 원점(0,0,0)으로 이동하는 버그 (마지막으로 본 위치로 경계)
- [v] 맞은 좀비가 피격 직후 바로 반격하는 버그 (Hit_Timer 경직)
- [v] 죽은 좀비를 때리면 사망 애니메이션이 다시 나오는 버그 (Character_Zombie/TakeDamage 사망 검사)
- [v] 인벤토리를 연 채 잡히면 커서가 보이는 버그 (Stop 상태에서 커서 잠금)
- [v] 잡힌 상태에서 F키 상호작용이 되는 버그 (Mgr_PlayerController/OnInteractable 상태 검사)
- [v] 웅크린 채 잡히면 일어서는 애니메이션이 잡힘 애니메이션을 끊는 버그 (잡힌 동안 자세 트리거 차단)
- [v] 방망이 장착 시 프리팹 없음 오류 (PF_Bat · Icon_Bat · Bat 모델 복원)
- [v] 죽은 좀비 콜라이더가 남아 플레이어가 지나가지 못하는 버그 (Character_Zombie/OnDeath — 콜라이더 비활성화)
- [v] 등에 멘 무기가 너무 위로 올라가는 문제 (손 기준 원점 대신 HolsterPoint 기준으로 등 소켓에 부착)
- [v] 아이템 장착 시 Outline이 메쉬를 못 읽는 오류 (아이템 모델 FBX Read/Write 켜기)
- [v] 아이콘 · 프리팹 경로가 틀리면 아이콘이 비거나 장착 시 오류 (Mgr_Item — 기본 아이콘 Icon_Temp · 테스트 프리팹으로 대체 + 경고 로그)

### 장착 아이템 (EquipItem)
- [v] EquipItem 공통 부모 — InitEquip(Item, owner) (줍기 · 물리 · 콜라이더 · 레이어), AttachToSocket · AttachToHolster, HolsterPoint, FindChild
- [v] 무기 이름 변경 — Weapon → EquipItem_Weapon, Weapon_Melee → EquipItem_Weapon_Melee (WeaponData는 ItemData 캐스팅 속성)
- [v] EquipItem_Container (ContainerData 캐스팅 속성) → EquipItem_Container_Bag
- [v] GripPoint_L · HolsterPoint를 자식 이름으로 자동 연결 (Awake — Instantiate 직후 필요)
- [v] 가방 장착 시 등에 오브젝트 표시 (MeshComponent.BagSocket, InventoryComponent/SetBag — 사망 시 함께 제거)

### 아이템 데이터 · 에디터 도구
- [v] 아이콘 · 프리팹을 아이템 타입별 폴더에서 찾기 (UI/Icon/{타입}/, Prefab/Item/{타입}/)
- [v] Tools 메뉴 스크립트 Tools_ 접두사 규칙 (Tools_ItemDataFetcher · Tools_ItemIconBaker, Editor 폴더)
- [v] 아이콘 베이킹 툴 복원 (Tools_ItemIconBaker — 모델 → 아이콘 PNG) + Z축(롤) 회전 추가
- [v] 인스펙터 표시 변수 Tooltip · 미표시 변수 주석 규칙 — 플레이어 스크립트 적용 (Character_Player · PlayerMovement · PlayerAnimator · InventoryComponent · MeshComponent)

### 좀비 잡기 공격 (잡기 → 물기 → 밀치기)
- [v] 좀비 Zombie_Grab · Zombie_Bite · Zombie_Pushed / 플레이어 Player_Grabbed · Player_Bitten · Player_Push 애니메이션 추가
- [v] 좀비가 공격 사거리에 들어가면 잡기 재생 — 공격 조건 검사 (Target · 플레이어 생존 · 사거리)
- [v] 플레이어 잡힘 반응 (Character_Player/OnGrabbed — UI 닫기 · 조작/카메라 잠금 · 공격/아이템 사용 취소)
- [v] 좀비 · 플레이어 서로 마주 보도록 즉시 회전 (Character_Zombie/LookAt · PlayerMovement/LookAt)
- [v] 웅크린 상태면 선 자세로 초기화 (PlayerMovement/OnGrabbed · IsGrabbed)
- [v] 물기 데미지 (Character_Zombie/OnBite 애니메이션 이벤트 → AttackDamage)
- [v] 잡기 종료 시 잠금 해제 (Player_Push 이벤트 → Character_Player/OnGrabEnd)
- [v] RootMotion 태그 상태에서만 루트 모션 이동 (PlayerMovement/IsRootMotionState)

### 좀비 AI 개선
- [v] 피격 경직 — 애니메이션 이벤트 대신 Hit_Timer로 해제
- [v] 플레이어를 잊으면 마지막으로 본 위치로 경계 이동 (AlertPoint · LastSeenPosition)
- [v] 좀비 뛰기 제거 — 추격도 걷기 속도 (WalkSpeed를 Character_Zombie 스텟으로 이동)
- [v] AIZombieController.Target 읽기 전용 공개

### 캐릭터 구조 분리 (빈 부모 + 모델 자식)
- [v] 로직 컴포넌트는 빈 부모에, Animator는 자식 모델에 — 모델만 교체하면서 작업 가능
- [v] PlayerAnimationEvents — 애니메이션 이벤트 · OnAnimatorMove를 부모의 Character_Player · PlayerMovement로 전달
- [v] ZombieAnimationEvents — 애니메이션 이벤트를 Character_Zombie로 전달, 루트 모션은 Character_Zombie/ApplyRootMotion으로 부모 이동
- [v] PlayerAnimator · PlayerMovement · ZombieAnimator · MeshComponent — Animator를 GetComponentInChildren으로 찾도록 변경
- [v] Player_Scene 플레이어 · Field_Scene 좀비 구조 변경

### 씬 분할
- [v] 씬 분할 (Player, UI, Mgr)
- [v] 씬 전환 — 출격 (Interactable_Deploy) · 귀환 (EscapeZone — 체류 타이머)
- [v] 사망 시 아이템 손실 + 은신처 귀환
- [v] UI_EscapeTimer 프리팹 제작 및 Mgr_UI 인스팩터 연결
- [v] EScene 이름 + "_Scene" 규칙으로 씬 이름 문자열 제거 (Mgr_Game/GetSceneName)
- [v] Interactable_Deploy · EscapeZone — 인스펙터에서 이동할 씬 선택 (TargetScene)

### 소음 시스템
- [v] Mgr_Noise — MakeNoise (OverlapSphere + NavMesh 경로 거리 체크 + 태그 기반 좀비 감지)
- [v] NoiseWave — LineRenderer 파동 시각화 (반지름까지 확장 후 페이드)
- [v] 좀비 소음 감지 — OnNoiseDetected → Alert 전환 · 이동

### 좀비 행동 패턴
- [v] AIZombieController — 우선순위 상태 머신 (Death > Hit > Attack > Chase > Alert > Idle)
- [v] 시각 감지 — 거리 · 각도 · 레이캐스트, Contact/Forget 타이머
- [v] 피격/사망 파이프라인 — Character_Zombie → ZombieAnimator · AIZombieController 연결
- [v] 좀비 애니메이션 연동 (ZombieAnimator — 이동 · 공격 · 피격 · 사망)

### 전투
- [v] 플레이어 체력 시스템 + 회복기능 + 플레이어 상태창 (체력)
- [v] 회복 아이템 클래스 설계 (Item_Medical — HealAmount, UseTime, 구글시트 연동)
- [v] 회복 아이템 사용 (컨텍스트 메뉴 사용 버튼 → UseItem → 수량 소모)
- [v] 집중 사용 시스템 (FocusCoroutine — IsFocusing/EFocusType, 원형 타이머 UI, 애니메이터 연동)
- [v] 무기 베이스 클래스 설계 (근접무기)
- [v] 애니메이션 상하체 분리 (Avatar Mask — 하체는 이동, 상체는 조준 독립 재생)
- [v] 히트박스 / 데미지 시스템 + 허수아비 오브젝트
- [v] 공격 콤보 시스템 (ComboCount, OnAttackCombo, OnAttackEnd)
- [v] 공격 중 루트 모션 (OnAnimatorMove → KCC 속도 전달)
- [v] 공격 애니메이션 Full Body → Upper Body 레이어 이동 — 공격 중 이동 가능 (공격 루트 모션은 사용 안 함)
- [v] OnAttackEnd 이벤트 제거 — OnAttackCombo에서 다음 콤보가 없으면 종료 (PlayAttack bool 반환)

### 플레이어
- [v] 마네킹 변경
- [v] 플레이어 움직임 간소화

### 인벤토리 기능
- [v] 아이템 버리기 기능 (인벤토리에서 바닥으로 버리기)
- [v] 아이템 분할 기능 (UI_W_ItemSplit — 슬라이더/입력창 연동)
- [v] 가방 열기 기능 (UI_W_BagInventory — 가방 이동 시 창 자동 닫힘)
- [v] 아이템 스택 겹치기 (습득 시 기존 스택에 먼저 채우기)
- [v] 인벤토리 데이터 변경 콜백 (InventoryData.OnChanged → UI_ItemGrid 자동 갱신)

### UI 시스템
- [v] 창 시스템 (UI 상단 드래그 가능한 윈도우 창, 여러 정보 패널 용도)
- [v] 컨텍스트 메뉴 구현
  - [v] 열기
  - [v] 분할
  - [v] 버리기
  - [v] 상세정보

### 아이템 / 상호작용
- [v] 아이템 프리팹 제작 (인게임 배치용 아이템 오브젝트)
- [v] 바닥 아이템 F키로 습득
- [v] 가방/벨트/케이스 구분 (EContainerType — UI_EquipSlot 서브타입 구분)
- [v] 소지 인벤토리 (조끼 → 주머니 → 가방 순, InventoryComponent + UI_Inventory 연동)
- [v] 상자/박스 인벤토리 (Interactable_Box 상호작용 시 UI_Inventory에 표시)
- [v] 랜덤 아이템 생성 (ItemSpawnEntry — 타입/등급 가중치 + 미등장 확률 + SpawnCount)
