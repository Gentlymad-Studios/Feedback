using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DetailPopup : MonoBehaviour {

    public TMP_Text title;
    public TMP_Text description;
    public Button closeButton;
    public Action onClosePopup;

    private void Start() {
        onClosePopup += OnCloseDefault;
        closeButton.onClick.AddListener(() => onClosePopup.Invoke());
    }

    public void FillDetailPopup(string title, string description) {
        this.title.text = title;
        this.description.text = description;
    }

    private void OnCloseDefault() {
        title.text = "";
        description.text = "";
        Destroy(this.gameObject);
    }
}
