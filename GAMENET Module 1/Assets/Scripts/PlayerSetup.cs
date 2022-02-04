﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject camera;

    [SerializeField]
    TextMeshProUGUI playerNameText;

    // Start is called before the first frame update
    void Start()
    {
        // Lets you control your own player
        if (photonView.IsMine)
        {
            transform.GetComponent<MovementController>().enabled = true;
            camera.GetComponent<Camera>().enabled = true;
        }
        else
        {
            // avoids controlling other players
            transform.GetComponent<MovementController>().enabled = false;
            camera.GetComponent<Camera>().enabled = false;
        }

        playerNameText.text = photonView.Owner.NickName;
    }

}
