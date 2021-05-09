using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaController : MonoBehaviour
{
    public GameObject agentPrefab;
    public List<Transform> spawnPoints;
    public int maxAgents;

    private List<GameObject> agents;

    public void Start()
    {
        agents = new List<GameObject>();
        Spawn();
    }

    public void ResetEnv(GameObject ToDelete)
    {
        Destroy(ToDelete);
        agents.Remove(ToDelete);
        Spawn();
    }

    private void Spawn()
    {
        //Debug.Log(currentDogCount+" "+ number);
        for (int j = agents.Count; j < maxAgents; j++)
        {
            int spawnNumber = Random.Range(0, spawnPoints.Count);
            Quaternion direction = Quaternion.LookRotation(spawnPoints[(spawnNumber + 1) % spawnPoints.Count].position - spawnPoints[spawnNumber].position);
            GameObject go = Instantiate(agentPrefab, spawnPoints[spawnNumber].position, direction, transform) as GameObject;
            go.GetComponent<BirdAgent>().SetTarget(spawnNumber+1);
            //go.GetComponent<DogAgent>().SetRandomTarget(true);
            agents.Add(go);
        }

    }
}