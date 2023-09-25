using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI {
    public class ColorPopup : PopupPanel {
        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory : UxmlFactory<ColorPopup, UxmlTraits> {
        }

        private float H, S, V, A;

        private System.Action<Color> onSubmit;

        private Slider rSlider;
        private Slider gSlider;
        private Slider bSlider;
        private Slider aSlider;
        private Slider2D gradientSlider;
        private Slider hueSlider;
        private VisualElement gradientSliderDragger;
        private VisualElement hueSliderDragger;

        private Texture2D gradientTexture;
        private Texture2D hueSliderTexture;

        private StyleSheet stylesheet;
        private StyleSheet StyleSheet {
            get {
                if (stylesheet == null) {
                    stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(stylesheetGUID));
                }

                return stylesheet;
            }
        }
        private const string stylesheetGUID = "9153a560cf1694b4cb538dbd33d91d63";
        private const string ussPopupClassName = "color-popup";
        private const string ussContentBack = ussPopupClassName + "__content-area";
        private const string ussRSlider = ussPopupClassName + "__red-slider";
        private const string ussGSlider = ussPopupClassName + "__green-slider";
        private const string ussBSlider = ussPopupClassName + "__blue-slider";
        private const string ussASlider = ussPopupClassName + "__alpha-slider";
        private const string ussGradientArea = ussPopupClassName + "__gradient-area";
        private const string ussGradientSlider = ussPopupClassName + "__gradient-slider";
        private const string ussHueSlider = ussPopupClassName + "__hue-slider";

        // ------------------------------------------------------------------------------------------------------------

        public ColorPopup()
            : this(null, null) {
        }

        public ColorPopup(string heading, string buttonLabel) {
            styleSheets.Add(StyleSheet);

            // panel
            mainPanel.AddToClassList(ussPopupClassName);

            // content area
            var content = new VisualElement();
            content.AddToClassList(ussContentBack);
            mainPanel.Add(content);

            // gradient area
            var gradientArea = new VisualElement();
            gradientArea.AddToClassList(ussGradientArea);
            content.Add(gradientArea);

            // gradient block
            gradientSlider = new Slider2D();
            gradientSliderDragger = gradientSlider.Q("dragger");
            gradientSlider.AddToClassList(ussGradientSlider);
            gradientArea.Add(gradientSlider);

            // hue slider
            hueSlider = new UnityEngine.UIElements.Slider(null, 0f, 360f, SliderDirection.Vertical, 0f);
            hueSliderDragger = hueSlider.Q("unity-dragger");
            hueSlider.AddToClassList(ussHueSlider);
            gradientArea.Add(hueSlider);

            // rgba sliders
            rSlider = new Slider("Red", 0f, 1f, SliderDirection.Horizontal, 0f);
            gSlider = new Slider("Green", 0f, 1f, SliderDirection.Horizontal, 0f);
            bSlider = new Slider("Blue", 0f, 1f, SliderDirection.Horizontal, 0f);
            aSlider = new Slider("Alpha", 0f, 1f, SliderDirection.Horizontal, 0f);

            rSlider.showInputField = true;
            gSlider.showInputField = true;
            bSlider.showInputField = true;
            aSlider.showInputField = true;

            rSlider.AddToClassList(ussRSlider);
            gSlider.AddToClassList(ussGSlider);
            bSlider.AddToClassList(ussBSlider);
            aSlider.AddToClassList(ussASlider);

            content.Add(rSlider);
            content.Add(gSlider);
            content.Add(bSlider);
            content.Add(aSlider);

            rSlider.RegisterValueChangedCallback(ev => SetColorFromRSliders(ev.newValue));
            gSlider.RegisterValueChangedCallback(ev => SetColorFromGSliders(ev.newValue));
            bSlider.RegisterValueChangedCallback(ev => SetColorFromBSliders(ev.newValue));
            aSlider.RegisterValueChangedCallback(ev => A = ev.newValue);
            hueSlider.RegisterValueChangedCallback(SetColorFromHueSlider);
            gradientSlider.RegisterValueChangedCallback(SetColorFromGradientSlider);
        }

        public void Show(Color color, System.Action<Color> onSubmit) {
            Color.RGBToHSV(color, out H, out S, out V);
            A = color.a;

            this.onSubmit = onSubmit;

            CreateTextures();
            OnColorChanged(true, true);
            aSlider.SetValueWithoutNotify(A);

            base.Show();
        }

        public override void Hide() {
            gradientSlider.style.backgroundImage = null;
            hueSlider.style.backgroundImage = null;

            Object.Destroy(gradientTexture);
            Object.Destroy(hueSliderTexture);

            onSubmit = null;
            base.Hide();
        }

        public override void PointerDownEvent() {
            OnSubmitButton();
            base.PointerDownEvent();
        }

        // ------------------------------------------------------------------------------------------------------------

        private void OnSubmitButton() {
            var c = Color.HSVToRGB(H, S, V);
            c.a = A;

            onSubmit?.Invoke(c);

            Hide();
        }

        private void SetColorFromGradientSlider(ChangeEvent<Vector2> ev) {
            S = ev.newValue.x;
            V = ev.newValue.y;
            OnColorChanged(false, false);
        }

        private void SetColorFromHueSlider(ChangeEvent<float> ev) {
            H = ev.newValue / 360f; // hue slider value 0..360
            OnColorChanged(false, true);
        }

        private void SetColorFromRSliders(float value) {
            Color c = Color.HSVToRGB(H, S, V);
            c.r = value;
            Color.RGBToHSV(c, out H, out S, out V);
            OnColorChanged(true, true);
        }

        private void SetColorFromGSliders(float value) {
            Color c = Color.HSVToRGB(H, S, V);
            c.g = value;
            Color.RGBToHSV(c, out H, out S, out V);
            OnColorChanged(true, true);
        }

        private void SetColorFromBSliders(float value) {
            Color c = Color.HSVToRGB(H, S, V);
            c.b = value;
            Color.RGBToHSV(c, out H, out S, out V);
            OnColorChanged(true, true);
        }

        // ------------------------------------------------------------------------------------------------------------

        private void OnColorChanged(bool updateHue, bool updateGradient) {
            var c = Color.HSVToRGB(H, S, V);
            hueSliderDragger.style.backgroundColor = Color.HSVToRGB(H, 1f, 1f);
            gradientSliderDragger.style.backgroundColor = c;

            rSlider.SetValueWithoutNotify(Round(c.r, 3));
            gSlider.SetValueWithoutNotify(Round(c.g, 3));
            bSlider.SetValueWithoutNotify(Round(c.b, 3));

            if (updateHue) {
                hueSlider.SetValueWithoutNotify(H * 360f);
            }

            if (updateGradient) {
                UpdateGradientTexture();
                gradientSlider.SetValueWithoutNotify(new Vector2(S, V));
            }
        }

        private void CreateTextures() {
            gradientTexture = new Texture2D(64, 64, TextureFormat.RGB24, false) { filterMode = FilterMode.Point };
            gradientTexture.hideFlags = HideFlags.HideAndDontSave;
            gradientSlider.style.backgroundImage = gradientTexture;

            hueSliderTexture = new Texture2D(1, 64, TextureFormat.RGB24, false) { filterMode = FilterMode.Point };
            hueSliderTexture.hideFlags = HideFlags.HideAndDontSave;
            hueSlider.style.backgroundImage = hueSliderTexture;
            UpdateHueSliderTexture();
        }

        private void UpdateHueSliderTexture() {
            if (hueSliderTexture == null)
                return;
            for (var i = 0; i < hueSliderTexture.height; i++) {
                hueSliderTexture.SetPixel(0, i, Color.HSVToRGB((float)i / (hueSliderTexture.height - 1), 1f, 1f));
            }

            hueSliderTexture.Apply();
            hueSlider.MarkDirtyRepaint();
        }

        private void UpdateGradientTexture() {
            if (gradientTexture == null)
                return;
            var pixels = new Color[gradientTexture.width * gradientTexture.height];

            for (var x = 0; x < gradientTexture.width; x++) {
                for (var y = 0; y < gradientTexture.height; y++) {
                    pixels[x * gradientTexture.width + y] = Color.HSVToRGB(H, (float)y / gradientTexture.height, (float)x / gradientTexture.width);
                }
            }

            gradientTexture.SetPixels(pixels);
            gradientTexture.Apply();
            gradientSlider.MarkDirtyRepaint();
        }

        private static float Round(float value, int digits) {
            float mult = Mathf.Pow(10.0f, digits);
            return Mathf.Round(value * mult) / mult;
        }

        // ============================================================================================================
    }
}