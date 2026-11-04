using UnityEngine;



[CreateAssetMenu(fileName = "TemplateDataSO", menuName = "Scriptable Objects/TemplateDataSO")]
public class TemplateDataSO : ScriptableObject
{
    public UnitTemplateSO unitTemplateSO;
    public Sprite unitSprite;
    public Color templateBGColor;
    public string UNIT_NAME;
}
