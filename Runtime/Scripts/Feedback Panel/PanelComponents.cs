using UnityEngine.UIElements;

public class PanelComponents {
    public UIDocument uiDocument;
    public VisualElement root;

    // Tab Buttons
    public VisualElement searchTab;
    public VisualElement reportTab;
    public Button searchBtn;
    public Button reportBtn;

    // Login Section
    public Button loginBtn;
    public Button logoutBtn;

    // Search Section
    public TextField searchTxtFld;
    public Button searchSubmitBtn;
    public VisualElement tagContainer;
    public ScrollView taskContainer;

    // Report Section
    public DropdownField taskTypeDrpDwn;
    public DropdownField taskMentionsDrpDwn;
    public TextField taskTitleTxt;
    public TextField taskDescriptionTxt;
    public Button taskSubmitBtn;

    // Screenshot Section
    public VisualElement imageContainer;
    public VisualElement screenshotContainer;
    public VisualElement overpaintContainer;

    public void Initialize(UIDocument uiDocument) {
        this.uiDocument = uiDocument;
        
        root = uiDocument.rootVisualElement;

        searchTab = uiDocument.rootVisualElement.Q("SearchTab");
        reportTab = uiDocument.rootVisualElement.Q("ReportTab");
        searchBtn = uiDocument.rootVisualElement.Q("searchBtn") as Button;
        reportBtn = uiDocument.rootVisualElement.Q("reportBtn") as Button;

        loginBtn = uiDocument.rootVisualElement.Q("loginBtn") as Button;

        searchTxtFld = uiDocument.rootVisualElement.Q("searchTxt") as TextField;
        searchSubmitBtn = uiDocument.rootVisualElement.Q("searchSubmitBtn") as Button;
        tagContainer = uiDocument.rootVisualElement.Q("tagContainer");
        taskContainer = uiDocument.rootVisualElement.Q("taskContainer") as ScrollView;

        taskTypeDrpDwn = uiDocument.rootVisualElement.Q("taskTypeDrpDwn") as DropdownField;
        taskMentionsDrpDwn = uiDocument.rootVisualElement.Q("taskMentionsDrpDwn") as DropdownField;
        taskTitleTxt = uiDocument.rootVisualElement.Q("taskTitleTxt") as TextField;
        taskDescriptionTxt = uiDocument.rootVisualElement.Q("taskDescriptionTxt") as TextField;
        taskSubmitBtn = uiDocument.rootVisualElement.Q("taskSubmitBtn") as Button;

        imageContainer = uiDocument.rootVisualElement.Q("imageContainer");
        screenshotContainer = uiDocument.rootVisualElement.Q("screenshotContainer");
        overpaintContainer = uiDocument.rootVisualElement.Q("overpaintContainer");
    }
}
