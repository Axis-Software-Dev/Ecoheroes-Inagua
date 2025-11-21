using System.Collections;
using UnityEngine;

public class carManager : MonoBehaviour
{
    public GameObject[] carModelStraight;
    public GameObject[] carModelRight;
    
    private Transform[] spawnerStraight;
    private Transform[] spawnerRight;
    
    [HideInInspector]
    public float timer = 0;
    [HideInInspector]
    public float timeToSpawn = 1f;
    
    public float minSpeed = 0.5f;
    public float maxSpeed = 1.5f;
    public float minTimeToSpawn = 1f;
    public float maxTimeToSpawn = 5f;
    public float timeToDestroy = 10f;

    private int lastCarSpawnedRight;
    private int lastCarSpawnedStraight;
    private bool timerActive = false;
    private CarPool pool;

    private const string CAR_SPAWNER_STRAIGHT_TAG = "CarSpawnerStraight";
    private const string CAR_SPAWNER_RIGHT_TAG = "CarSpawnerRight";
    private const float RANDOM_INIT_RANGE = 1f;
    private const int MIN_RANDOM_INDEX = 0;
    private const int ARRAY_ADJUST_OFFSET = 1;

    private void Awake()
    {
        InitializeSpawners();
        InitializeCarModels();
    }

    private void InitializeSpawners()
    {
        GameObject[] spawnerObjectStraight = GameObject.FindGameObjectsWithTag(CAR_SPAWNER_STRAIGHT_TAG);
        spawnerStraight = new Transform[spawnerObjectStraight.Length];
        
        for (int i = 0; i < spawnerObjectStraight.Length; i++)
        {
            if (spawnerObjectStraight[i] != null)
            {
                spawnerStraight[i] = spawnerObjectStraight[i].transform;
            }
        }

        GameObject[] spawnerObjectRight = GameObject.FindGameObjectsWithTag(CAR_SPAWNER_RIGHT_TAG);
        spawnerRight = new Transform[spawnerObjectRight.Length];
        
        for (int i = 0; i < spawnerObjectRight.Length; i++)
        {
            if (spawnerObjectRight[i] != null)
            {
                spawnerRight[i] = spawnerObjectRight[i].transform;
            }
        }
    }

    private void InitializeCarModels()
    {
        if (carModelStraight != null)
        {
            foreach (GameObject car in carModelStraight)
            {
                if (car != null)
                {
                    car.SetActive(false);
                }
            }
        }

        if (carModelRight != null)
        {
            foreach (GameObject car in carModelRight)
            {
                if (car != null)
                {
                    car.SetActive(false);
                }
            }
        }
    }

    private void Start()
    {
        pool = gameObject.AddComponent<CarPool>();
    }

    private void Update()
    {
        spawnCars();
    }

    private void spawnCars()
    {
        if (timerActive)
        {
            timer += Time.deltaTime;
        }

        if (timer <= timeToSpawn)
        {
            timerActive = true;
        }
        else
        {
            timeToSpawn = Random.Range(minTimeToSpawn, maxTimeToSpawn);
            float randomInit = Random.Range(MIN_RANDOM_INDEX, RANDOM_INIT_RANGE);
            Invoke(nameof(SpawnCarRight), randomInit);
            randomInit = Random.Range(MIN_RANDOM_INDEX, RANDOM_INIT_RANGE);
            Invoke(nameof(SpawnCarStraight), randomInit);
            resetTimer();
        }
    }

    private IEnumerator DestroyObject(GameObject objectToDestroy)
    {
        yield return new WaitForSeconds(timeToDestroy);
        
        if (objectToDestroy != null)
        {
            Destroy(objectToDestroy);
        }
    }

    private void resetTimer()
    {
        timer = 0;
        timerActive = false;
    }

    private void SpawnCarStraight()
    {
        if (carModelStraight == null || carModelStraight.Length == 0 || spawnerStraight == null || spawnerStraight.Length == 0 || pool == null)
        {
            return;
        }

        int randNum = GetRandomCarIndex(carModelStraight.Length, lastCarSpawnedStraight);
        lastCarSpawnedStraight = randNum;

        int randSpawn = Mathf.FloorToInt(Random.Range(MIN_RANDOM_INDEX, spawnerStraight.Length));
        
        if (randSpawn >= 0 && randSpawn < spawnerStraight.Length && spawnerStraight[randSpawn] != null &&
            randNum >= 0 && randNum < carModelStraight.Length && carModelStraight[randNum] != null)
        {
            GameObject mostRecentObject = pool.Get(
                carModelStraight[randNum],
                spawnerStraight[randSpawn].position,
                spawnerStraight[randSpawn].rotation
            );
            
            if (mostRecentObject != null)
            {
                mostRecentObject.SetActive(true);
                pool.Return(mostRecentObject, timeToDestroy);
            }
        }
    }

    private void SpawnCarRight()
    {
        if (carModelRight == null || carModelRight.Length == 0 || spawnerRight == null || spawnerRight.Length == 0)
        {
            return;
        }

        int randNum = GetRandomCarIndex(carModelRight.Length, lastCarSpawnedRight);
        lastCarSpawnedRight = randNum;

        int randSpawn = Mathf.FloorToInt(Random.Range(MIN_RANDOM_INDEX, spawnerRight.Length));
        
        if (randSpawn >= 0 && randSpawn < spawnerRight.Length && spawnerRight[randSpawn] != null &&
            randNum >= 0 && randNum < carModelRight.Length && carModelRight[randNum] != null)
        {
            GameObject mostRecentObject = Instantiate(
                carModelRight[randNum], 
                spawnerRight[randSpawn].position, 
                spawnerRight[randSpawn].rotation
            );
            
            if (mostRecentObject != null)
            {
                mostRecentObject.SetActive(true);
                StartCoroutine(DestroyObject(mostRecentObject));
            }
        }
    }

    private int GetRandomCarIndex(int arrayLength, int lastIndex)
    {
        if (arrayLength <= 0) return 0;

        int randNum = Mathf.FloorToInt(Random.Range(MIN_RANDOM_INDEX, arrayLength));

        if (randNum == lastIndex)
        {
            int upOrDown = Mathf.FloorToInt(Random.Range(MIN_RANDOM_INDEX, 2));

            if (upOrDown == 1)
            {
                randNum -= 1;
            }
            else
            {
                randNum += 1;
            }

            if (randNum < 0)
            {
                randNum = arrayLength - ARRAY_ADJUST_OFFSET;
            }
            else if (randNum >= arrayLength)
            {
                randNum = 0;
            }
        }

        return randNum;
    }
}
