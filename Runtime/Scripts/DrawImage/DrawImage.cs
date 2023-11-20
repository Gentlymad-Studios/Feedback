using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

//DarkEdge Issue
//https://forum.unity.com/threads/paintable-rendertexture-premultiplied-alpha-problems.1072268/
//https://forum.unity.com/threads/dark-edge-around-textures-drawn-into-a-rendertexture.516545/

namespace Feedback {
    public class DrawImage {
        private PanelComponents panelComponents;

        //Pen Settings
        public int drawWidth = 12;
        public Color drawColor = Color.black;
        public float[,] drawKernel;
        public float sigma = 1; //describes the pen fallof / hardness
        public Texture2D drawSurfaceTexture;
        private RenderTexture drawRenderTexture;
        private Texture2D brushTexture;
        private Texture2D brushIndicator;

        private int minDrawWidth = 4;
        private int maxDrawWidth = 100;
        private int drawWidthSteprate = 2;

        private Color tempDrawColor = Color.black;
        private Color pickedColor = Color.black;

        private float drawSurfaceWidth;
        private float drawSurfaceHeight;

        private Vector2 localPointerPosition;

        // see drawable
        Vector2 previous_drag_position;
        bool color_changed = true;
        bool rtDirty = false;
        bool rtReload = true;
        bool draw = false;
        bool firstDraw = false;
        bool drawIndicator = false;
        bool interuptClick = false;

        Color32[] resetColorArray;
        Color32[] last_colors;
        Color32[] cur_colors;

        float[] max_opacity;
        bool isEraser = false;
        public bool drawingCanBeDestroyed = false;

        public void Dispose() {
            UnregisterEvents();
            panelComponents.overpaintContainer.style.backgroundImage = null;
            drawRenderTexture?.Release();
            if (drawingCanBeDestroyed) {
                UnityEngine.Object.Destroy(drawSurfaceTexture);
                drawingCanBeDestroyed = false;
            }
        }

        public void Setup(PanelComponents panelComponents, Texture2D screenshot) {
            this.panelComponents = panelComponents;

            RegisterEvents();

            ToolbarSetup();

            //for correct size, multiply width canvas scale, use screensize or use screenshot size
            drawSurfaceWidth = screenshot.width;
            drawSurfaceHeight = screenshot.height;

            bool recreateDrawSurfaceTexture = false;
            if (drawSurfaceTexture == null) {
                recreateDrawSurfaceTexture = true;
            } else if (drawSurfaceWidth != drawSurfaceTexture.width || drawSurfaceHeight != drawSurfaceTexture.height) {
                recreateDrawSurfaceTexture = true;
            }

            if (recreateDrawSurfaceTexture) {
                drawSurfaceTexture = new Texture2D((int)drawSurfaceWidth, (int)drawSurfaceHeight);
                drawSurfaceTexture.name = "DrawSurfaceTex";
                drawSurfaceTexture.hideFlags = HideFlags.HideAndDontSave;
                drawSurfaceTexture.filterMode = FilterMode.Point; //prevent grey outlines for now

                // Reset all pixels color to transparent
                Color32 resetColor = new Color32(0, 0, 0, 0);
                resetColorArray = drawSurfaceTexture.GetPixels32();

                max_opacity = new float[resetColorArray.Length];
                ResetMaxOpacity();

                for (int i = 0; i < resetColorArray.Length; i++) {
                    resetColorArray[i] = resetColor;
                }

                drawSurfaceTexture.SetPixels32(resetColorArray);
                drawSurfaceTexture.Apply();
            }

            //Reset Brush
            panelComponents.brushBtn.AddToClassList("active");
            panelComponents.eraseBtn.RemoveFromClassList("active");
            isEraser = false;
            SetPenColor(pickedColor);

            panelComponents.overpaintContainer.style.backgroundImage = drawSurfaceTexture;
        }

