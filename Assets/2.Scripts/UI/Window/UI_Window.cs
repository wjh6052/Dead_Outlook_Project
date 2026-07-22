using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IUI_WindowContent
{
    UI_Window Window { get; set; }
    Vector2   WindowSize { get; }
}

// 창의 헤더에 부착 — 제목 표시 및 닫기 버튼 담당
public class UI_Window : MonoBehaviour
{

    private RectTransform WindowRT;
    private Transform Content;


    private void Awake()
    {
        // 닫기 버튼
        transform.Find("UI_WindowDragHandle/Close_Btn").GetComponent<Button>().onClick.AddListener(WindowClose);
        
        WindowRT = GetComponent<RectTransform>();

        Content = transform.Find("Content").transform;

        Transform dragHandle = transform.Find("UI_WindowDragHandle");
        if (dragHandle != null)
            dragHandle.GetComponent<UI_WindowDragHandle>().Init(WindowRT);
    }
    

    public void Init(string title, IUI_WindowContent content)
    {
        transform.Find("UI_WindowDragHandle/Title_TMP").GetComponent<TextMeshProUGUI>().text = title;
        WindowRT.sizeDelta = content.WindowSize + new Vector2(24, 75);

        content.Window = this;
        (content as MonoBehaviour)?.transform.SetParent(Content, false);
    }

    public void WindowClose()
    {
        Destroy(WindowRT.gameObject);
    }
}
