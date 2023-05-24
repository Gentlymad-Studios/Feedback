using System;
using UnityEngine.UIElements;

public class Prompt : VisualElement {
    public Label title;
    public Label description;
    public VisualElement popup;
    public Button okButton;
    public Action callback;

    public Prompt(VisualTreeAsset vta) {
        Add(vta.Instantiate());

        name = "prompt";
        style.position = Position.Absolute;
        style.height = new Length(100, LengthUnit.Percent);
        style.width = new Length(100, LengthUnit.Percent);

        title = this.Q("promptTitleLbl") as Label;
        description = this.Q("promptDescriptionLbl") as Label;
        okButton = this.Q("okButton") as Button;

        RegisterEvents();

        Hide();
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
