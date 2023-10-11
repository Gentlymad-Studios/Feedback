using System;
using UnityEngine;
using UnityEngine.UIElements;

public class TagPreview {
    public VisualElement ui;
    public Label tagLbl;
    public Button removeBtn;

    public Action addTagToTagList;
    public Action removeFromTagList;
    public string title;
    public string gid;

    public TagPreview(VisualElement ui, string title, string gid = "", bool displayOnly = false) {
        this.ui = ui;
        this.title = title;
        this.gid = gid;

        tagLbl = ui.Q("label") as Label;
        tagLbl.text = title;

        removeBtn = ui.Q("removeBtn") as Button;
        if (displayOnly) {
            removeBtn.AddToClassList("hide");
        } else {
            removeBtn.clicked += () => ToggleTag(false);
        }
    }

    public void ToggleTag(bool selected) {
        if (selected) {
            ui.style.display = DisplayStyle.Flex;
            addTagToTagList.Invoke();
        } else {
            ui.style.display = DisplayStyle.None;
            removeFromTagList.Invoke();
        }
    }

    //private void TagLbl_clicked(ClickEvent evt) {
    //    if (!selected) {
    //        Select();
    //    } else {
    //        Deselect();
    //    }
    //}

    //public void Select() {
    //    selected = true;
    //    ui.RemoveFromClassList("tagLabel");
    //    ui.AddToClassList("tagLabelSelected");
    //    addTagToTagList.Invoke();
    //}

    //public void Deselect() {
    //    selected = false;
    //    ui.AddToClassList("tagLabel");
    //    ui.RemoveFromClassList("tagLabelSelected");
    //    removeFromTagList.Invoke();
    //}

}
