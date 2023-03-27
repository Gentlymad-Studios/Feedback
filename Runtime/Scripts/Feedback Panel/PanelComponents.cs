using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelComponents : MonoBehaviour {

    [Header("Login Information")]
    public GameObject userInfoPanel;
    public TMP_Text userName;
    public TMP_Text userMail;

    [Header("Data")]
    public TMP_Text title;
    public TMP_Text text;
    public TMP_Dropdown dropdown;

    [Header("Screenshot")]
    public RawImage screenshot;
    public RawImage overpaint;

    [Header("Token exchange")]
    public GameObject tokenPanel;
    public TMP_InputField tokenText;
    public Button tokenSubmitButton;

    [Header("Tab Buttons")]
    public Button searchTabButton;
    public Button reportTabButton;
    public GameObject reportPanel;
    public GameObject searchPanel;
}
