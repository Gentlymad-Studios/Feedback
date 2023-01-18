using UnityEngine;
using UnityEngine.UI;

public class AspectRatioFit : MonoBehaviour{

    public Canvas mainCanvas;
    public RectTransform feedbackContainer;
    public RectTransform screenshotPanel;
    public AspectRatioFitter aspectRatioFitter;

    float uiFixedWidth = 0;
    float uiBorder = 0;
    
    public float uiScaleFactor = 0;

    float screenshotPanelWidth = 0;
    float screenshotPanelHeight = 0;


    void OnEnable(){
        FitAspectRatio();
    }

    // Need to be called after the MainCanvas is loaded
    public void FitAspectRatio() {
        // Extract Extra Width, BorderWidth and the ScaleFactor of the MainCanvas
        uiFixedWidth = Mathf.Abs(screenshotPanel.offsetMin.x) + Mathf.Abs(screenshotPanel.offsetMax.x);
        uiBorder = Mathf.Abs(feedbackContainer.offsetMin.x) + Mathf.Abs(feedbackContainer.offsetMax.x);
        uiScaleFactor = mainCanvas.scaleFactor;

        // Calculate the ScreenshotPanel Dimensions
        screenshotPanelWidth = Camera.main.pixelWidth - uiFixedWidth * uiScaleFactor - uiBorder * uiScaleFactor;
        screenshotPanelHeight = screenshotPanelWidth / Camera.main.aspect;

        // Calculate AspectRatio for the whole UI Stack
        aspectRatioFitter.aspectRatio = (screenshotPanelWidth + uiFixedWidth * uiScaleFactor) / screenshotPanelHeight;
    }
}
