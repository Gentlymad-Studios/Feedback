using UnityEngine;
using UnityEngine.UIElements;

public class Topbar : MonoBehaviour
{
    UIDocument topbar;
    Button searchBtn;
    Button reportBtn;
    VisualElement searchTab;
    VisualElement reportTab;

    void OnEnable() {
        topbar = GetComponent<UIDocument>();
        if(topbar == null){
            Debug.LogError("Topbar missing");
        }
        searchBtn = topbar.rootVisualElement.Q("searchBtn") as Button;
        reportBtn = topbar.rootVisualElement.Q("reportBtn") as Button;

        if(searchBtn != null){
            Debug.Log("searchBtn");
        }
        if(reportBtn != null){
            Debug.Log("reportBtn");
        }

        reportBtn.RegisterCallback<ClickEvent>(ShowReport);
        searchBtn.RegisterCallback<ClickEvent>(ShowSearch);
    }
    public void ShowReport(ClickEvent evt){
        Debug.Log("Switch2Report");
        searchTab = topbar.rootVisualElement.Q("SearchTab") as VisualElement;
        searchTab.style.display = DisplayStyle.None;
        Debug.Log("searchTab Display None");
        reportTab = topbar.rootVisualElement.Q("ReportTab") as VisualElement;
        reportTab.style.display = DisplayStyle.Flex;

        reportBtn = topbar.rootVisualElement.Q("reportBtn") as Button;
        reportBtn.style.backgroundColor = new Color(255, 255, 255, 1);
        searchBtn = topbar.rootVisualElement.Q("searchBtn") as Button;
        searchBtn.style.backgroundColor = new Color(133, 133, 133, 1);
    }
    public void ShowSearch(ClickEvent evt){
        Debug.Log("Switch2Search");
        reportTab = topbar.rootVisualElement.Q("ReportTab") as VisualElement;
        reportTab.style.display = DisplayStyle.None;
        Debug.Log("ReportTab Display None");
        searchTab = topbar.rootVisualElement.Q("SearchTab") as VisualElement;
        searchTab.style.display = DisplayStyle.Flex;

        searchBtn = topbar.rootVisualElement.Q("searchBtn") as Button;
        searchBtn.style.backgroundColor = new Color(255, 255, 255, 1);
        reportBtn = topbar.rootVisualElement.Q("reportBtn") as Button;
        reportBtn.style.backgroundColor = new Color(133, 133, 133, 1);
    }
}
