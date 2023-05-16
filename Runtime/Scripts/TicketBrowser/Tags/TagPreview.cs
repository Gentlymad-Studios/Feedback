using System;
using UnityEngine;
using UnityEngine.UIElements;

public class TagPreview {
    public VisualElement ui;

    public Action addTagToTagList;
    public Action removeFromTagList;
    public Label tagLbl;
    public string title;

    private bool selected = false;

    public TagPreview(VisualElement ui, string title) {
        this.ui = ui.Q("tagRoot");
        this.title = title;

        tagLbl = ui.Q("tagLbl") as Label;
        tagLbl.text = title;

        ui.RegisterCallback<ClickEvent>(TagLbl_clicked);
    }

    private void TagLbl_clicked(ClickEvent evt) {
        if (!selected) {
            Select();
        } else {
            Deselect();
        }
    }

    public void Select() {
        selected = true;
        ui.RemoveFromClassList("tagLabel");
        ui.AddToClassList("tagLabelSelected");
        addTagToTagList.Invoke();
    }

    public void Deselect() {
        selected = false;
        ui.AddToClassList("tagLabel");
        ui.RemoveFromClassList("tagLabelSelected");
        removeFromTagList.Invoke();
    }

}
