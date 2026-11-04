using System;
using System.Collections.Generic;
using UnityEngine;


[DefaultExecutionOrder(100)]
public class ObjectPooler : MonoBehaviour
{
    #region Singleton
    public static ObjectPooler Instance { get; private set; }
    #endregion



    [Header("Collection")]
    //public Queue<GameObject> pool;
    public Dictionary<PoolKey, Queue<GameObject>> bigPool;

    [Header("Reference")]
    //Testing
    public GameObject playerUnitPrefab;
    [SerializeField] private GameObject enemyUnit;
    //[SerializeField] private GameObject unit;
    [SerializeField] private GameObject coin;
    [SerializeField] private GameObject apple;

    [Header("Settings")]
    private int numberOfHordes;
    public int maxUnitCapacityInHorde = 25;
    private int numOfEnemyUnitsToPool;
    private int numOfPlayerUnitsToPool;

    [Header("Spawnable")]
    public int totalNumOfCoins = 250;
    public int totalNumOfApples = 40;


    public enum PoolKey
    {
        EnemyUnit,
        PlayerUnit,
        Unit,
        Coin,
        Apple,
    }

    public enum UnitKey
    {
        Man,
        Woman,
        Child,
    }



    public GameObject GetUnitPrefab(UnitKey unitKey)
    {
        foreach (UnitTemplateUI unitTemplateUI in UnitSelectionWindowUI.Instance.GetUnitTemplateList())
        {
            if (unitTemplateUI.GetUnitTemplateSO().unitKey == unitKey)
            {
                Debug.Log(unitTemplateUI.GetUnitTemplateSO().unitPrefab);
                return unitTemplateUI.GetUnitTemplateSO().unitPrefab;
            }
        }
        return null;
    }





    private void Awake()
    {
        Instance = this;

        bigPool = new Dictionary<PoolKey, Queue<GameObject>>();


        bigPool.Add(PoolKey.Unit, new Queue<GameObject>());
        bigPool.Add(PoolKey.Coin, new Queue<GameObject>());
        bigPool.Add(PoolKey.Apple, new Queue<GameObject>());
        bigPool.Add(PoolKey.PlayerUnit, new Queue<GameObject>());
        bigPool.Add(PoolKey.EnemyUnit, new Queue<GameObject>());
        //Queue is Empty till now;

        if (SaveSystem.Instance != null) SaveSystem.Instance.LoadInGameData();

        if (EnemyManager.Instance == null) Debug.Log("EnemyManager's Instance is null");
        if (GameplayManager.Instance == null) Debug.Log("GameplayManager's Instance is null");

        numberOfHordes = GameplayManager.Instance.GetHordeFormation().Count;
        numOfEnemyUnitsToPool = EnemyManager.Instance.GetEnemyHordeCount() * (maxUnitCapacityInHorde + 5); //5 is just extra for extreme case.
        numOfPlayerUnitsToPool = maxUnitCapacityInHorde + 5;

        //EnemyUnit Pooling;
        for (int i = 0; i < numOfEnemyUnitsToPool; i++)
        {
            if (enemyUnit == null)
            {
                Debug.LogWarning("Null reference of EnemyUnitPrefab in ObjectPooler.cs");
                break;
            }
            GameObject obj = Instantiate(enemyUnit, transform.position, Quaternion.identity);
            obj.SetActive(false);

            bigPool[PoolKey.EnemyUnit].Enqueue(obj);
        }

        //PlayerUnit Pooling;
        for (int i = 0; i < numOfPlayerUnitsToPool; i++)
        {
            if (playerUnitPrefab == null)
            {
                Debug.LogWarning("Null reference of PlayerUnitPrefab in ObjectPooler.cs");
                break;
            }
            GameObject obj = Instantiate(playerUnitPrefab, transform.position, Quaternion.identity);
            obj.SetActive(false);

            bigPool[PoolKey.PlayerUnit].Enqueue(obj);
        }

        //Coin ObjectPooling;
        for (int i = 0; i < totalNumOfCoins; i++)
        {
            if (coin == null)
            {
                Debug.LogWarning("Null reference of CoinPrefab in ObjectPooler.cs");
                break;
            }
            GameObject obj = Instantiate(coin, transform.position, Quaternion.identity);
            obj.SetActive(false);

            bigPool[PoolKey.Coin].Enqueue(obj);
        }

        //Apple ObjectPooling;
        for (int i = 0; i < totalNumOfApples; i++)
        {
            if (apple == null)
            {
                Debug.LogWarning("Null reference of ApplePrefab in ObjectPooler.cs");
                break;
            }
            GameObject obj = Instantiate(apple, transform.position, Quaternion.identity);
            obj.SetActive(false);

            bigPool[PoolKey.Apple].Enqueue(obj);
        }



        /*pool = new Queue<GameObject>();
        numberOfHordes = EnemyManager.Instance.hordeFormations.Count;

        numOfObjectsToPool = numberOfHordes * maxUnitCapacityInHorde;

        for (int i = 0; i < numOfObjectsToPool; i++)
        {
            GameObject obj = Instantiate(unit, transform.position, Quaternion.identity);
            obj.SetActive(false);

            pool.Enqueue(obj);
        }*/
        /*for (int i = 0; i < numOfObjectsToPool; i++)
        {
            if (unit == null)
            {
                Debug.LogWarning("Null reference of UnitPrefab in ObjectPooler.cs");
                break;
            }
            GameObject obj = Instantiate(unit, transform.position, Quaternion.identity);
            obj.SetActive(false);

            bigPool[PoolKey.Unit].Enqueue(obj);
        }*/
    }