        public void OnGUI() {
            if (Event.current.type.Equals(EventType.Repaint)) {
                if (drawIndicator) {
                    DrawIndicator(Mouse.current.position.value);
                }

                if ((draw || firstDraw) && !isEraser) {
                    if (previous_drag_position == Vector2.zero) {
                        // If this is the first time we've ever dragged on this image, simply colour the pixels at our mouse position
                        DrawTexture(localPointerPosition);
                    } else if (previous_drag_position == localPointerPosition) {
                        // just testing for now -> prevents draw on the same spot
                    } else {
                        // Colour in a line from where we were on the last update call
                        Vector2 start_point = previous_drag_position;
                        Vector2 end_point = localPointerPosition;

                        // Get the distance from start to finish
                        float distance = Vector2.Distance(start_point, end_point);
                        int lerp_steps = Mathf.CeilToInt(distance / (drawWidth / 4));

                        // Calculate how many times we should interpolate between start_point and end_point based on the amount of time that has passed since the last update
                        Vector2[] positions = new Vector2[lerp_steps];
                        for (int i = 0; i < lerp_steps; i++) {
                            float lerp = i / (float)Mathf.Max(1, lerp_steps - 1);
                            positions[i] = Vector2.Lerp(start_point, end_point, lerp);
                        }

                        for (int i = 0; i < positions.Length; i++) {
                            DrawTexture(positions[i]);
                        }
                    }

                    previous_drag_position = localPointerPosition;

                    if (firstDraw && !draw) {
                        RtIsDirty();
                    }

                    firstDraw = false;
                }
            }
        }

        private void RegisterEvents() {
            UnregisterEvents();
            panelComponents.overpaintContainer.RegisterCallback<PointerMoveEvent>(OnPointerMoveEvent, TrickleDown.TrickleDown);
            panelComponents.overpaintContainer.RegisterCallback<PointerLeaveEvent>(OnPointerLeaveEvent, TrickleDown.TrickleDown);
            panelComponents.overpaintContainer.RegisterCallback<ClickEvent>(OnClickEvent, TrickleDown.TrickleDown);
            panelComponents.overpaintContainer.RegisterCallback<WheelEvent>(OnWheelEvent, TrickleDown.TrickleDown);
        }

        private void UnregisterEvents() {
            panelComponents.overpaintContainer.UnregisterCallback<PointerMoveEvent>(OnPointerMoveEvent, TrickleDown.TrickleDown);
            panelComponents.overpaintContainer.UnregisterCallback<PointerLeaveEvent>(OnPointerLeaveEvent, TrickleDown.TrickleDown);
            panelComponents.overpaintContainer.UnregisterCallback<ClickEvent>(OnClickEvent, TrickleDown.TrickleDown);
            panelComponents.overpaintContainer.UnregisterCallback<WheelEvent>(OnWheelEvent, TrickleDown.TrickleDown);
        }

        private void OnClickEvent(ClickEvent evt) {
            if (!interuptClick) {
                localPointerPosition = CalculatePointerPosition(evt.localPosition);
                firstDraw = true;
                ReloadRt();
            }

            interuptClick = false;
        }

        private void OnPointerMoveEvent(PointerMoveEvent evt) {
            drawIndicator = true;

            if (Mouse.current.leftButton.isPressed) {
                localPointerPosition = CalculatePointerPosition(evt.localPosition);
                draw = true;
                interuptClick = true;

                if (isEraser) {
                    PenBrush(localPointerPosition);
                    color_changed = true;
                } else {
                    ReloadRt();
                }
            } else {
                draw = false;
                previous_drag_position = Vector2.zero;

                if (color_changed) {
                    last_colors = drawSurfaceTexture.GetPixels32();
                    color_changed = false;
                    ResetMaxOpacity();
                }

                RtIsDirty();
            }
        }

        private void OnPointerLeaveEvent(PointerLeaveEvent evt) {
            drawIndicator = false;

            // We're not over our destination texture
            previous_drag_position = Vector2.zero;
            draw = false;

            if (rtDirty) {
                rtReload = true;
                rtDirty = false;
                RtToTex();
                panelComponents.overpaintContainer.style.backgroundImage = drawSurfaceTexture;
            }
        }

