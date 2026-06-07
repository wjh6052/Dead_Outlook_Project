using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using GoogleSheetsToUnity;

public class ItemDataFetcher : EditorWindow
{
    private static readonly string SheetID    = "1UCd3jIo20DEq1X40wNzIar4UIqBldI7rN66u5g6hwUc";
    private static readonly string SavePath   = "Assets/Resources/Data/ItemData.json";

    private static ItemDataList   DataList    = new ItemDataList();
    private static int            TotalSheets;
    private static int            LoadedSheets;

    [MenuItem("Tools/Dead Outlook/구글시트 업데이트")]
    public static void FetchItemData()
    {
        DataList     = new ItemDataList();
        TotalSheets  = Enum.GetValues(typeof(EItemType)).Length;
        LoadedSheets = 0;

        Debug.Log("[ItemDataFetcher] 구글시트 데이터 가져오는 중...");

        foreach (EItemType type in Enum.GetValues(typeof(EItemType)))
        {
            string    sheetName    = $"Item{type}_Sheet";
            EItemType capturedType = type;

            SpreadsheetManager.Read(new GSTU_Search(SheetID, sheetName),
                sheet => OnSheetLoaded(sheet, capturedType));
        }
    }

    private static void OnSheetLoaded(GstuSpreadSheet sheet, EItemType itemType)
    {
        foreach (var pair in sheet.rows.primaryDictionary)
        {
            if (pair.Key == 1) continue;

            List<GSTU_Cell> cells = pair.Value;
            if (cells.Count < 8) continue;

            try
            {
                DataList.Items.Add(new ItemDataDTO
                {
                    ItemType    = itemType.ToString(),
                    Rarity      = cells[0].value,
                    ID          = cells[1].value,
                    Name        = cells[2].value,
                    Description = cells[3].value,
                    Weight      = float.Parse(cells[4].value),
                    MaxStack    = int.Parse(cells[5].value),
                    TileSizeX   = int.Parse(cells[6].value),
                    TileSizeY   = int.Parse(cells[7].value),
                    IconKey     = cells.Count > 8 ? cells[8].value : "UI/Icon/Temp_ItemIcon",
                    PrefabKey   = cells.Count > 9 ? cells[9].value : "",
                    GridWidth   = cells.Count > 10 ? int.Parse(cells[10].value) : 0,
                    GridHeight  = cells.Count > 11 ? int.Parse(cells[11].value) : 0,
                    IsBelt      = cells.Count > 12 ? bool.Parse(cells[12].value) : false,
                });
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ItemDataFetcher] 파싱 실패 Row {pair.Key}: {e.Message}");
            }
        }

        LoadedSheets++;
        if (LoadedSheets == TotalSheets) SaveToJson();
    }

    private static void SaveToJson()
    {
        string dir = Path.GetDirectoryName(SavePath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        string json = JsonUtility.ToJson(DataList, true);
        File.WriteAllText(SavePath, json);
        AssetDatabase.Refresh();

        Debug.Log($"[ItemDataFetcher] 저장 완료 — {DataList.Items.Count}개 아이템 → {SavePath}");
    }
}
