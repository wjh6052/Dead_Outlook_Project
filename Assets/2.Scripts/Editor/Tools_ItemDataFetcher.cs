using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using GoogleSheetsToUnity;

public class Tools_ItemDataFetcher : EditorWindow
{
    private static readonly string SheetID    = "1UCd3jIo20DEq1X40wNzIar4UIqBldI7rN66u5g6hwUc";
    private static readonly string SaveDir    = "Assets/Resources/Data"; // 시트별 파일 저장 폴더 (ItemData_{종류}.json)

    private static int            TotalSheets;
    private static int            LoadedSheets;
    private static int            TotalItems;   // 저장한 전체 아이템 수 (완료 로그용)

    [MenuItem("Tools/Dead Outlook/구글시트 업데이트")]
    public static void FetchItemData()
    {
        TotalSheets  = Enum.GetValues(typeof(EItemType)).Length;
        LoadedSheets = 0;
        TotalItems   = 0;

        Debug.Log("[Tools_ItemDataFetcher] 구글시트 데이터 가져오는 중...");

        foreach (EItemType type in Enum.GetValues(typeof(EItemType)))
        {
            string    sheetName    = $"Item{type}_Sheet";
            EItemType capturedType = type;

            SpreadsheetManager.Read(new GSTU_Search(SheetID, sheetName),
                sheet => OnSheetLoaded(sheet, capturedType));
        }
    }

    // 시트 하나를 읽으면 바로 그 시트의 파일 저장
    private static void OnSheetLoaded(GstuSpreadSheet sheet, EItemType itemType)
    {
        // 타입별 추가 필드 — 종류에 맞는 DTO로 읽어 저장
        switch (itemType)
        {
            case EItemType.Container:
                SaveSheet(itemType, ParseRows<ItemDataDTO_Container>(sheet, itemType, (container, cells) =>
                {
                    container.ContainerType = cells.Count > 10 && !string.IsNullOrEmpty(cells[10].value) ? cells[10].value : "Bag";
                    container.GridWidth     = cells.Count > 11 ? int.Parse(cells[11].value) : 0;
                    container.GridHeight    = cells.Count > 12 ? int.Parse(cells[12].value) : 0;
                }));
                break;
            case EItemType.WeaponMelee:
                SaveSheet(itemType, ParseRows<ItemDataDTO_Weapon>(sheet, itemType, (melee, cells) =>
                {
                    melee.WeaponType      = "Melee";
                    melee.WeaponMeleeType = cells.Count > 10 && !string.IsNullOrEmpty(cells[10].value) ? cells[10].value : "Bat";
                    melee.Damage          = cells.Count > 11 && !string.IsNullOrEmpty(cells[11].value) ? float.Parse(cells[11].value) : 0f;
                    melee.AttackSpeed     = cells.Count > 12 && !string.IsNullOrEmpty(cells[12].value) ? float.Parse(cells[12].value) : 1f;
                }));
                break;
            case EItemType.WeaponRanged:
                SaveSheet(itemType, ParseRows<ItemDataDTO_Weapon>(sheet, itemType, (ranged, cells) =>
                {
                    ranged.WeaponType = "Ranged";
                }));
                break;
            case EItemType.Medical:
                SaveSheet(itemType, ParseRows<ItemDataDTO_Medical>(sheet, itemType, (medical, cells) =>
                {
                    medical.HealAmount  = cells.Count > 10 && !string.IsNullOrEmpty(cells[10].value) ? float.Parse(cells[10].value) : 0f;
                    medical.UseTime     = cells.Count > 11 && !string.IsNullOrEmpty(cells[11].value) ? float.Parse(cells[11].value) : 0f;
                    medical.MedicalType = cells.Count > 12 && !string.IsNullOrEmpty(cells[12].value) ? cells[12].value.Trim() : "Bandage";
                }));
                break;
            case EItemType.Food:
                SaveSheet(itemType, ParseRows<ItemDataDTO_Food>(sheet, itemType, (food, cells) =>
                {
                    food.HungerAmount = cells.Count > 10 && !string.IsNullOrEmpty(cells[10].value) ? float.Parse(cells[10].value) : 0f;
                    food.ThirstAmount = cells.Count > 11 && !string.IsNullOrEmpty(cells[11].value) ? float.Parse(cells[11].value) : 0f;
                    food.UseTime      = cells.Count > 12 && !string.IsNullOrEmpty(cells[12].value) ? float.Parse(cells[12].value) : 0f;
                    food.EatType      = cells.Count > 13 && !string.IsNullOrEmpty(cells[13].value) ? cells[13].value.Trim() : "Eat";
                }));
                break;
            default: // 전용 칸 없는 아이템 (General · Armor · Tool)
                SaveSheet(itemType, ParseRows<ItemDataDTO>(sheet, itemType, null));
                break;
        }

        LoadedSheets++;
        if (LoadedSheets < TotalSheets) return;

        AssetDatabase.Refresh();
        Debug.Log($"[Tools_ItemDataFetcher] 저장 완료 — {TotalItems}개 아이템 → {SaveDir}/ItemData_{{종류}}.json");
    }

