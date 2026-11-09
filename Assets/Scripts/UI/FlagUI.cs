using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;


public class FlagUI : MonoBehaviour
{
    [SerializeField] private RectTransform parentRectTransform;
    [SerializeField] private GameObject flagUITemplate;

    [Header("Settings")]
    private float flagUITemplateHeight = 100f;
    private float flagUITemplateWidth = 100f;


    [Serializable]
    private struct Flag_FlagUI
    {
        public Flag flag;
        public RectTransform flagUI;
    }
    private List<Flag_FlagUI> flag_FlagUIList;



    private void Awake()
    {
        flag_FlagUIList = new List<Flag_FlagUI>();
    }

    private void Start()
    {
        parentRectTransform.sizeDelta = new Vector2(flagUITemplateWidth * GameplayManager.Instance.GetFlagList().Count, flagUITemplateHeight);

        //float i = /*GameplayManager.Instance.GetFlagList().Count / 2f*/ 2.5f;
        //int k = 0;

        foreach (Transform flagTransform in GameplayManager.Instance.GetFlagList())
        {
            Flag flag = flagTransform.GetComponent<Flag>();

            RectTransform flagTemplate = Instantiate(flagUITemplate, parentRectTransform).GetComponent<RectTransform>();
            flagTemplate.gameObject.SetActive(true);

            flag.OnOwnerChanged += UpdateFlagUI;
            flag_FlagUIList.Add(new Flag_FlagUI() { flag =  flag , flagUI = flagTemplate});
        }

        /*for (float j = -i; j < i; j++)
        {
            Flag flag = GameplayManager.Instance.GetFlagList()[k].GetComponent<Flag>();
            k++;

            Vector2 flagTemplatePosition = new Vector2(j * flagUITemplateWidth , 0f);

            RectTransform flagTemplate = Instantiate(flagUITemplate, parentRectTransform).GetComponent<RectTransform>();
            flagTemplate.gameObject.SetActive(true);
            flagTemplate.localPosition = flagTemplatePosition;

            flag_FlagUIList.Add(new Flag_FlagUI() { flag =  flag , flagUI = flagTemplate});
        }*/


        //Event subscriptions
    }

    private void UpdateFlagUI(Flag flag)
    {
        foreach (Flag_FlagUI flag_FlagUI in flag_FlagUIList)
        {
            if (flag_FlagUI.flag != flag) continue;

            if (flag.GetFlagOwner() != null) flag_FlagUI.flagUI.GetComponentInChildren<Image>().color = GameplayManager.Instance.GetUnitMaterial(flag.GetFlagOwner()).color;
        }
    }

}
