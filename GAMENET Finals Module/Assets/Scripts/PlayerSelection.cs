using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSelection : MonoBehaviour
{
    public GameObject[] SelectablePlayer;

    public int playerSelectionNumber;
    // Start is called before the first frame update
    void Start()
    {
        playerSelectionNumber = 0;

        ActivatePlayer(playerSelectionNumber);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ActivatePlayer(int x)
    {
        foreach(GameObject go in SelectablePlayer)
        {
            go.SetActive(false);
        }

        SelectablePlayer[x].SetActive(true);

        ExitGames.Client.Photon.Hashtable playerSelectionProperties = new ExitGames.Client.Photon.Hashtable() { { Constants.PLAYER_SELECTION_NUMBER, playerSelectionNumber} };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerSelectionProperties);
    }

    public void goToNextPlayer()
    {
        playerSelectionNumber++;

        if(playerSelectionNumber >= SelectablePlayer.Length)
        {
            playerSelectionNumber = 0;
        }

        ActivatePlayer(playerSelectionNumber);
    }

    public void goToPreviousPlayer()
    {
        playerSelectionNumber--;

        if(playerSelectionNumber < 0)
        {
            playerSelectionNumber = SelectablePlayer.Length - 1;
        }

        ActivatePlayer(playerSelectionNumber);
    }
}