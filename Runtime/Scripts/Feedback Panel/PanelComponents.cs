using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelComponents : MonoBehaviour {

    [Header("Login Information")]
    public GameObject loginSection;
    public Button loginButton;
    public Button logoutButton;
    public TMP_Text userName;

    [Header("Data")]
    public TMP_InputField title;
    public TMP_InputField text;
    public TMP_Dropdown dataTyepDropdown;
    public TMP_Dropdown mentionList;
    public Button sendButton;

    [Header("Screenshot")]
    public RawImage screenshot;
    public RawImage overpaint;

    [Header("Token exchange")]
    public GameObject submitLoginPanel;
    public Button loginSubmitButton;

    [Header("Tab Buttons")]
    public Button searchTabButton;
    public Button reportTabButton;
    public GameObject reportPanel;
    public GameObject searchPanel;

    [Header("TicketBrowser")]
    public Button createTicketButton;
    public TMP_InputField searchInput;
    public GameObject tabPanel;
    public GameObject scrollviewPreviewLayoutGroup;
    public GameObject ticketPreviewPrefab;
    public GameObject tagPanel;

    [Header("Popup")]
    public GameObject detailPopup;
}
