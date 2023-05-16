using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class TopbarButtons : MonoBehaviour {
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

        if(searchBtn != null){
            Debug.Log("searchBtn");
        }
        if(reportBtn != null){
            Debug.Log("reportBtn");
        }

        reportBtn.RegisterCallback<ClickEvent>(OnRBtnClick);
        
    }
    public void OnRBtnClick(ClickEvent evt){
        searchTab = topbar.rootVisualElement.Q("SearchTab") as VisualElement;
        searchTab.style.display = DisplayStyle.None;
        Debug.Log(searchTab);
        Debug.Log("Display None");
    }





    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
