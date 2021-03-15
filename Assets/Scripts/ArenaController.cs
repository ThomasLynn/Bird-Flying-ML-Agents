using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaController : MonoBehaviour
{
    public GameObject agentPrefab;
    public Transform spawnPoint;
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
            GameObject go = Instantiate(agentPrefab, spawnPoint.position, Quaternion.identity, transform) as GameObject;
            //go.GetComponent<DogAgent>().SetRandomTarget(true);
            agents.Add(go);
        }

    }
}