using System;
using System.Collections.Generic;
using UnityEngine;


public class HordeFormation : MonoBehaviour
{
    #region Event
    public event Action<int, HordeFormation> OnDisableUpdateEnemyLayer;
    public event Action<HordeFormation> UpdatePlayerLives;
    public event Action<int> UpdateCoinCountUI;
    #endregion



    [Header("Important")]
    public LayerMask enemyLayer;
    public float hordeMorale { get; private set; }
    public int coins { get; set; }
    public int coinsAllTime;
    private BaseController baseController; //Parent
    public Material unitMaterial;


    [Header("Reference")]
    [SerializeField] private Transform lookTransform;

    [Header("Setting")]
    [SerializeField] private int startingUnitsCount = 4;
    public int perUnitCost { get; private set; } = 5;
    public int numberOfColumns { get; private set; } = 5; //X-Axis;
    public int numberOfRows { get; private set; } = 5; //Z-Axis;
    public float unitSpacingX { get; private set; }
    [SerializeField] private float unitSpacingX_Axis = 1f;
    public float unitSpacingZ { get; private set; }
    [SerializeField] private float unitSpacingZ_Axis = 1f;

    [SerializeField, Range(0, 1)] private float formationJitter = 0.3f; //Noise;

    private float halfOfHordeWidth = 2f;
    private float halfOfHordeLength = 2f;

    private float moraleCalculationCooldown = 0.2f;
    private float moraleCalculationElapsedTime = 0f;

    [Header("Collections")]
    private List<Vector3> points;
    private List<Transform> units;
    private List<float> distanceFromPoints;
    private List<Flag> occupiedFlags;

    public float score { get; set; }


    private enum KingLocation
    {
        Front,
        Center
    }
    [SerializeField] KingLocation kingLocation;



    private void Awake()
    {
        points = new List<Vector3>();
        //points.Add(new Vector3(0f, 0f, 0f));

        units = new List<Transform>();
        distanceFromPoints = new List<float>();
        occupiedFlags = new List<Flag>();

        unitSpacingX = unitSpacingX_Axis;
        unitSpacingZ = unitSpacingZ_Axis;

        baseController = GetComponent<BaseController>();
        EvaluatePoints((kingLocation == KingLocation.Center)? true : false);
    }




    private void Start()
    {
        UpdateCoinCountUI?.Invoke(coins);

        AddStartingUnits(points);
    }

    private void Update()
    {
        CalculateHordeMorale();
        AssignUnitPosition();

        UpdateHordeSpeed();
    }

    private void OnDisable()
    {
        OnDisableUpdateEnemyLayer?.Invoke(lookTransform.gameObject.layer, this);
    }


    //Unit position specification;
    private int totalIterationsRequired = 26; //Including the king's position;
    private void EvaluatePoints(bool kingInCenter)
    {
        float zPosMultiplier = (kingInCenter) ? 1f : -1f;
        if (kingInCenter) numberOfRows++;

        for (float z = 1; z <= numberOfRows; z++)
        {
            float zPos;
            zPos = z;

            if (kingInCenter)
            {
                zPos = zPos - 1;
                if (zPos > halfOfHordeLength)
                {
                    zPos = -(zPos - halfOfHordeLength); //0, -1, -2, 1, 2
                }
            }

            for (float x = 0; x < numberOfColumns; x++)
            {
                if (points.Count >= totalIterationsRequired) return;

                float xPos;
                xPos = x;
                if (x > halfOfHordeWidth)
                {
                    xPos = -(x - halfOfHordeWidth); //Order 0, 1 ,2 ,-1 ,-2; for 5 Columns.
                }
                Vector3 position = new Vector3(xPos * unitSpacingX, 0f, zPos * zPosMultiplier * unitSpacingZ);
                if (kingInCenter && position == Vector3.zero) 
                {
                    continue;
                }
                position += GetSlotJitter();
                points.Add(position);

                float distanceFromPoint = Vector3.Distance(position, Vector3.zero);
                distanceFromPoints.Add(distanceFromPoint);
            }
        }
    }

    public void AddStartingUnits(List<Vector3> positions)
    {
        if (positions == null || positions.Count == 0)
        {
            Debug.LogError("There are no positions for units to spawn!");
            return;
        }

        foreach (Vector3 position in positions)
        {
            if (units.Count >= startingUnitsCount) break;

            GameObject unitToSpawn = ObjectPooler.Instance.SpawnUnit(transform.TransformPoint(position), Quaternion.identity, this);

            unitToSpawn.GetComponent<Unit>().targetPosition = transform.TransformPoint(position);
            units.Add(unitToSpawn.transform);

            unitToSpawn.layer = lookTransform.gameObject.layer;
            unitToSpawn.GetComponent<Unit>().Initialize(this);
        }
    }


    private void AssignUnitPosition()
    {
        if (units == null || points == null || points.Count == 0)
        {
            Debug.LogWarning("points is empty or null, please check the List");
            return;
        }

        for (int i = 0; i < units.Count; i++)
        {
            Unit unit = units[i].GetComponent<Unit>();
            unit.targetPosition = transform.TransformPoint(points[i]);
        }
    }


