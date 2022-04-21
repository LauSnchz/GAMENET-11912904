using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Pun.UtilityScripts;

public class PlayerDamage : MonoBehaviourPunCallbacks
{
    public Camera camera;

    [Header ("Player Damage")]
    public float attackRate = 0.8f;
    private float attackTimer = 0;

    [Header ("Stun Feature")]
    private float stunAttackRate = 10.0f;
    private float stunAttackTimer = 0;

    [Header("Health Related")]
    public float startHealth = 100;
    private float health;
    public Image healthBar;
    private float lives = 3;
    public int playerDamage;

    public int playerScore;

    // Start is called before the first frame update
    void Start()
    {
        health = startHealth;
        healthBar.fillAmount = health / startHealth;
    }

    // Update is called once per frame
    void Update()
    {
        this.playerScore = photonView.Owner.GetScore();
        this.gameObject.GetComponent<RaiseEventScript>().GetPlayerScore(this.playerScore);

        // Damage
        if (attackTimer < attackRate)
        {
            attackTimer += Time.deltaTime;
        }

        if (Input.GetButton("Fire1") && attackTimer > attackRate)
        {
            attackTimer = 0.0f;
                
            Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 3))
            {
                Debug.Log(hit.collider.gameObject.name);

                if(hit.collider.CompareTag("Player") && !hit.collider.gameObject.GetComponent<PhotonView>().IsMine)
                {
                    Debug.Log("Player Hit!");
                    hit.collider.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, playerDamage);
                }
            }
        }

        // Stun
        if(stunAttackTimer < stunAttackRate)
        {
            stunAttackTimer += Time.deltaTime;
        }

        if(Input.GetButtonDown("Fire2") && stunAttackTimer > stunAttackRate)
        {
            stunAttackTimer = 0.0f;

            Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 4))
            {
                if (hit.collider.CompareTag("Player") && !hit.collider.gameObject.GetComponent<PhotonView>().IsMine)
                {
                    Debug.Log("Player Hit!");
                    hit.collider.gameObject.GetComponent<PhotonView>().RPC("StunPlayer", RpcTarget.All);
                }
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Debuff"))
        {
            if(photonView.IsMine)
            {
                Debug.Log("You are stunned!");
                StartCoroutine(StunPlayerAction());
                this.transform.position = Spawning.instance.spawnPoints[Random.Range(0, 8)].transform.position;
            }
        }
    }

    [PunRPC]
    public void StunPlayer()
    {
        if(photonView.IsMine)
        {
            StartCoroutine(StunPlayerAction());
        }
    }

    [PunRPC]
    public void TakeDamage(int damage, PhotonMessageInfo info)
    {
        this.health -= damage;
        this.healthBar.fillAmount = health / startHealth;

        if (health == 0)
        {
            info.Sender.AddScore(1);
            if (lives == 0)
            {
                GetComponent<PlayerMovement>().enabled = false;
                GetComponent<PlayerDamage>().enabled = false;
                StartCoroutine(PermaKillFeed(info));
                return;
            }
            lives--;
            StartCoroutine(KillFeed(info));

            Debug.Log(info.Sender.NickName + " killed " + info.photonView.Owner.NickName);
            Debug.Log(info.photonView.Owner.NickName + " lives remaining: " + this.lives);
            Die();
        }
    }

    public void Die()
    {
        if(photonView.IsMine)
        {
            StartCoroutine(Respawn());
        }
    }

    IEnumerator Respawn()
    {
        GameObject respawnText = GameObject.Find("RespawnText");
        float respawnTime = 5.0f;

        while(respawnTime > 0)
        {
            yield return new WaitForSeconds(1.0f);
            respawnTime--;

            transform.GetComponent<PlayerMovement>().enabled = false;
            this.gameObject.GetComponent<Collider>().enabled = false;
            respawnText.GetComponent<Text>().text = "You died! Respawning in " + respawnTime.ToString(".00");
        }

        respawnText.GetComponent<Text>().text = "";
        this.gameObject.GetComponent<Collider>().enabled = true;
        this.transform.position = Spawning.instance.spawnPoints[Random.RandomRange(0, 8)].transform.position;
        transform.GetComponent<PlayerMovement>().enabled = true;

        photonView.RPC("RegainHealth", RpcTarget.AllBuffered);
    }

    IEnumerator KillFeed(PhotonMessageInfo info)
    {
        GameObject killFeedText = GameObject.Find("KillFeedText");
        GameObject remainingLivesText = GameObject.Find("RemainingLivesText");

        float activeTime = 4.0f;

        while(activeTime > 0)
        {
            yield return new WaitForSeconds(1.0f);
            activeTime--;

            killFeedText.GetComponent<Text>().text = info.Sender.NickName + " has killed " + info.photonView.Owner.NickName;
            remainingLivesText.GetComponent<Text>().text = info.photonView.Owner.NickName + " remaining extra lives: " + this.lives;
        }

        killFeedText.GetComponent<Text>().text = "";
        remainingLivesText.GetComponent<Text>().text = "";
    }

    IEnumerator PermaKillFeed(PhotonMessageInfo info)
    {
        GameObject permaKillFeed = GameObject.Find("PermaKillFeedText");

        float activeTime = 4.0f;

        while(activeTime > 0)
        {
            yield return new WaitForSeconds(1.0f);
            activeTime--;

            permaKillFeed.GetComponent<Text>().text = info.Sender.NickName + " has PERMANENTLY killed " + info.photonView.Owner.NickName;
        }
        permaKillFeed.GetComponent<Text>().text = "";
    }

    IEnumerator StunPlayerAction()
    {
        GameObject stunPlayerText = GameObject.Find("StunPlayerText");
        float stunTime = 4.0f;

        while(stunTime > 0)
        {
            yield return new WaitForSeconds(1.0f);
            stunTime--;

            transform.GetComponent<PlayerMovement>().enabled = false;
            stunPlayerText.GetComponent<Text>().text = "YOU ARE STUNNED! " + stunTime.ToString(".00");
        }

        stunPlayerText.GetComponent<Text>().text = "";
        transform.GetComponent<PlayerMovement>().enabled = true;
    }

    [PunRPC]
    public void RegainHealth()
    {
        health = startHealth;
        healthBar.fillAmount = health / startHealth;
    }
}