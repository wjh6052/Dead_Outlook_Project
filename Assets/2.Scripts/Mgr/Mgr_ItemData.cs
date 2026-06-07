using UnityEngine;
using System.Collections.Generic;

public class Mgr_ItemData : MonoBehaviour
{
    private Dictionary<string, Item>       Items          = new Dictionary<string, Item>();
    [SerializeField] private List<Item>     DebugItems     = new List<Item>();     // 디버그용 인스펙터 확인
    [SerializeField] private List<Item_Bag> DebugBagItems  = new List<Item_Bag>(); // 가방 아이템 디버그용

    // ERarity 순서대로 — Normal, Rare, Epic, Unique, Legendary
    [SerializeField] private Color[] RarityColors = new Color[5];
    
    
    public static Mgr_ItemData Inst { get; private set; }

    private void Awake()
    {
        Inst = this;
        LoadFromJson();
    }


    // ERarity에 해당하는 색상 반환
    public Color GetRarityColor(ERarity rarity) => RarityColors[(int)rarity];

    // ID로 아이템 반환
    public Item GetItem(string id)
    {
        Items.TryGetValue(id, out Item item);
        return item?.Copy();
    }



    #region 플레이시 아이템 정보 변환
    // JSON에서 아이템 데이터 로드
    private void LoadFromJson()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Data/ItemData");
        if (jsonFile == null)
        {
            Debug.LogWarning("[Mgr_ItemData] ItemData.json 없음 — Tools/Dead Outlook/Fetch Item Data 실행 필요");
            return;
        }

        ItemDataList dataList = JsonUtility.FromJson<ItemDataList>(jsonFile.text);
        foreach (ItemDataDTO dto in dataList.Items)
        {
            EItemType itemType = (EItemType)System.Enum.Parse(typeof(EItemType), dto.ItemType);
            ERarity rarity = (ERarity)System.Enum.Parse(typeof(ERarity), dto.Rarity);

            Sprite icon = Resources.Load<Sprite>(dto.IconKey) ?? Resources.Load<Sprite>("UI/Icon/Temp_ItemIcon");
            GameObject prefab = Resources.Load<GameObject>(dto.PrefabKey) ?? Resources.Load<GameObject>("UI/Prefab/Temp_ItemPrefab");

            Item item;
            switch (itemType)
            {
                case EItemType.Bag:
                    item = new Item_Bag(itemType, rarity, dto.ID, dto.Name, dto.Description,
                                        dto.Weight, dto.MaxStack, dto.TileSizeX, dto.TileSizeY, icon, prefab,
                                        dto.GridWidth, dto.GridHeight, dto.IsBelt);
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
                case Item_Bag bag:
                    DebugBagItems.Add(bag);
                    break;
                default:
                    DebugItems.Add(pair.Value);
                    break;
            }
        }

        Debug.Log("아이템 로드 완료");
    }

    #endregion
}
