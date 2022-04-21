using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;


public class RaiseEventScript : MonoBehaviourPunCallbacks
{
    private PlayerDamage playerDamage;

    private const byte PLAYER_WINNER = 0;

    private void Start()
    {
        GetPlayerScore(this.GetComponent<PlayerDamage>().playerScore);
    }

    public void GetPlayerScore(int score)
    {
        string playerName = photonView.Owner.NickName;
        int playerScore = score;
        int viewId = photonView.ViewID;

        object data = new object[] { playerName, playerScore, viewId };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All,
            CachingOption = EventCaching.DoNotCache
        };
        SendOptions sendOption = new SendOptions { Reliability = false };

        if(playerScore >= 3)
        {
            PhotonNetwork.RaiseEvent(PLAYER_WINNER, data, raiseEventOptions, sendOption);
        }
    }

    private void OnEnable() 
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    void OnEvent(EventData photonEvent)
    {
        if(photonEvent.Code == PLAYER_WINNER)
        {
            object[] data = (object[])photonEvent.CustomData;
            string playerName = (string)data[0];
            int playerScore = (int)data[1];
            int viewId = (int)data[2];

            Debug.Log("Winner: " + playerName);

            GameObject[] playerArray = GameObject.FindGameObjectsWithTag("Player");
            GameObject winningText = GameObject.Find("WinningText");

            foreach (GameObject players in playerArray)
            {
                players.GetComponent<PlayerDamage>().enabled = false;
                players.GetComponent<PlayerMovement>().enabled = false;
            }
               
            winningText.GetComponent<Text>().text = playerName + " has won!";

        }
    }
}