        private void OnWheelEvent(WheelEvent evt) {
            if (evt.delta.y < 0) {
                SetPenSize(drawWidth + drawWidthSteprate);
            } else if (evt.delta.y > 0) {
                SetPenSize(drawWidth - drawWidthSteprate);
            }
        }

        private void ToolbarSetup() {
            panelComponents.brushBtn.clicked += (() => {
                panelComponents.brushBtn.AddToClassList("active");
                panelComponents.eraseBtn.RemoveFromClassList("active");
                isEraser = false;
                SetPenColor(pickedColor);
            });

            panelComponents.eraseBtn.clicked += (() => {
                panelComponents.eraseBtn.AddToClassList("active");
                panelComponents.brushBtn.RemoveFromClassList("active");
                isEraser = true;
                SetPenColor(Color.clear);
            });

            panelComponents.colorField.RegisterValueChangedCallback((evt) => {
                pickedColor = evt.newValue;
                SetPenColor(pickedColor);
            });

            panelComponents.clearBtn.clicked += (() => {
                ResetCanvas();
            });

            panelComponents.brushSizeDownBtn.clicked += (() => {
                SetPenSize(drawWidth - drawWidthSteprate);
            });

            panelComponents.brushSizeUpBtn.clicked += (() => {
                SetPenSize(drawWidth + drawWidthSteprate);
            });

            // call it to calculate the first kernel
            SetPenSize(drawWidth);
        }

        #region Pen Settings
        private void SetPenColor(Color color) {
            drawColor = color;
            tempDrawColor = color;
            BuildBrushTexture();
        }

        private void SetPenSize(int size) {
            drawWidth = Mathf.Max(minDrawWidth, Mathf.Min(maxDrawWidth, size));
            CalculatePenKernel();
            BuildBrushTexture();
            CalculateIndicator();
        }

        private void CalculatePenKernel() {
            try {
                int kernelSize = drawWidth * 2 + 1;
                float kernelMid = 0;
                drawKernel = new float[kernelSize, kernelSize];
                // Fit sigma to kernel size -> rule of thumb -> half kernelsize = 3*sigma
                sigma = (drawWidth + 1) / 3.0f;

                // calculate each kernel field
                for (int x = -drawWidth, xIndex = 0; x <= drawWidth; x++, xIndex++) {
                    for (int y = -drawWidth, yIndex = 0; y <= drawWidth; y++, yIndex++) {
                        drawKernel[xIndex, yIndex] = (float)(1 / (2 * Mathf.PI * (sigma * sigma))) * (float)Mathf.Exp(-((x * x + y * y) / (2 * (sigma * sigma))));

                        //     (1/B) * 1/(1+exp(A*(x-B))) * 1/(1+exp(-A*(x+B)))
                    }
                }

                // extract kernel mid value
                kernelMid = drawKernel[(kernelSize - 1) / 2, (kernelSize - 1) / 2];

                // remap kernel to 0-1
                for (int x = 0; x < kernelSize; x++) {
                    for (int y = 0; y < kernelSize; y++) {
                        drawKernel[x, y] *= 1 / kernelMid;
                    }
                }
            } catch (Exception e) {
                Debug.LogException(e);
            }
        }

        private void BuildBrushTexture() {
            int kernelSize = drawWidth * 2 + 1;

            if (brushTexture == null || brushTexture.width != kernelSize) {
                brushTexture = new Texture2D(kernelSize, kernelSize, TextureFormat.ARGB32, false);
            }
            Color32[] brushPixel = brushTexture.GetPixels32();
            int index = 0;

            for (int x = 0; x < kernelSize; x++) {
                for (int y = 0; y < kernelSize; y++) {
                    Color color = drawColor;
                    color.a *= drawKernel[x, y];
                    brushPixel[index] = color;
                    index++;
                }
            }

            brushTexture.SetPixels32(brushPixel);
            brushTexture.Apply();
        }
        #endregion

        private Vector2 CalculatePointerPosition(Vector2 pos) {
            pos.x = pos.x / panelComponents.overpaintContainer.layout.width * drawSurfaceWidth;
            pos.y = Math.Abs(pos.y / panelComponents.overpaintContainer.layout.height * drawSurfaceHeight - drawSurfaceHeight);

            return pos;
        }

