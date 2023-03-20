using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "QA Tags/New QA Tag List")]
public class TagList : ScriptableObject{
    public List<ScriptableTag> tags = new List<ScriptableTag>();    
}
