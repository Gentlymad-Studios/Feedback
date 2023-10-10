using System;
using UnityEngine;
using UnityEngine.UIElements;

public class Prompt : VisualElement {
    public Label title;
    public Label description;
    public VisualElement popup;
    public Button okButton;
    public Action callback;

    public Prompt(VisualTreeAsset vta) {
        TemplateContainer tmpCnt = vta.Instantiate();
        tmpCnt.style.height = new Length(100, LengthUnit.Percent);
        tmpCnt.style.width = new Length(100, LengthUnit.Percent);
        tmpCnt.style.justifyContent = Justify.Center;
        tmpCnt.style.alignItems = Align.Center;

        Add(tmpCnt);

        name = "prompt";
        AddToClassList("popup");
        style.position = Position.Absolute;

        title = this.Q("promptTitleLbl") as Label;
        description = this.Q("promptDescriptionLbl") as Label;
        okButton = this.Q("okButton") as Button;

        RegisterEvents();

        Hide();

        RegisterCallback<KeyDownEvent>(e => {
            if (e.keyCode == KeyCode.Escape) {
                e.StopImmediatePropagation();
                Hide();
            }
        });
    }

    public void Show(string title, string description, Action callback, string buttonText = "Ok") {
        this.title.text = title;
        this.description.text = description;
        okButton.text = buttonText;

        this.callback = callback;

        RegisterEvents();

        style.display = DisplayStyle.Flex;
    }

    public void Hide() {
        style.display = DisplayStyle.None;
    }

    public void RegisterEvents() {
        UnregisterEvents();
        okButton.clicked += Hide;
        okButton.clicked += callback;
    }

    public void UnregisterEvents() {
        okButton.clicked -= Hide;
        okButton.clicked -= callback;
    }
}