        /// <summary>
        /// Draw to RenderTexture
        /// </summary>
        /// <param name="pos"></param>
        private void DrawTexture(Vector2 pos) {
            RenderTexture.active = drawRenderTexture;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, drawSurfaceWidth, drawSurfaceHeight, 0);
            Graphics.DrawTexture(new Rect(pos.x - brushTexture.width * 0.5f, (drawRenderTexture.height - pos.y) - brushTexture.height * 0.5f, brushTexture.width, brushTexture.height), brushTexture);
            GL.PopMatrix();
            RenderTexture.active = null;
        }

        /// <summary>
        /// Draw the Brush Indicator
        /// </summary>
        /// <param name="pos"></param>
        private void DrawIndicator(Vector2 pos) {
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, drawSurfaceWidth, drawSurfaceHeight, 0);
            Graphics.DrawTexture(new Rect(pos.x - brushIndicator.width * 0.5f, (drawSurfaceHeight - pos.y) - brushIndicator.height * 0.5f, brushIndicator.width, brushIndicator.height), brushIndicator);
            GL.PopMatrix();
        }

        #region Old Draw procedure (used for eraser)
        /// <summary>
        /// Old Draw (used for eraser)
        /// </summary>
        /// <param name="pos"></param>
        private void PenBrush(Vector2 pos) {
            cur_colors = drawSurfaceTexture.GetPixels32();

            if (previous_drag_position == Vector2.zero) {
                // If this is the first time we've ever dragged on this image, simply colour the pixels at our mouse position
                MarkPixelsToColour(pos, drawWidth, drawColor);
            } else if (previous_drag_position == pos) {
                // just testing for now -> prevents draw on the same spot
            } else {
                // Colour in a line from where we were on the last update call
                ColourBetween(previous_drag_position, pos, drawWidth, drawColor);
            }
            ApplyMarkedPixelChanges();

            previous_drag_position = pos;
        }

        // Set the colour of pixels in a straight line from start_point all the way to end_point, to ensure everything inbetween is coloured
        private void ColourBetween(Vector2 start_point, Vector2 end_point, int width, Color color) {
            // Get the distance from start to finish
            float distance = Vector2.Distance(start_point, end_point);
            int lerp_steps = Mathf.CeilToInt(distance / (drawWidth / 4));

            // Calculate how many times we should interpolate between start_point and end_point based on the amount of time that has passed since the last update
            Vector2[] positions = new Vector2[lerp_steps];
            for (int i = 0; i < lerp_steps; i++) {
                float lerp = i / (float)(lerp_steps - 1);
                positions[i] = Vector2.Lerp(start_point, end_point, lerp);
            }

            foreach (Vector2 cur_position in positions) {
                MarkPixelsToColour(cur_position, width, color);
            }
        }

        private void MarkPixelsToColour(Vector2 center_pixel, int pen_thickness, Color color_of_pen) {
            // Figure out how many pixels we need to colour in each direction (x and y)
            int center_x = (int)center_pixel.x;
            int center_y = (int)center_pixel.y;

            for (int x = center_x - pen_thickness, i = 0; x <= center_x + pen_thickness; x++, i++) {
                // Check if the X wraps around the image, so we don't draw pixels on the other side of the image
                if (x >= (int)drawSurfaceWidth || x < 0)
                    continue;

                for (int y = center_y - pen_thickness, j = 0; y <= center_y + pen_thickness; y++, j++) {
                    if (isEraser) {
                        tempDrawColor.a = (drawKernel[i, j] - 1f) * -1f; // invert Kernel for eraser
                    } else {
                        tempDrawColor.a = color_of_pen.a * drawKernel[i, j];
                    }
                    MarkPixelToChange(x, y, tempDrawColor);
                }
            }
        }

        private void MarkPixelToChange(int x, int y, Color color) {
            // Need to transform x and y coordinates to flat coordinates of array
            int array_pos = y * (int)drawSurfaceWidth + x;

            // Check if this is a valid position
            if (array_pos > cur_colors.Length || array_pos < 0 || array_pos > last_colors.Length || array_pos > cur_colors.Length) {
                return;
            }

            if (isEraser) {
                cur_colors[array_pos].a = (byte)(cur_colors[array_pos].a * color.a);
            } else {
                Color col = last_colors[array_pos];
                Color targetColor = color;
                if (color.a > max_opacity[array_pos]) {
                    col.r = Mathf.Lerp(col.r, targetColor.r, Mathf.Max(color.a, 1.0f - col.a));
                    col.g = Mathf.Lerp(col.g, targetColor.g, Mathf.Max(color.a, 1.0f - col.a));
                    col.b = Mathf.Lerp(col.b, targetColor.b, Mathf.Max(color.a, 1.0f - col.a));
                    col.a = Mathf.Lerp(col.a, 1, color.a);
                    max_opacity[array_pos] = color.a;
                    cur_colors[array_pos] = col;
                }
            }
        }

        //Apply changed Pixels to Texture
        private void ApplyMarkedPixelChanges() {
            drawSurfaceTexture.SetPixels32(cur_colors);
            drawSurfaceTexture.Apply();
        }
        #endregion

        // Changes every pixel to be the reset colour
        private void ResetCanvas() {
            drawSurfaceTexture.SetPixels32(resetColorArray);
            drawSurfaceTexture.Apply();
            color_changed = true;
        }

        //Clear the max_opacity Array
        private void ResetMaxOpacity() {
            for (int i = 0; i < max_opacity.Length; i++) {
                max_opacity[i] = 0;
            }
        }

        private void CalculateIndicator() {
            int fullSize = Mathf.FloorToInt(drawWidth * 1.5f);
            int center = fullSize / 2;
            int circleRadius = center - 2;
            int outlineWidth = 1;

            brushIndicator = new Texture2D(fullSize, fullSize);

            Color32[] colors = new Color32[fullSize * fullSize];

            for (int x = 0; x < fullSize; x++) {
                for (int y = 0; y < fullSize; y++) {
                    float distance = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
                    int distanceToOutline = Mathf.Abs(Mathf.FloorToInt(distance) - circleRadius);

                    if (distanceToOutline <= outlineWidth) {
                        colors[y * fullSize + x] = Color.white;
                    } else {
                        colors[y * fullSize + x] = Color.clear;
                    }
                }
            }

            brushIndicator.SetPixels32(colors);
            brushIndicator.Apply();
        }

        private void RtIsDirty() {
            if (rtDirty) {
                rtReload = true;
                rtDirty = false;
                RtToTex();
                panelComponents.overpaintContainer.style.backgroundImage = drawSurfaceTexture;
            }
        }

        private void ReloadRt() {
            if (rtReload) {
                rtReload = false;
                drawRenderTexture = new RenderTexture((int)drawSurfaceWidth, (int)drawSurfaceHeight, 0, RenderTextureFormat.ARGBFloat);
                drawRenderTexture.filterMode = FilterMode.Point;
                TexToRt();
            }
            rtDirty = true;
            panelComponents.overpaintContainer.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(drawRenderTexture));
        }

        private void RtToTex() {
            drawSurfaceTexture = new Texture2D(drawRenderTexture.width, drawRenderTexture.height);
            RenderTexture.active = drawRenderTexture;
            drawSurfaceTexture.ReadPixels(new Rect(0, 0, drawRenderTexture.width, drawRenderTexture.height), 0, 0);
            drawSurfaceTexture.Apply();
            RenderTexture.active = null;
            drawRenderTexture.Release();
        }

        private void TexToRt() {
            RenderTexture.active = drawRenderTexture;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, drawSurfaceWidth, drawSurfaceHeight, 0);
            Graphics.DrawTexture(new Rect(0, 0, drawSurfaceWidth, drawSurfaceHeight), drawSurfaceTexture);
            GL.PopMatrix();
            RenderTexture.active = null;
        }
    }
}