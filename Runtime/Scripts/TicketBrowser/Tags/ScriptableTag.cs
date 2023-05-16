using Lucene.Net.Documents;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "QA Tags/New QA Tag")]
public class ScriptableTag : ScriptableObject {
    public GameObject tagPrefab;
    public string tagName;
}
