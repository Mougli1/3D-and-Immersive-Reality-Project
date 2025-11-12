using UnityEngine;

[CreateAssetMenu(menuName = "Heartwood/Dialogue Asset")]
public class DialogueAsset : ScriptableObject {
    public string id = "intro";
    [TextArea(2,4)] public string[] lines;
}
