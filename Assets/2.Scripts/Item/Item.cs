using UnityEngine;

public enum EItemType // 아이템 타입 열거형
{
    General,    // 기타
    Weapon,     // 무기
    Armor,      // 방어구
    Tool,       // 도구
    Food,       // 음식
    Medical,    // 의료
    Material,   // 재료
    Bag         // 가방
}

public enum ERarity // 희귀도 열거형
{
    Normal,     // 노멀
    Rare,       // 레어
    Epic,       // 에픽
    Unique,     // 유니크
    Legendary   // 레전더리
}

// 모든 아이템의 공통 데이터 — Weapon, Armor 등이 상속받는 베이스 클래스
[System.Serializable]
public class Item
{
    [field: SerializeField] public EItemType  ItemType    { get; private set; } // 아이템 타입
    [field: SerializeField] public ERarity    Rarity      { get; private set; } // 희귀도
    [field: SerializeField] public string     ID          { get; private set; } // 아이템 고유 코드
    [field: SerializeField] public string     Name        { get; private set; } // 아이템 이름
    [field: SerializeField] public string     Description { get; private set; } // 아이템 설명
    [field: SerializeField] public float      Weight      { get; private set; } // 무게 (kg)
    [field: SerializeField] public int        MaxStack    { get; private set; } // 최대 스택 수량
    [field: SerializeField] public int        TileSizeX   { get; private set; } // 인벤토리 격자 가로 크기
    [field: SerializeField] public int        TileSizeY   { get; private set; } // 인벤토리 격자 세로 크기
    [field: SerializeField] public int        Amount      { get; private set; } // 현재 보유 수량
    [field: SerializeField] public int        SlotX       { get; private set; } // 인벤토리 그리드 X 위치
    [field: SerializeField] public int        SlotY       { get; private set; } // 인벤토리 그리드 Y 위치
    [field: SerializeField] public bool       IsRotated   { get; private set; } // 90도 회전 여부
    [field: SerializeField] public Sprite     Icon        { get; private set; } // UI 아이콘 스프라이트
    [field: SerializeField] public GameObject Prefab      { get; private set; } // 필드 드롭 프리팹

    // 생성자
    public Item(EItemType itemType, ERarity rarity, string id, string name, string description,
                float weight, int maxStack, int tileSizeX, int tileSizeY, Sprite icon, GameObject prefab)
    {
        ItemType    = itemType;
        Rarity      = rarity;
        ID          = id;
        Name        = name;
        Description = description;
        Weight      = weight;
        MaxStack    = maxStack;
        TileSizeX   = tileSizeX;
        TileSizeY   = tileSizeY;
        Amount      = 1;
        Icon        = icon;
        Prefab      = prefab;
    }

    // 동일한 새 인스턴스 반환
    public virtual Item Copy() => (Item)MemberwiseClone();

    // 인벤토리 그리드 위치 설정
    public void SetSlot(int x, int y)
    {
        SlotX = x;
        SlotY = y;
    }

    // 회전 상태 토글
    public void ToggleRotation()
    {
        IsRotated = !IsRotated;
    }

    // 수량 직접 설정
    public void SetAmount(int amount)
    {
        Amount = Mathf.Clamp(amount, 0, MaxStack);
    }

    // 수량 추가 — MaxStack 초과 불가
    public void AddAmount(int amount)
    {
        Amount = Mathf.Min(Amount + amount, MaxStack);
    }

    // 수량 감소 — 0 미만 불가
    public void DecreaseAmount(int amount)
    {
        Amount = Mathf.Max(Amount - amount, 0);
    }

    // 완전히 합칠 수 있는지 확인 (green)
    public bool CanStackWith(Item other)
    {
        return other != null && ID == other.ID && Amount + other.Amount <= MaxStack;
    }

    // 부분적으로 합칠 수 있는지 확인 (yellow)
    public bool CanPartialStack(Item other)
    {
        return other != null && ID == other.ID && other.Amount < other.MaxStack && Amount + other.Amount > other.MaxStack;
    }
}
