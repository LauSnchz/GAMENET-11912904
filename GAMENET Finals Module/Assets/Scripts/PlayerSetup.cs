using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    public Camera camera;

    [SerializeField]
    TextMeshProUGUI playerNameText;

    // Start is called before the first frame update
    void Start()
    {
        if(photonView.IsMine)
        {
            transform.GetComponent<PlayerMovement>().enabled = true;
            camera.enabled = photonView.IsMine;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            transform.GetComponent<PlayerMovement>().enabled = false;
            camera.enabled = false;
        }

        playerNameText.text = photonView.Owner.NickName;
    }
}