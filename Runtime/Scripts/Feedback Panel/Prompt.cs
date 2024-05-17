using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Feedback {
    public class Prompt : VisualElement {
        public Label title;
        public Label description;
        public VisualElement popup;
        public Button okButton;
        public Action callback;
        public Button extraButton;
        public Action extraCallback;
        public Toggle dontShowAgainToggle;

        public bool DontShowAgain {
            get {
                return PlayerPrefs.GetInt($"{Application.productName}_{nameof(Feedback)}_{nameof(DontShowAgain)}") == 1 ? true : false;
            }
            set {
                PlayerPrefs.SetInt($"{Application.productName}_{nameof(Feedback)}_{nameof(DontShowAgain)}", value ? 1 : 0);
            }
        }

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
            extraButton = this.Q("additionalButton") as Button;
            dontShowAgainToggle = this.Q("dontShowAgainToggle") as Toggle;

            RegisterEvents();

            Hide();

            RegisterCallback<KeyDownEvent>(e => {
                if (e.keyCode == KeyCode.Escape || e.keyCode == KeyCode.Return) {
                    e.StopImmediatePropagation();
                    Hide();
                }
            });
        }

        public void Show(string title, string description, Action callback = null, string buttonText = "Ok", bool dontShowAgainFlag = false, Action extraCallback = null, string extraButtonText = "") {
            if (DontShowAgain && dontShowAgainFlag) {
                if (callback != null) {
                    callback.Invoke();
                }
                return;
            }

            this.title.text = title;
            this.description.text = description;
            okButton.text = buttonText;

            okButton.Focus();

            dontShowAgainToggle.style.display = dontShowAgainFlag ? DisplayStyle.Flex : DisplayStyle.None;
            extraButton.style.display = string.IsNullOrEmpty(extraButtonText) ? DisplayStyle.None : DisplayStyle.Flex;

            this.callback = callback;
            this.extraCallback = extraCallback;

            RegisterEvents();

            style.display = DisplayStyle.Flex;
        }

        public void Hide() {
            style.display = DisplayStyle.None;
        }

        public void RegisterEvents() {
            UnregisterEvents();
            okButton.clicked += Hide;
            if (callback != null) {
                okButton.clicked += callback;
            }
            if (extraCallback != null) {
                extraButton.clicked += extraCallback;
            }
            dontShowAgainToggle.RegisterValueChangedCallback(DontShowAgain_Toggled);
        }

        public void UnregisterEvents() {
            okButton.clicked -= Hide;
            okButton.clicked -= callback;
            extraButton.clicked -= extraCallback;
            dontShowAgainToggle.UnregisterValueChangedCallback(DontShowAgain_Toggled);
        }

        private void DontShowAgain_Toggled(ChangeEvent<bool> evt) {
            DontShowAgain = evt.newValue;
        }
    }
}