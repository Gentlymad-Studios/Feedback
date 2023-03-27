using System;
using UnityEngine;
using UnityEngine.UI;

public class TagPreview : MonoBehaviour{
    public ScriptableTag scriptableTag;
    public Button deletButton;
    public Action<ScriptableTag> deleteAction;

    private void Start() {
        deletButton.onClick.AddListener(() => {
            deleteAction.Invoke(scriptableTag);
            Destroy(gameObject); 
        });
    }
}
