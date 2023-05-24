using UnityEngine.UIElements;

public class DetailPopup : VisualElement {

    public Label title;
    public Label description;
    public VisualElement popup;
    public Button closeButton;

    public DetailPopup(VisualTreeAsset vta) {
        Add(vta.Instantiate());

        name = "taskDetail";
        style.position = Position.Absolute;
        style.height = new Length(100, LengthUnit.Percent);
        style.width = new Length(100, LengthUnit.Percent);

        title = this.Q("taskTitleLbl") as Label;
        description = this.Q("taskDescriptionLbl") as Label;
        closeButton = this.Q("closeBtn") as Button;
        popup = this.Q("popup");

        RegisterEvents();
    }

    public void FillDetailPopup(string title, string description) {
        this.title.text = title;
        this.description.text = description;
    }

    public void Show() {
        style.display = DisplayStyle.Flex;
    }

    public void Hide() {
        style.display = DisplayStyle.None;
    }

    public void Click(MouseDownEvent evt) {
        if (!popup.layout.Contains(evt.mousePosition)) {
            Hide();
        }
    }

    public void RegisterEvents() {
        UnregisterEvents();
        closeButton.clicked += Hide;
        RegisterCallback<MouseDownEvent>(Click);
    }

    public void UnregisterEvents() {
        closeButton.clicked -= Hide;
        UnregisterCallback<MouseDownEvent>(Click);
    }
}
