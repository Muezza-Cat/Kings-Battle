using UnityEngine;
using System.Collections.Generic;
using System;


//Testing Phase; Not final;
public class Flag : MonoBehaviour
{
    #region Event
    public event Action<Flag> OnOwnerChanged;
    public event Action<float> OnCaptureProgress;
    #endregion

    [Header("Reference")]
    [SerializeField] private MeshRenderer meshRenderer;


    [Header("Temp Collection")]
    private List<HordeFormation> hordeNearby;

    [Header("Settings")]
    private HordeFormation flagOwner;
    private float flagCaptureRadius = 10f;

    public int scorePerMinute { get; set; } = 30;


    private void Awake()
    {
        hordeNearby = new List<HordeFormation>();
    }



    private void Update()
    {
        DetectHordeAroundFlag();

        SetFlagOwner();
        EnableUnitsAddition();
    }



    //private float hordeDetectionCooldownTime = 0.1f;
    //private float hordeDetectionElapsedTime = 0f;

    //Other solution => Iterate the hordeFormations and find out the closest one, that one is cheap;
    /*private void DetectHordeAroundFlag()
    {
        hordeDetectionElapsedTime += Time.deltaTime;
        if (hordeDetectionElapsedTime < hordeDetectionCooldownTime) return;
        hordeDetectionElapsedTime = 0f;

        if (hordeNearby.Count == GameplayManager.Instance.GetHordeFormation().Count) return;
        hordeNearby.Clear();

        Collider[] unitsNearby = Physics.OverlapSphere(transform.position, flagCaptureRadius);
        foreach (Collider collider in unitsNearby)
        {
            if (!collider.TryGetComponent<Unit>(out Unit unit)) return;
            if (hordeNearby.Count == 0)
            {
                hordeNearby.Add(unit.hordeFormation);
            }
            else
            {
                if (!hordeNearby.Contains(unit.hordeFormation))
                {
                    hordeNearby.Add(unit.hordeFormation);
                }
            }
        }
    }*/

    private void DetectHordeAroundFlag()
    {
        hordeNearby.Clear();
        foreach (HordeFormation hordeFormation in GameplayManager.Instance.GetHordeFormation())
        {
            float sqrDistanceFromFlag = (hordeFormation.transform.position - transform.position).sqrMagnitude;

            if (sqrDistanceFromFlag <= flagCaptureRadius * flagCaptureRadius && !hordeNearby.Contains(hordeFormation))
            {
                hordeNearby.Add(hordeFormation);
            }
        }
    }


    private float flagCaptureTime = 3f;
    private float flagCaptureElapsedTime = 0f;
    private float highestHordeMorale;
    private HordeFormation worthyOwner;
    private List<float> hordeMorales = new List<float>();

    private bool startTimer = false;

    private void SetFlagOwner()
    {
        hordeMorales.Clear();
        if (hordeNearby.Count == 0)
        {
            flagCaptureElapsedTime = 0f;
            OnCaptureProgress?.Invoke(flagCaptureElapsedTime / flagCaptureTime);
            startTimer = false;
            return;
        }
        //No hordeNearby, just ignore;

        highestHordeMorale = hordeNearby[0].hordeMorale;
        if (hordeNearby.Count > 1)
        {
            foreach (HordeFormation hordeFormation in hordeNearby)
            {
                hordeMorales.Add(hordeFormation.hordeMorale);
            }
            hordeMorales.Sort();
            if (hordeMorales[hordeMorales.Count - 1] == hordeMorales[hordeMorales.Count - 2])
            {
                startTimer = false;
                flagCaptureElapsedTime = 0f;
                OnCaptureProgress?.Invoke(flagCaptureElapsedTime / flagCaptureTime);
            } //Case same morale count;
            else startTimer = true;
        }
        //More than one hordeNearby, let the strongest be worthy;
        else
        {
            if (flagOwner != null && hordeNearby[0] == flagOwner)
            {
                flagCaptureElapsedTime = 0f;
                OnCaptureProgress?.Invoke(flagCaptureElapsedTime / flagCaptureTime);
            }
            else
            {
                worthyOwner = hordeNearby[0];
                startTimer = true;
            }
        }
        //Just one hordeNearby, give it to them and just don't work if it's the owner;
        

        if (startTimer)
        {
            flagCaptureElapsedTime += Time.deltaTime;
            OnCaptureProgress?.Invoke(flagCaptureElapsedTime / flagCaptureTime);
            if (flagCaptureElapsedTime < flagCaptureTime) return;

            flagCaptureElapsedTime = 0f;
            if (flagOwner != null) flagOwner.RemoveFlag(this);
            flagOwner = worthyOwner;
            flagOwner.AddFlag(this);
            meshRenderer.materials[1].color = GameplayManager.Instance.GetUnitMaterial(flagOwner).color;

            OnOwnerChanged?.Invoke(this);
        }
    }


    public bool isPlayerNearby = false;
    private float interactableDistance = 10f;
    private void EnableUnitsAddition()
    {
        if (flagOwner == null) return;
        if ((flagOwner.transform.position - transform.position).sqrMagnitude < interactableDistance * interactableDistance)
        {
            isPlayerNearby = true;
        }
        else
        {
            isPlayerNearby = false;
        }
    }

    public HordeFormation GetFlagOwner()
    {
        return flagOwner;
    }
}