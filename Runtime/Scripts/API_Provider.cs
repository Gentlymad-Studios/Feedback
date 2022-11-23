using TMPro;
using UnityEngine;

public class API_Provider : MonoBehaviour {

    [Header("Panel Components")]
    public TMP_Text title;
    public TMP_Text text;
    public TMP_Dropdown dropdown;

    [Header("Settings")]
    public GeneralSettings generalSettings;
    public API_Type type;

    private APISettingsHandler _apiSettingsHandler;
    private Base_API _apiBase;
    private DataType _currentDataType = DataType.feedback;

    private void Start() {

        dropdown.onValueChanged.AddListener(SetDataType);
        _apiSettingsHandler = new APISettingsHandler();

        //To build your API settings call BuildAPISettings method:
        //apiSettingsHandler.BuildAPISettings(your api key, your base url, api type);

        APISettings settings = _apiSettingsHandler
            .LoadAPISettingsAtPath(generalSettings.settingsPath + 
            _apiSettingsHandler.SO_PREFIX + type.ToString() + 
            _apiSettingsHandler.SO_SUFFIX);

        if(type is API_Type.asana) {
            _apiBase = new AsanaAPI(settings as AsanaAPISettings);
        }
    }

    public void SetDataType(int i) {
        _currentDataType = (DataType)i+1;
    }

    public void Submit() {
        API_Data data = new API_Data(title.text, text.text, _currentDataType);
        _apiBase.requestHandler.POST(data);
        Debug.Log("Submit");
    }

    
}
