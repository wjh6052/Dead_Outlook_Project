# 앞으로 해야할 일

## 발견된 버그

- [ ] 이동 중 인벤토리 열기 시 이동 입력이 유지되어 계속 이동하는 버그


## 다음 작업
- [ ] 아이템 3D 모델링 제작
- [ ] 에디터 베이킹 툴 구현 (ItemIconBaker.cs — 모델 → 아이콘 PNG 자동 생성)
- [ ] 장비 장착 시 장비 오브젝트를 캐릭터에 부착 (조끼, 가방 등)




---

---

## 나중에

### 적 (좀비)
- [ ] 플레이어 행동별 MakeNoise 연결 (걷기 · 달리기 · 공격 · 착지)
- [ ] 좀비 애니메이션 클립 제작 (Death · Hit · Attack)
- [ ] 플레이어 데미지 — ZombieHitBox 콜라이더 방식 (Enable/Disable 애니메이션 이벤트)
- [ ] 공격 중 플레이어 방향 회전
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
- [ ] 저장 시스템
- [ ] 은신처 강화 시스템



## 고민중
- [ ] 플레이어 생존 스탯 (배고픔, 목마름, 피로, 체온, 스트레스, 질병)



- [ ] 부상 시스템 (출혈, 골절)

- [ ] IK리깅
- [ ] 부위별 체력 시스템 (머리·몸통·팔·다리 개별 HP — 다리 손상 시 뛰기 불가, 팔 손상 시 반동 증가·조준 흔들림, 머리/몸통 HP 0 시 즉사)
- [ ] 부상시 애니메이션
- [ ] 앉은 상태에서 뛰기 제거

- [ ] 낮/밤 사이클
- [ ] 날씨 시스템

---

## 완료

### 씬 분할
- [v] 씬 분할 (Player, UI, Mgr)
- [v] 씬 전환 — 출격 (Interactable_Deploy) · 귀환 (EscapeZone — 체류 타이머)
- [v] 사망 시 아이템 손실 + 은신처 귀환
- [v] UI_EscapeTimer 프리팹 제작 및 Mgr_UI 인스팩터 연결

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
