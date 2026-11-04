using UnityEngine;
using System.Collections.Generic;




[CreateAssetMenu(fileName = "UnitTemplateListSO", menuName = "Scriptable Objects/UnitTemplateListSO")]
public class UnitTemplateListSO : ScriptableObject
{
    public List<TemplateDataSO> templateDataSOList;   
}
