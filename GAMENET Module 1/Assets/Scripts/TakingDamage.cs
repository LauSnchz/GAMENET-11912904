using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class TakingDamage : MonoBehaviourPunCallbacks
{
    [SerializeField]
    Image healthbar;

    private float startingHealth = 100;
    public float currentHealth;
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = startingHealth;
        healthbar.fillAmount = currentHealth / startingHealth;
    }

    [PunRPC] // Remote Procedure Call, if a function is an rpc it is then broadcasted to the whole room
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log(currentHealth);

        healthbar.fillAmount = currentHealth / startingHealth;

        if (currentHealth < 0)
        {
            Die();
        }
    }

    private void Die()
    {
        //determines if the player you are controlling is yours to prevent other players from getting kicked or leaving
        if(photonView.IsMine)
        {
            GameManager.instance.LeaveRoom();
        }
    }

}
