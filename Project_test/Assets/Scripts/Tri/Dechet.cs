using UnityEngine;

public class Dechet : MonoBehaviour
{
    public enum TypeDechet { Papier, Emballages, Verre, Organique }
    public TypeDechet type;

    [SerializeField] private string dechetId;
    public string DechetId => dechetId;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(dechetId))
            dechetId = System.Guid.NewGuid().ToString();
    }

    [ContextMenu("Regenerate ID")]
    private void RegenerateId()
    {
        dechetId = System.Guid.NewGuid().ToString();
    }
#endif
}
