using System;
using System.Collections.Generic;
using UnityEngine;

public class Mgr_Item : MonoBehaviour
{
    private Dictionary<string, Item>       Items          = new Dictionary<string, Item>();
    [SerializeField] private List<Item>     DebugItems     = new List<Item>();     // 디버그용 인스펙터 확인
    [SerializeField] private List<Item_Container> DebugBagItems  = new List<Item_Container>(); // 가방 아이템 디버그용

    // ERarity 순서대로 — Normal, Rare, Epic, Unique, Legendary
    [SerializeField] private Color[] RarityColors = new Color[]
    {
        new Color(0.59f, 0.59f, 0.59f, 0.39f), // Normal    — 회색
        new Color(0.39f, 0.71f, 1.00f, 0.39f), // Rare      — 하늘색
        new Color(0.71f, 0.39f, 1.00f, 0.39f), // Epic      — 보라색
        new Color(1.00f, 0.78f, 0.00f, 0.39f), // Unique    — 노란색
        new Color(0.39f, 1.00f, 0.59f, 0.39f), // Legendary — 초록색
    };


    public static Mgr_Item Inst { get; private set; }

    // 아이템 데이터를 불러온 뒤, 아이템이 들어왔을 때만 싱글톤 등록 (Mgr_Game.C_GameStart가 Inst로 준비 완료 확인)
    private void Awake()
    {
        LoadFromJson();

        if (Items.Count > 0) Inst = this;
        else Debug.LogWarning("[Mgr_Item] 불러온 아이템이 없어 Inst를 등록하지 않음 — 게임 시작이 대기 상태로 멈춤");
    }


    // 월드에 아이템 스폰
    public void SpawnItem(Item item, Vector3 pos)
    {
        if (item?.Prefab == null)
        {
            Debug.LogWarning($"[Mgr_Item] SpawnItem 실패 — {item?.Name} 프리팹 없음");
            return;
        }

        GameObject go = Instantiate(item.Prefab, pos, Quaternion.identity);
        Interactable_Item worldItem = go.GetComponent<Interactable_Item>() ?? go.AddComponent<Interactable_Item>();
        worldItem.Init(item, pos);
    }


    // ERarity에 해당하는 색상 반환
    public Color GetRarityColor(ERarity rarity) => RarityColors[(int)rarity];

    public IEnumerable<Item> GetAllItems() => Items.Values;

    // ID로 아이템 반환
    public Item GetItem(string id)
    {
        Items.TryGetValue(id, out Item item);
        return item?.Copy();
    }

    // ItemSpawnData 기반으로 희귀도 결정 후 랜덤 아이템 반환 — null이면 미등장
    public Item GetRandomItem(ItemSpawnData data)
    {
        ERarity? rarity = data.PickRarity();
        if (rarity == null) return null;

        List<Item> filtered = new List<Item>();
        foreach (Item item in Items.Values)
            if (item.ItemType == data.ItemType && item.Rarity == rarity.Value) filtered.Add(item);

        if (filtered.Count == 0) return null;
        return filtered[UnityEngine.Random.Range(0, filtered.Count)].Copy();
    }

    



