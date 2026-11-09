using UnityEngine;



[CreateAssetMenu(fileName = "TemplateDataSO", menuName = "Scriptable Objects/TemplateDataSO")]
public class TemplateDataSO : ScriptableObject
{
    public UnitTemplateSO unitTemplateSO;
    public Sprite unitSprite;
    public Color templateBGColor;
    public string UNIT_NAME;

    public string unit_ID;
    public int adsToWatch = 3;
    public bool isLocked = false;

    [HideInInspector] public const string IS_LOCKED = "isLocked_";
    [HideInInspector] public const string ADS = "ads_";

    //Testing
    public float damage;
    public float hp;
}
