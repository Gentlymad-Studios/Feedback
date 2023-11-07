using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Feedback {
    public class Loading : VisualElement {
        public Label loadingLbl;
        public Button loadingAbort;
        public VisualElement loadingSpinner;
        public VisualElement popup;
        public Action abortCallback;

        private float animationTime;
        private float duration = 1f;
        private AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public Loading(VisualTreeAsset vta) {
            TemplateContainer tmpCnt = vta.Instantiate();
            tmpCnt.style.height = new Length(100, LengthUnit.Percent);
            tmpCnt.style.width = new Length(100, LengthUnit.Percent);
            tmpCnt.style.justifyContent = Justify.Center;
            tmpCnt.style.alignItems = Align.Center;

            Add(tmpCnt);

            name = "LoadWH";
            style.position = Position.Absolute;
            style.display = DisplayStyle.Flex;
            style.height = new Length(100, LengthUnit.Percent);
            style.width = new Length(100, LengthUnit.Percent);
            loadingLbl = this.Q("loadingLabel") as Label;
            loadingSpinner = this.Q("spinner");
            loadingAbort = this.Q("abortButton") as Button;
            loadingLbl.text = "load tickets...";

            RegisterEvents();

            Hide();
        }

        public void Show(string text, bool abortable = false, Action abortCallback = null) {
            loadingLbl.text = text;
            loadingAbort.style.display = abortable ? DisplayStyle.Flex : DisplayStyle.None;

            this.abortCallback = abortCallback;

            if (abortCallback != null) {
                RegisterEvents();
            }

            style.display = DisplayStyle.Flex;
        }

        public void Hide() {
            style.display = DisplayStyle.None;
        }

        private void RegisterEvents() {
            UnregisterEvents();
            loadingAbort.clicked += abortCallback;
        }

        private void UnregisterEvents() {
            loadingAbort.clicked -= abortCallback;
        }

        public void SpinLoadingIcon() {
            while (animationTime > duration) {
                animationTime -= duration;
            }
            var t = animationTime / duration;
            var angle = rotationCurve.Evaluate(t) * 360f;
            var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            loadingSpinner.transform.rotation = rotation;
            animationTime += Time.deltaTime;
        }
    }
}