using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class carManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject[] carModelStraight,carModelRight;
    Transform[] spawnerStraight,spawnerRight;
    [HideInInspector]
    public float timer = 0;
    [HideInInspector]
    public float timeToSpawn=1f;

    public float minSpeed = 0.5f, maxSpeed = 1.5f, minTimeToSpawn = 1f, maxTimeToSpawn = 5f, timeToDestroy = 10f;
    bool timerActive = false;
    
    
    private void Awake()
    {
        GameObject[] spawnerObjectStraight = GameObject.FindGameObjectsWithTag("CarSpawnerStraight");
        spawnerStraight= new Transform[spawnerObjectStraight.Length];   
        for (int i=0;i<spawnerObjectStraight.Length; i++) {spawnerStraight[i]=spawnerObjectStraight[i].transform;}
        foreach (GameObject car in carModelStraight) { 
            car.SetActive(false);
        }
        GameObject[] spawnerObjectRight = GameObject.FindGameObjectsWithTag("CarSpawnerRight");
        spawnerRight= new Transform[spawnerObjectRight.Length];   
        for (int i=0;i<spawnerObjectRight.Length; i++) {spawnerRight[i]=spawnerObjectRight[i].transform;}
        foreach (GameObject car in carModelRight) { 
            car.SetActive(false);
        }
    }


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        spawnCars();
    }
    void spawnCars()
    {
        if (timerActive == true) timer += Time.deltaTime;
        if (timer <= timeToSpawn) {
            timerActive = true;

        }
        else
        {
            timeToSpawn = Random.Range(minTimeToSpawn,maxTimeToSpawn);
            float randomInit = Random.Range(0,1f);
            Invoke("spawnCarRight",randomInit);
            randomInit = Random.Range(0, 1f);
            Invoke("spawnCarStraight", randomInit);
            resetTimer();
        }
    }
    IEnumerator destroyObject(GameObject objectToDesrtoy)
    {
        yield return new WaitForSeconds(timeToDestroy);
        Destroy(objectToDesrtoy);
    }
    void resetTimer()
    {
        timer = 0;
    }
    void spawnCarStraight()
    {
        int randNum = Mathf.FloorToInt(Random.Range(0, carModelStraight.Length));
        
        int randSpawn = Mathf.FloorToInt(Random.Range(0, spawnerStraight.Length));
        GameObject mostRecentObject = Instantiate(carModelStraight[randNum], spawnerStraight[randSpawn].position, spawnerStraight[randSpawn].rotation);
        //float randomSpeed = Random.Range(minSpeed, maxSpeed);
        //mostRecentObject.GetComponent<Animator>().speed = randomSpeed;
        mostRecentObject.SetActive(true);
        StartCoroutine(destroyObject(mostRecentObject));
    }
    void spawnCarRight()
    {
        int randNum = Mathf.FloorToInt(Random.Range(0, carModelRight.Length));
        int randSpawn = Mathf.FloorToInt(Random.Range(0, spawnerRight.Length));
        GameObject mostRecentObject = Instantiate(carModelRight[randNum], spawnerRight[randSpawn].position, spawnerRight[randSpawn].rotation);
        //float randomSpeed = Random.Range(minSpeed, maxSpeed);
        //mostRecentObject.GetComponent<Animator>().speed = randomSpeed;
        mostRecentObject.SetActive(true);
        StartCoroutine(destroyObject(mostRecentObject));
    }
}
