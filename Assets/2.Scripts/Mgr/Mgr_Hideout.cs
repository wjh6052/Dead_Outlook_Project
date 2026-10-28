using UnityEngine;

// 은신처 데이터 관리 — Mgr_Scene에 배치
// - 씬 전환과 무관하게 은신처 관련 데이터를 영구 보관
public class Mgr_Hideout : MonoBehaviour
{

    [Header("보관함")]
    [SerializeField] public InventoryData StorageData = new InventoryData(); // 은신처 보관함 데이터

    public static Mgr_Hideout Inst { get; private set; }

    private void Awake()
    {
        Inst = this;
    }

    private void Start()
    {
        // 테스트
        {
            // 테스트 모든 아이템 추가
            foreach (Item item in Mgr_Item.Inst.GetAllItems())
                StorageData.TryAddItem(item.Copy());
        }

    }
}
