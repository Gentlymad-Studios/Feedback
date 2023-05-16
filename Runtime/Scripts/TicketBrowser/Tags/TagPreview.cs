using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TagPreview : MonoBehaviour {

    public ScriptableTag scriptableTag;
    public Action addTagToTagList;
    public Action removeFromTagList;
    public Button btn;

    private TMP_Text text;
    private bool selected = false;

    private void Start() {
        text = gameObject.GetComponentInChildren<TMP_Text>();
        text.text = scriptableTag.tagName;
        btn = gameObject.GetComponent<Button>();
        btn.onClick.AddListener(onClick);
    }

    private void onClick() {
        if (!selected) {
            Select();
        } else {
            Deselect();
        }
    }
    public void Select() {
        selected = true;
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(111, 111, 111, 255);
        colors.highlightedColor = new Color(111, 111, 111, 255);
        btn.colors = colors;
        addTagToTagList.Invoke();
    }
    public void Deselect() {
        selected = false;
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color32(225, 225, 225, 255);
        btn.colors = colors;
        removeFromTagList.Invoke();
    }

}