    //Noise
    private Vector3 GetSlotJitter()
    {
        Vector2 offset = UnityEngine.Random.insideUnitCircle * formationJitter;
        return new Vector3(offset.x, 0f, offset.y);
    }

    //Add and Remove Unit;
    public void AddUnitForFree(Transform spawnTransform)
    {
        GameObject unitToAdd = ObjectPooler.Instance.SpawnUnit(spawnTransform.position, Quaternion.identity, this);
        //if (ObjectPooler.Instance == null) Debug.Log("ObjectPooler's Instance is null");

        unitToAdd.layer = lookTransform.gameObject.layer;
        units.Add(unitToAdd.transform);

        Unit unit = unitToAdd.GetComponent<Unit>();
        unit.Initialize(this);
        unit.isDetected = false;
    }


    public bool isNearAFlag = false;
    public void AddUnit()
    {
        if ((GameplayManager.Instance.GetHomeTransform(this).position - transform.position).sqrMagnitude < 10f * 10f)
        {
            if (RemoveCoin(perUnitCost))
            {
                AddUnitForFree(GameplayManager.Instance.GetHomeTransform(this));
                UpdatePlayerLives?.Invoke(this);
                return;
            }
        }

        if (!IsFlagNearby()) return;
        if (units.Count < points.Count && isNearAFlag)
        {
            if (RemoveCoin(perUnitCost))
            {
                if (nearestFlag != null) AddUnitForFree(nearestFlag.transform);
                UpdatePlayerLives?.Invoke(this);
            }
        }
    }


    public void RemoveUnit(Transform unit)
    {
        units.Remove(unit);
        //unit.gameObject.layer = 0;
        ObjectPooler.Instance.DespawnUnit(unit.gameObject, this);
        UpdatePlayerLives?.Invoke(this);
        //if (this.TryGetComponent<PlayerController>(out PlayerController controller)) Debug.Log("Removed Unit : " + units.Count);
    }

    public void ClearUnitsList()
    {
        for (int i = 0; i < startingUnitsCount; i++)
        {
            Transform unitToRemove = units[0];
            units.Remove(unitToRemove);
            Destroy(unitToRemove.gameObject);
        }
        if (units == null) Debug.Log("Units is null");
    }

    public int GetStartingUnitsCount()
    {
        return startingUnitsCount;
    }

    
    private void CalculateHordeMorale()
    {
        moraleCalculationElapsedTime += Time.deltaTime;
        if (moraleCalculationElapsedTime >= moraleCalculationCooldown && units.Count >= 0)
        {
            moraleCalculationElapsedTime = 0f;
            int quotient = Mathf.FloorToInt(units.Count / 10f);

            float additionalMorale = (quotient == 0) ? 0f : quotient * EnemyManager.Instance.unitGroupMorale;
            hordeMorale = (units.Count * EnemyManager.Instance.singleUnitMorale) + additionalMorale;
        }
    }
    public int GetUnitCount()
    {
        return units.Count;
    }

    public Transform GetLookTransform()
    {
        return lookTransform;
    }

    //Add and Remove Flag;
    public void AddFlag(Flag flag)
    {
        occupiedFlags.Add(flag);
    }
    public void RemoveFlag(Flag flag)
    {
        occupiedFlags.Remove(flag);
    }

    //Add and Remove Coin;
    public void AddCoin(int amount)
    {
        coins += amount;
        UpdateCoinCountUI?.Invoke(coins);

        coinsAllTime += amount;
        score += amount / 3f;
    }
    public bool RemoveCoin(int amount)
    {
        bool removedCoin = false;
        if (coins >= amount)
        {
            coins -= amount;
            UpdateCoinCountUI?.Invoke(coins);
            removedCoin = true;
        }
        else
        {
            removedCoin = false;
        }
        return removedCoin;
    }

    public List<Flag> GetOccupiedFlags()
    {
        return occupiedFlags;
    }



    public List<Vector3> GetUnitPositionList()
    {
        return points;
    }

    private Flag nearestFlag;
    public bool IsFlagNearby()
    {
        foreach (Flag flag in occupiedFlags)
        {
            if (flag.isPlayerNearby)
            {
                isNearAFlag = true;
                nearestFlag = flag;
                break;
            }
            else
            {
                isNearAFlag = false;
            }
        }

        return isNearAFlag;
    }

    public BaseController GetBaseController()
    {
        return baseController;
    }


    public int appleCount = 0;
    private bool isOnSpeedup;
    private float speedupMoveSpeed = 6f;

    private float speedupTimeMax = 3f;
    private float speedupElapsedTime = 0f;
    private void UpdateHordeSpeed()
    {
        if (appleCount == 0) return;
        baseController.SetCurrentMoveSpeed(speedupMoveSpeed);
        isOnSpeedup = true;

        speedupElapsedTime += Time.deltaTime;
        if (speedupElapsedTime < speedupTimeMax) return;
        speedupElapsedTime = 0f;

        appleCount--;
        if (appleCount > 0) return;

        baseController.SetCurrentMoveSpeed(baseController.GetNormalMoveSpeed());
        isOnSpeedup = false;
    }

    public bool IsOnSpeedUp()
    {
        return isOnSpeedup;
    }
}