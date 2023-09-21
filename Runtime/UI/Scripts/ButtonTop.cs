using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ButtonTop : MonoBehaviour
{
    UIDocument topbar;
    Button searchBtn;
    Button reportBtn;
    VisualElement searchTab;
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
    }
    public void ShowReport(ClickEvent evt){
        Debug.Log("Switch2Report");
        searchTab = topbar.rootVisualElement.Q("SearchTab") as VisualElement;
        searchTab.style.display = DisplayStyle.None;
        Debug.Log(searchTab);
        Debug.Log("Display None");
    }
}
