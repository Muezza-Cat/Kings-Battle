using UnityEngine;

[CreateAssetMenu(fileName = "UnitTemplateSO", menuName = "Scriptable Objects/UnitTemplateSO")]
public class UnitTemplateSO : ScriptableObject
{
    public ObjectPooler.UnitKey unitKey;
    public GameObject unitPrefab;
    public Material unitMaterial;
}
