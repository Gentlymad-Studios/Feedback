using TMPro;
using UnityEngine;

public class API_Provider : MonoBehaviour {

    [Header("Panel Components")]
    public TMP_Text title;
    public TMP_Text text;
    public TMP_Text user_name;
    public TMP_Text user_mail;
    public TMP_Dropdown dropdown;
    public TMP_InputField tokenText;

    public GameObject loginTokenPanel;
    public GameObject userInfoPanel;

    [Header("Settings")]
    public GeneralSettings generalSettings;
    public API_Type type;

    private APISettingsHandler _apiSettingsHandler;
    private Base_API _apiBase;
    private DataType _currentDataType = DataType.feedback;
    private AsanaRequestHandler asanaRequestHandler;

    private void Start() {

        dropdown.onValueChanged.AddListener(SetDataType);
        _apiSettingsHandler = new APISettingsHandler();

        //To build your API settings call BuildAPISettings method:
        //apiSettingsHandler.BuildAPISettings(your api key, your base url, api type);

        APISettings settings = _apiSettingsHandler
            .LoadAPISettingsAtPath(generalSettings.settingsPath +
            _apiSettingsHandler.SO_PREFIX + type.ToString() +
            _apiSettingsHandler.SO_SUFFIX);

        if (type is API_Type.asana) {
            _apiBase = new AsanaAPI(settings as AsanaAPISettings);
            asanaRequestHandler = _apiBase.requestHandler as AsanaRequestHandler;
        }
    }

    public void SetDataType(int i) {
        _currentDataType = (DataType)i + 1;
    }

    public void OpenAuthorizationWebsite() {
        asanaRequestHandler.OpenAuthorizationEndpoint();
        loginTokenPanel.SetActive(true);
        userInfoPanel.SetActive(true);
    }
    public void LogIn() {
        _apiBase.settings.token = tokenText.text.ToString();
        asanaRequestHandler.LogIn();
        user_name.text = asanaRequestHandler.GetUserData().name;
        user_mail.text = asanaRequestHandler.GetUserData().email;
        loginTokenPanel.SetActive(false);
    }

    public void LogOut() {
        _apiBase.requestHandler.LogOut();
        user_name.text = "user name";
        user_mail.text = "user mail";
        userInfoPanel.SetActive(false);
    }
    public void Submit() {
        API_Data data = new API_Data(title.text, text.text, _currentDataType);
        _apiBase.requestHandler.POST(data);
    }


}
