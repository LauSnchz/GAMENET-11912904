using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;


    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.Instantiate(playerPrefab.name, Spawning.instance.spawnPoint[Random.RandomRange(0, 4)].transform.position, Quaternion.identity); // for the instantiate of the photonnetwork, the prefab must be at the resources 
        }
    }

    void Update()
    {
        
    }
}