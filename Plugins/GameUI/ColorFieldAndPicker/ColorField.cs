using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI {
    public class ColorField : BaseField<Color> {
        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory : UxmlFactory<ColorField, UxmlTraits> {
        }

        [UnityEngine.Scripting.Preserve]
        public new class UxmlTraits : BaseFieldTraits<Color, UxmlColorAttributeDescription> {
            private readonly UxmlStringAttributeDescription resetLabel = new UxmlStringAttributeDescription { name = "reset-label", defaultValue = null };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc) {
                base.Init(ve, bag, cc);
                var item = ve as ColorField;
                item.resetLabel = resetLabel.GetValueFromBag(bag, cc);
                item.UpdateResetButton();
            }
        }

        public event System.Action ResetButtonPressed;
        public ColorPopup ColorPopup {
            get; set;
        }
        public VisualElement PopupRoot {
            get; set;
        }

        private string resetLabel = null;
        private ColorFieldInput colorFieldInput;
        private Button resetButton;

        //private StyleSheet stylesheet;
        //private StyleSheet StyleSheet {
        //    get {
        //        if (stylesheet == null) {
        //            stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(stylesheetGUID));
        //        }

        //        return stylesheet;
        //    }
        //}
        //private const string stylesheetGUID = "bdacf44a7b5bf5c4d94ea5784f944720";
        private const string ussFieldName = "color-field";
        private const string ussFieldLabel = "color-field__label";
        private const string ussFieldResetButton = "color-field__reset-button";

        // ------------------------------------------------------------------------------------------------------------

        public ColorField()
            : this(null, null, new ColorFieldInput()) {
        }

        public ColorField(string label, string resetLabel = null)
            : this(label, resetLabel, new ColorFieldInput()) {
        }

        private ColorField(string label, string resetLabel, ColorFieldInput colorFieldInput)
            : base(label, colorFieldInput) {
            this.colorFieldInput = colorFieldInput;
            this.resetLabel = resetLabel;

            //styleSheets.Add(StyleSheet);
            AddToClassList(ussFieldName);

            labelElement.AddToClassList(ussFieldLabel);

            colorFieldInput.RegisterCallback<ClickEvent>(OnClickEvent);

            UpdateResetButton();

            RegisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
        }

        public override void SetValueWithoutNotify(Color newValue) {
            base.SetValueWithoutNotify(newValue);
            colorFieldInput.SetColor(newValue);
        }

        public void ShowPopup() {
            if (ColorPopup == null) {
                ColorPopup = new ColorPopup();
                PopupRoot.Add(ColorPopup);
            }

            ColorPopup.Show(value, c => value = c);
        }

        private void UpdateResetButton() {
            if (resetLabel == null) {
                if (resetButton != null) {
                    Remove(resetButton);
                    resetButton = null;
                }
            } else {
                if (resetButton == null) {
                    resetButton = new Button();
                    resetButton.AddToClassList(ussFieldResetButton);
                    resetButton.clicked += OnResetButton;
                    Add(resetButton);
                }
                resetButton.text = resetLabel;
            }
        }

        private void OnGeometryChangedEvent(GeometryChangedEvent ev) {
            UnregisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
            colorFieldInput.SetColor(value);
        }

        private void OnClickEvent(ClickEvent ev) {
            ShowPopup();
        }

        private void OnResetButton() {
            ResetButtonPressed?.Invoke();
        }

        // ============================================================================================================

        private class ColorFieldInput : VisualElement {
            public VisualElement rgbField;
            public VisualElement alphaField;

            private const string ussFieldInput = "color-field__input";
            private const string ussFieldInputRGB = "color-field__input-rgb";
            private const string ussFieldInputAlpha = "color-field__input-alpha";
            private const string ussFieldInputAlphaContainer = "color-field__input-alpha-container";

            public ColorFieldInput() {
                AddToClassList(ussFieldInput);

                rgbField = new VisualElement();
                rgbField.AddToClassList(ussFieldInputRGB);
                Add(rgbField);

                var alphaFieldContainer = new VisualElement();
                alphaFieldContainer.AddToClassList(ussFieldInputAlphaContainer);
                //Add(alphaFieldContainer);

                alphaField = new VisualElement();
                alphaField.AddToClassList(ussFieldInputAlpha);
                alphaFieldContainer.Add(alphaField);
            }

            public void SetColor(Color color) {
                rgbField.style.backgroundColor = new Color(color.r, color.g, color.b, 1f);
                alphaField.style.width = alphaField.parent.resolvedStyle.width * color.a;
            }
        }

        // ============================================================================================================
    }
}