using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_W_ItemInfo : MonoBehaviour, IUI_WindowContent
{
    public UI_Window Window { get; set; }
    [SerializeField] private Vector2 _windowSize = new Vector2(800, 700);
    public Vector2 WindowSize => _windowSize;

    private Image           Icon_BackImage;
    private Image           Icon_Image;
    private TextMeshProUGUI Description_TMP;

    public void Init(Item item)
    {
        Icon_BackImage  = transform.Find("Icon_BackImage").GetComponent<Image>();
        Icon_Image      = Icon_BackImage.transform.Find("Icon_Image").GetComponent<Image>();
        Description_TMP = transform.Find("Description_TMP").GetComponent<TextMeshProUGUI>();

        Icon_BackImage.color = Mgr_UI.Inst.GetRarityColor(item.Rarity);
        Icon_Image.sprite    = item.Icon;
        Description_TMP.text = item.Description;
    }
}