    #region 플레이시 아이템 정보 변환
    // JSON에서 아이템 데이터 로드 — 시트별 파일 ItemData_{종류}.json
    private void LoadFromJson()
    {
        // 전용 칸 없는 아이템 (General · Armor · Tool)
        foreach (EItemType itemType in new[] { EItemType.General, EItemType.Armor, EItemType.Tool })
        {
            foreach (ItemDataDTO dto in LoadList<ItemDataDTO>(itemType))
            {
                LoadResources(dto, out ERarity rarity, out Sprite icon, out GameObject prefab);
                Add(new Item(itemType, rarity, dto.ID, dto.Name, dto.Description,
                             dto.Weight, dto.MaxStack, dto.TileSizeX, dto.TileSizeY, icon, prefab));
            }
        }

        // 원거리 무기 — 전용 클래스가 없어 일반 아이템으로 (클래스 추가 예정)
        foreach (ItemDataDTO_Weapon dto in LoadList<ItemDataDTO_Weapon>(EItemType.WeaponRanged))
        {
            LoadResources(dto, out ERarity rarity, out Sprite icon, out GameObject prefab);
            Add(new Item(EItemType.WeaponRanged, rarity, dto.ID, dto.Name, dto.Description,
                         dto.Weight, dto.MaxStack, dto.TileSizeX, dto.TileSizeY, icon, prefab));
        }

        // 근접 무기
        foreach (ItemDataDTO_Weapon dto in LoadList<ItemDataDTO_Weapon>(EItemType.WeaponMelee))
        {
            LoadResources(dto, out ERarity rarity, out Sprite icon, out GameObject prefab);
            EWeaponType      weaponType      = Enum.Parse<EWeaponType>(dto.WeaponType);
            EWeaponMeleeType weaponMeleeType = Enum.Parse<EWeaponMeleeType>(dto.WeaponMeleeType);
            Add(new Item_Weapon_Melee(rarity, dto.ID, dto.Name, dto.Description,
                                      dto.Weight, dto.TileSizeX, dto.TileSizeY, icon, prefab,
                                      weaponType, weaponMeleeType, dto.Damage, dto.AttackSpeed));
        }

        // 치료
        foreach (ItemDataDTO_Medical dto in LoadList<ItemDataDTO_Medical>(EItemType.Medical))
        {
            LoadResources(dto, out ERarity rarity, out Sprite icon, out GameObject prefab);
            Add(new Item_Medical(rarity, dto.ID, dto.Name, dto.Description,
                                 dto.Weight, dto.TileSizeX, dto.TileSizeY, icon, prefab,
                                 dto.HealAmount, dto.UseTime, Enum.Parse<EMedicalType>(dto.MedicalType)));
        }

        // 음식
        foreach (ItemDataDTO_Food dto in LoadList<ItemDataDTO_Food>(EItemType.Food))
        {
            LoadResources(dto, out ERarity rarity, out Sprite icon, out GameObject prefab);
            Add(new Item_Food(rarity, dto.ID, dto.Name, dto.Description,
                              dto.Weight, dto.MaxStack, dto.TileSizeX, dto.TileSizeY, icon, prefab,
                              dto.HungerAmount, dto.ThirstAmount, dto.UseTime, Enum.Parse<EEatType>(dto.EatType)));
        }

        // 컨테이너
        foreach (ItemDataDTO_Container dto in LoadList<ItemDataDTO_Container>(EItemType.Container))
        {
            LoadResources(dto, out ERarity rarity, out Sprite icon, out GameObject prefab);
            EContainerType containerType = Enum.Parse<EContainerType>(dto.ContainerType);
            Add(new Item_Container(EItemType.Container, rarity, dto.ID, dto.Name, dto.Description,
                                   dto.Weight, dto.MaxStack, dto.TileSizeX, dto.TileSizeY, icon, prefab,
                                   containerType, dto.GridWidth, dto.GridHeight));
        }

        RefreshDebugList();
    }

    // ItemData_{종류}.json 불러오기 — 파일이 없으면 경고 후 빈 목록
    private List<T> LoadList<T>(EItemType itemType) where T : ItemDataDTO
    {
        TextAsset jsonFile = Resources.Load<TextAsset>($"Data/ItemData_{itemType}");
        if (jsonFile == null)
        {
            Debug.LogWarning($"[Mgr_Item] ItemData_{itemType}.json 없음 — Tools/Dead Outlook/구글시트 업데이트 실행 필요");
            return new List<T>();
        }

        return JsonUtility.FromJson<ItemDataList<T>>(jsonFile.text).Items;
    }

    // 공통 리소스 불러오기 — 희귀도 · 아이콘 · 프리팹 (경로가 틀리면 기본값으로 대체)
    private void LoadResources(ItemDataDTO dto, out ERarity rarity, out Sprite icon, out GameObject prefab)
    {
        rarity = Enum.Parse<ERarity>(dto.Rarity);

        icon = Resources.Load<Sprite>(dto.IconKey);
        if (icon == null)
        {
            Debug.LogWarning($"아이콘을 찾지 못했습니다 ({dto.ID}) — {dto.IconKey}");
            icon = Resources.Load<Sprite>("UI/Icon/Icon_Temp"); // 경로가 틀리면 기본 아이콘으로 대체
        }

        string prefabKey = string.IsNullOrEmpty(dto.PrefabKey) ? "Prefab/Item/PF_TestItem" : dto.PrefabKey;
        prefab = Resources.Load<GameObject>(prefabKey);
        if (prefab == null)
        {
            Debug.LogWarning($"프리팹을 찾지 못했습니다 ({dto.ID}) — {prefabKey}");
            prefab = Resources.Load<GameObject>("Prefab/Item/PF_TestItem"); // 경로가 틀리면 테스트 프리팹으로 대체
        }
    }

    // 아이템 추가
    public void Add(Item item)
    {
        Items[item.ID] = item;
    }

    // 로드 완료 후 Dictionary → 디버그 리스트 변환
    public void RefreshDebugList()
    {
        DebugItems.Clear();
        DebugBagItems.Clear();
        foreach (var pair in Items)
        {
            switch (pair.Value)
            {
                case Item_Container bag:
                    DebugBagItems.Add(bag);
                    break;
                default:
                    DebugItems.Add(pair.Value);
                    break;
            }
        }

        //Debug.Log("아이템 로드 완료");
    }

    #endregion
}