    /*public GameObject SpawnUnit(Vector3 spawnPosition, Quaternion spawnRotation, HordeFormation hordeFormation)
    {
        GameObject unitToSpawn = bigPool[PoolKey.Unit].Dequeue();

        unitToSpawn.transform.position = spawnPosition;
        unitToSpawn.transform.rotation = spawnRotation;
        
        unitToSpawn.SetActive(true);

        return unitToSpawn;
    }*/






    //Testing
    public GameObject SpawnUnit(Vector3 spawnPosition, Quaternion spawnRotation, HordeFormation hordeFormation)
    {
        GameObject unitToSpawn = bigPool[hordeFormation.GetBaseController().poolKey].Dequeue();

        unitToSpawn.transform.position = spawnPosition;
        unitToSpawn.transform.rotation = spawnRotation;
        
        unitToSpawn.SetActive(true);

        return unitToSpawn;
    }

    public void DespawnUnit(GameObject unitToEnqueue, HordeFormation hordeFormation)
    {
        unitToEnqueue.transform.position = Vector3.zero;
        unitToEnqueue.transform.rotation = Quaternion.identity;

        unitToEnqueue.SetActive(false);

        bigPool[hordeFormation.GetBaseController().poolKey].Enqueue(unitToEnqueue);
    }

    public void SpawnCoin(Vector3 spawnPosition, Quaternion spawnRotation)
    {
        GameObject coinToSpawn = bigPool[PoolKey.Coin].Dequeue();
        coinToSpawn.SetActive(true);

        coinToSpawn.transform.position = spawnPosition;
        coinToSpawn.transform.rotation = spawnRotation;
    }

    public void DespawnCoin(GameObject coinToPool)
    {
        coinToPool.transform.position = Vector3.zero;
        coinToPool.transform.rotation = Quaternion.identity;

        coinToPool.SetActive(false);

        bigPool[PoolKey.Coin].Enqueue(coinToPool);
    }


    //Under Construction
    public void SpawnApple(Vector3 spawnPosition, Quaternion spawnRotation)
    {
        GameObject appleToSpawn = bigPool[PoolKey.Apple].Dequeue();
        appleToSpawn.SetActive(true);

        appleToSpawn.transform.position = spawnPosition;
        appleToSpawn.transform.rotation = spawnRotation;
    }

    public void DespawnApple(GameObject appleToPool)
    {
        appleToPool.transform.position = Vector3.zero;
        appleToPool.transform.rotation = Quaternion.identity;

        appleToPool.SetActive(false);

        bigPool[PoolKey.Apple].Enqueue(appleToPool);
    }



    public void ChangeUnitInPool(GameObject newTypeOfUnit)
    {
        if (newTypeOfUnit == null) 
        {
            Debug.Log("newTypeOfUnitIsNull");
            return;
        }

        while (bigPool[PoolKey.PlayerUnit].Count > 0)
        {
            GameObject obj = bigPool[PoolKey.PlayerUnit].Dequeue();
            if (obj != null)
            {
                Destroy(obj);
            }
        }


        for (int i = 0; i < maxUnitCapacityInHorde + 5; i++)
        {
            GameObject unit = Instantiate(newTypeOfUnit, transform.position, Quaternion.identity);
            unit.SetActive(false);
            bigPool[PoolKey.PlayerUnit].Enqueue(unit);
        }
    }



    [Serializable]
    public class SaveObject
    {
        public UnitKey unitKey; 

    }
    
    public SaveObject Save()
    {
         return new SaveObject() { unitKey = UnitSelector.Instance.selectedUnitKey };
    }

    public void Load(SaveObject saveObject)
    {
        UnitSelector.Instance.selectedUnitKey = saveObject.unitKey;
        playerUnitPrefab = GetUnitPrefab(saveObject.unitKey);
    }
}
