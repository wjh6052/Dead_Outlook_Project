using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryData
{
    [SerializeField] public int          GridWidth  = 0;              // 그리드 가로 슬롯 수
    [SerializeField] public int          GridHeight = 0;              // 그리드 세로 슬롯 수
    [SerializeField] public List<Item>   Items      = new List<Item>(); // 저장된 아이템 목록

    public Action OnChanged;

    // UI 없이 아이템이 들어갈 공간이 있는지 확인 (회전 포함)
    public bool CanAddItem(Item item)
    {
        bool[,] occupied = new bool[GridWidth, GridHeight];
        foreach (Item placed in Items)
        {
            int sw = placed.IsRotated ? placed.TileSizeY : placed.TileSizeX;
            int sh = placed.IsRotated ? placed.TileSizeX : placed.TileSizeY;
            for (int dy = 0; dy < sh; dy++)
                for (int dx = 0; dx < sw; dx++)
                    occupied[placed.SlotX + dx, placed.SlotY + dy] = true;
        }

        return CanFitAtAnyPos(item, occupied) ||
               (item.TileSizeX != item.TileSizeY && CanFitAtAnyPos(item, occupied, rotated: true));
    }

    // UI 없이 기존 스택에 겹치기 후 남은 수량을 첫 번째 빈 자리에 배치
    public bool TryAddItem(Item item)
    {
        // 기존 스택에 겹치기 시도
        foreach (Item placed in Items)
        {
            if (placed.ID != item.ID || placed.Amount >= placed.MaxStack) continue;

            int transfer = Mathf.Min(placed.MaxStack - placed.Amount, item.Amount);
            placed.AddAmount(transfer);
            item.DecreaseAmount(transfer);

            if (item.Amount <= 0) { OnChanged?.Invoke(); return true; }
        }

        bool[,] occupied = new bool[GridWidth, GridHeight];
        foreach (Item placed in Items)
        {
            int sw = placed.IsRotated ? placed.TileSizeY : placed.TileSizeX;
            int sh = placed.IsRotated ? placed.TileSizeX : placed.TileSizeY;
            for (int dy = 0; dy < sh; dy++)
                for (int dx = 0; dx < sw; dx++)
                    occupied[placed.SlotX + dx, placed.SlotY + dy] = true;
        }

        int sizeX = item.IsRotated ? item.TileSizeY : item.TileSizeX;
        int sizeY = item.IsRotated ? item.TileSizeX : item.TileSizeY;

        for (int y = 0; y <= GridHeight - sizeY; y++)
        {
            for (int x = 0; x <= GridWidth - sizeX; x++)
            {
                bool fits = true;
                for (int dy = 0; dy < sizeY && fits; dy++)
                    for (int dx = 0; dx < sizeX && fits; dx++)
                        if (occupied[x + dx, y + dy]) fits = false;

                if (fits) { item.SetSlot(x, y); Items.Add(item); OnChanged?.Invoke(); return true; }
            }
        }

        return false;
    }

    private bool CanFitAtAnyPos(Item item, bool[,] occupied, bool rotated = false)
    {
        int sw = rotated ? item.TileSizeY : item.TileSizeX;
        int sh = rotated ? item.TileSizeX : item.TileSizeY;

        for (int y = 0; y <= GridHeight - sh; y++)
        {
            for (int x = 0; x <= GridWidth - sw; x++)
            {
                bool fits = true;
                for (int dy = 0; dy < sh && fits; dy++)
                    for (int dx = 0; dx < sw && fits; dx++)
                        if (occupied[x + dx, y + dy]) fits = false;
                if (fits) return true;
            }
        }
        return false;
    }
}
