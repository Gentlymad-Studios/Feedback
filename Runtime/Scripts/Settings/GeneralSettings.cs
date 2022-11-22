using UnityEngine;

[CreateAssetMenu(fileName = nameof(GeneralSettings), menuName = nameof(GeneralSettings), order = 1)]
public class GeneralSettings : ScriptableObject {
    public string settingsPath;
}
