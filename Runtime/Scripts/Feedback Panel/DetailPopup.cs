using UnityEngine.UIElements;

namespace Feedback {
    public class DetailPopup : VisualElement {

        public Label title;
        public Label description;
        public VisualElement popup;
        public Button closeButton;

        public DetailPopup(VisualTreeAsset vta) {
            TemplateContainer tmpCnt = vta.Instantiate();
            tmpCnt.style.height = new Length(100, LengthUnit.Percent);
            tmpCnt.style.width = new Length(100, LengthUnit.Percent);
            tmpCnt.style.justifyContent = Justify.Center;
            tmpCnt.style.alignItems = Align.Center;

            Add(tmpCnt);

            name = "taskDetail";
            AddToClassList("popup");
            style.position = Position.Absolute;

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
                evt.StopPropagation();
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
}