    // 시트의 줄들을 DTO 목록으로 변환 — 공통 필드 후 setExtra로 전용 필드 채움
    private static List<T> ParseRows<T>(GstuSpreadSheet sheet, EItemType itemType, Action<T, List<GSTU_Cell>> setExtra) where T : ItemDataDTO, new()
    {
        List<T> items = new List<T>();

        foreach (var pair in sheet.rows.primaryDictionary)
        {
            if (pair.Key == 1) continue;

            List<GSTU_Cell> cells = pair.Value;
            if (cells.Count < 8) continue;

            try
            {
                T dto = new T();
                SetCommon(dto, cells, itemType);
                setExtra?.Invoke(dto, cells);
                items.Add(dto);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Tools_ItemDataFetcher] 파싱 실패 {itemType} Row {pair.Key}: {e.Message}");
            }
        }

        return items;
    }

    // 공통 필드 채우기
    private static void SetCommon(ItemDataDTO dto, List<GSTU_Cell> cells, EItemType itemType)
    {
        dto.ItemType    = itemType.ToString();
        dto.Rarity      = cells[0].value;
        dto.ID          = cells[1].value;
        dto.Name        = cells[2].value;
        dto.Description = cells[3].value;
        dto.Weight      = float.Parse(cells[4].value);
        dto.MaxStack    = int.Parse(cells[5].value);
        dto.TileSizeX   = int.Parse(cells[6].value);
        dto.TileSizeY   = int.Parse(cells[7].value);
        dto.IconKey     = cells.Count > 8 && !string.IsNullOrEmpty(cells[8].value)
                          ? $"UI/Icon/{itemType}/{cells[8].value}" // 타입별 폴더에서 찾기 (예: UI/Icon/WeaponMelee/Icon_Bat)
                          : "UI/Icon/Icon_Temp";                   // 비어 있으면 루트의 기본 아이콘
        dto.PrefabKey   = cells.Count > 9 && !string.IsNullOrEmpty(cells[9].value)
                          ? $"Prefab/Item/{itemType}/{cells[9].value}"
                          : "Prefab/Item/PF_TestItem";
    }

    // 시트 하나를 ItemData_{종류}.json으로 저장
    private static void SaveSheet<T>(EItemType itemType, List<T> items) where T : ItemDataDTO
    {
        if (!Directory.Exists(SaveDir)) Directory.CreateDirectory(SaveDir);

        string path = $"{SaveDir}/ItemData_{itemType}.json";
        string json = JsonUtility.ToJson(new ItemDataList<T> { Items = items }, true);
        File.WriteAllText(path, json);

        TotalItems += items.Count;
    }
}
