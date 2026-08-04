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

    private void Awake()
    {
        Inst = this;
        LoadFromJson();
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
    // JSON에서 아이템 데이터 로드
    private void LoadFromJson()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Data/ItemData");
        if (jsonFile == null)
        {
            Debug.LogWarning("[Mgr_Item] ItemData.json 없음 — Tools/Dead Outlook/Fetch Item Data 실행 필요");
            return;
        }

        ItemDataList dataList = JsonUtility.FromJson<ItemDataList>(jsonFile.text);
        foreach (ItemDataDTO dto in dataList.Items)
        {
            EItemType itemType = (EItemType)System.Enum.Parse(typeof(EItemType), dto.ItemType);
            ERarity rarity = (ERarity)System.Enum.Parse(typeof(ERarity), dto.Rarity);

            Sprite icon = Resources.Load<Sprite>(dto.IconKey) ?? Resources.Load<Sprite>("UI/Icon/Temp_ItemIcon");
            string     prefabKey = string.IsNullOrEmpty(dto.PrefabKey) ? "Prefab/Item/PF_TestItem" : dto.PrefabKey;
            GameObject prefab    = Resources.Load<GameObject>(prefabKey);

            Item item;
            switch (itemType)
            {
                case EItemType.Container:
                    EContainerType containerType = Enum.Parse<EContainerType>(dto.ContainerType);
                    item = new Item_Container(itemType, rarity, dto.ID, dto.Name, dto.Description,
                                        dto.Weight, dto.MaxStack, dto.TileSizeX, dto.TileSizeY, icon, prefab,
                                        containerType, dto.GridWidth, dto.GridHeight);
                    break;
                case EItemType.WeaponMelee:
                    EWeaponType      weaponType      = Enum.Parse<EWeaponType>(dto.WeaponType);
                    EWeaponMeleeType weaponMeleeType = Enum.Parse<EWeaponMeleeType>(dto.WeaponMeleeType);
                    item = new Item_Weapon_Melee(rarity, dto.ID, dto.Name, dto.Description,
                                        dto.Weight, dto.TileSizeX, dto.TileSizeY, icon, prefab,
                                        weaponType, weaponMeleeType, dto.Damage, dto.AttackSpeed);
                    break;
                default:
                    item = new Item(itemType, rarity, dto.ID, dto.Name, dto.Description,
                                    dto.Weight, dto.MaxStack, dto.TileSizeX, dto.TileSizeY, icon, prefab);
                    break;
            }
            Add(item);
        }

        RefreshDebugList();
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
