using UnityEngine;

// 컨테이너 장착 아이템 — EquipItem을 상속, 가방 · 조끼 · 벨트처럼 내부 인벤토리를 가진 장착 아이템의 공통 부모
public class EquipItem_Container : EquipItem
{
    public Item_Container ContainerData => ItemData as Item_Container; // 이 컨테이너의 아이템 데이터 — 부모의 ItemData를 컨테이너 데이터로 캐스팅
}
