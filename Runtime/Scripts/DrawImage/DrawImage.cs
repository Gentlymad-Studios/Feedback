﻿using UnityEngine;
using UnityEngine.UI;

public class DrawImage : MonoBehaviour {
    //Toolbar UI Elements
    public DrawToolbar toolbar;

    //Pen Settings
    public int drawWidth;
    public Color drawColor = Color.black;
    public float[,] drawKernel;
    public  float sigma = 1; //describes the pen fallof / hardness

    public float test = 1;

    private Color tempDrawColor = Color.black;
    private Color pickedColor = Color.red;

    private RectTransform drawSurfaceRectTransform;
    private Texture2D drawSurfaceTexture;
    private float drawSurfaceWidth;
    private float drawSurfaceHeight;

    private Vector2 localPointerPosition;

    // see drawable
    Vector2 previous_drag_position;
    bool mouse_was_previously_held_down = false;
    bool no_drawing_on_current_drag = false;
    bool color_changed = true;
    public delegate void Brush_Function(Vector2 world_position);

    // This is the function called when a left click happens
    // Pass in your own custom one to change the brush type
    // Set the default function in the Awake method
    public Brush_Function current_brush;

    Color32[] resetColorArray;
    Color32[] last_colors;
    Color32[] cur_colors;
    
    float[] max_opacity;
    bool isEraser = false;

    public void Setup() {
        ToolbarSetup();

        //for correct size, multiply width canvas scale, use screensize or use screenshot size
        drawSurfaceRectTransform = this.gameObject.GetComponent<RectTransform>();
        drawSurfaceWidth = drawSurfaceRectTransform.rect.width;
        drawSurfaceHeight = drawSurfaceRectTransform.rect.height;
        drawSurfaceTexture = new Texture2D((int)drawSurfaceWidth, (int)drawSurfaceHeight);
        drawSurfaceTexture.filterMode = FilterMode.Point; //prevent grey outlines for now
        this.gameObject.GetComponent<RawImage>().texture = drawSurfaceTexture;

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

        current_brush = PenBrush;
        isEraser = false;
    }

    void Update() {
        if (Input.GetMouseButton(0)) {
            if (RectTransformUtility.RectangleContainsScreenPoint(drawSurfaceRectTransform, Input.mousePosition, null)) {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(drawSurfaceRectTransform, Input.mousePosition, null, out localPointerPosition);
                current_brush(localPointerPosition);
                color_changed = true;
            } else {
                // We're not over our destination texture
                previous_drag_position = Vector2.zero;
                if (!mouse_was_previously_held_down) {
                    // This is a new drag where the user is left clicking off the canvas
                    // Ensure no drawing happens until a new drag is started
                    no_drawing_on_current_drag = true;
                }
            }
        }
        // Mouse is released
        else if (!Input.GetMouseButton(0)) {
            previous_drag_position = Vector2.zero;
            no_drawing_on_current_drag = false;

            if (color_changed) {
                last_colors = drawSurfaceTexture.GetPixels32();
                color_changed = false;
                ResetMaxOpacity();
            }
        }
        mouse_was_previously_held_down = Input.GetMouseButton(0);
    }

    private void ToolbarSetup() {
        toolbar.penButton.onClick.AddListener(() => {
            isEraser = false;
            SetPenColor(pickedColor);
        });

        toolbar.eraserButton.onClick.AddListener(() => {
            isEraser = true;
            SetPenColor(Color.clear);
        });

        toolbar.chooseColorButton.onClick.AddListener(() => {
            pickedColor = Color.green;
            SetPenColor(pickedColor); 
        });

        toolbar.clearButton.onClick.AddListener(() => {
            ResetCanvas();
        });

        toolbar.shrinkButton.onClick.AddListener(() => {
            SetPenSize(Mathf.Min(drawWidth - 1, 1));
        });

        toolbar.enlargeButton.onClick.AddListener(() => {
            SetPenSize(drawWidth + 1);
        });

        // call it to calculate the first kernel
        SetPenSize(drawWidth);
    }

    #region Pen Settings
    private void SetPenColor(Color color) {
        drawColor = color;
        tempDrawColor = color;
    }

    private void SetPenSize(int size) {
        drawWidth = size;
        CalculatePenKernel(size);
    }

    private void CalculatePenKernel(int size) {
        int kernelSize = size * 2 + 1;
        float kernelMid = 0;
        drawKernel = new float[kernelSize, kernelSize];
        // Fit sigma to kernel size -> rule of thumb -> half kernelsize = 3*sigma
        sigma = (size + 1) / 3.0f;

        // calculate each kernel field
        for (int x = -size, xIndex = 0; x <= size; x++, xIndex++) {
            for (int y = -size, yIndex = 0; y <= size; y++, yIndex++) {
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
    }
    #endregion

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
        Vector2 direction = (start_point - end_point).normalized;

        Vector2 cur_position = start_point;

        // Calculate how many times we should interpolate between start_point and end_point based on the amount of time that has passed since the last update
        float lerp_steps = 1 / distance;

        for (float lerp = 0; lerp <= 1; lerp += lerp_steps) {
            cur_position = Vector2.Lerp(start_point, end_point, lerp);
            MarkPixelsToColour(cur_position, width, color);
        }
    }

    private void MarkPixelsToColour(Vector2 center_pixel, int pen_thickness, Color color_of_pen) {
        // Figure out how many pixels we need to colour in each direction (x and y)
        int center_x = (int)(center_pixel.x + (drawSurfaceWidth / 2));
        int center_y = (int)(center_pixel.y + (drawSurfaceHeight / 2));

        for (int x = center_x - pen_thickness, i = 0; x <= center_x + pen_thickness; x++, i++) {
            // Check if the X wraps around the image, so we don't draw pixels on the other side of the image
            if (x >= (int)drawSurfaceRectTransform.rect.width || x < 0)
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
        int array_pos = y * (int)drawSurfaceRectTransform.rect.width + x;

        // Check if this is a valid position
        if (array_pos > cur_colors.Length || array_pos < 0)
            return;

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

}