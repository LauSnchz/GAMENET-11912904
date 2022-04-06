using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;

public class Shooting : MonoBehaviourPunCallbacks
{
    public Camera camera;
    public GameObject hitEffectPrefab;

    [Header("Health Related")]
    public float startHealth = 100;
    private float health;
    public Image healthBar;

    private Animator animator;

    void Start()
    {
        health = startHealth;
        healthBar.fillAmount = health / startHealth;
        animator = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Fire()
    {
        RaycastHit hit;
        Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        if(Physics.Raycast(ray, out hit, 200))
        {
            Debug.Log(hit.collider.gameObject.name);

            photonView.RPC("CreateHitEffects", RpcTarget.All, hit.point);

            if(hit.collider.gameObject.CompareTag("Player") && !hit.collider.gameObject.GetComponent<PhotonView>().IsMine)
            {
                hit.collider.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, 25);
            }
        }
    }

    [PunRPC]
    public void TakeDamage(int damage, PhotonMessageInfo info)
    {
        this.health -= damage;
        this.healthBar.fillAmount = health / startHealth;

        if(health == 0)
        {
            info.Sender.AddScore(1);
            StartCoroutine(KillFeed(info));

            Die();
            Debug.Log(info.Sender.NickName + " killed " + info.photonView.Owner.NickName);
        }
    }

    [PunRPC]
    public void CreateHitEffects(Vector3 position)
    {
        GameObject hiteffectGameObject = Instantiate(hitEffectPrefab, position, Quaternion.identity);
        Destroy(hiteffectGameObject, 0.2f);
    }

    public void Die()
    {
        if(photonView.IsMine)
        {
            animator.SetBool("IsDead", true);
            StartCoroutine(RespawnCountdown());
        }
    }

    IEnumerator RespawnCountdown()
    {
        GameObject respawnText = GameObject.Find("Respawn Text");
        float respawnTime = 5.0f;

        while(respawnTime > 0)
        {
            yield return new WaitForSeconds(1.0f);
            respawnTime--;

            transform.GetComponent<PlayerMovementController>().enabled = false;
            respawnText.GetComponent<Text>().text = "You died. Respawning in " + respawnTime.ToString(".00");
        }

        animator.SetBool("IsDead", false);
        respawnText.GetComponent<Text>().text = "";

        this.transform.position = Spawning.instance.spawnPoint[Random.RandomRange(0, 4)].transform.position;
        transform.GetComponent<PlayerMovementController>().enabled = true;

        photonView.RPC("RegainHealth", RpcTarget.AllBuffered);
    }

    IEnumerator KillFeed(PhotonMessageInfo info)
    {
        GameObject killFeedText = GameObject.Find("Kill Feed Text");
        float activeTime = 3.0f;

        while(activeTime > 0)
        {
            yield return new WaitForSeconds(1.0f);
            activeTime--;

            killFeedText.GetComponent<Text>().text = info.Sender.NickName + " killed " + info.photonView.Owner.NickName;
        }

        killFeedText.GetComponent<Text>().text = "";

        Debug.Log(info.Sender.NickName + "Score: " + info.Sender.GetScore());
        Debug.Log(info.photonView.Owner.NickName + "Score: " + info.photonView.Owner.GetScore());

        if (info.Sender.GetScore() == 10)
        {
            // display text
            GameObject winText = GameObject.Find("Winning Text");
            winText.GetComponent<Text>().text = info.Sender.NickName + " has won!";
        }
    }

    [PunRPC]
    public void RegainHealth()
    {
        health = 100;
        healthBar.fillAmount = health / startHealth;
    }
}
