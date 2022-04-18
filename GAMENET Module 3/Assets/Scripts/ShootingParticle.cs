using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ShootingParticle : MonoBehaviourPunCallbacks
{
    public Camera camera;

    public GameObject vehiclePrefab;
    public GameObject missileVehiclePrefab;

    [SerializeField]
    public float fireRate = 2.0f;
    private float fireTimer = 0;

    [Header("Health Related")]
    public float startHealth = 100;
    private float health;
    private float lives = 3;
    public Image healthBar;

    [Header("Bullet Properties")]
    public Rigidbody bulletPrefab;
    public float bulletSpeed;
    public GameObject bulletSpawn;

    // Start is called before the first frame update
    void Start()
    {
        health = startHealth;
        healthBar.fillAmount = health / startHealth;
    }

    // Update is called once per frame
    void Update()
    {
        GameObject winningText = GameObject.Find("WinningText");

        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            GetComponent<VehicleMovement>().enabled = false;
            winningText.GetComponent<Text>().text = "No other players remaining. You have won!";
            return;
        }

        if (fireTimer < fireRate)
        {
            fireTimer += Time.deltaTime;
        }

        if (Input.GetButton("Fire1") && fireTimer > fireRate)
        {
            fireTimer = 0.0f;

            if (vehiclePrefab == missileVehiclePrefab)
            {
                if (photonView.IsMine)
                {
                    photonView.RPC("ShootParticle", RpcTarget.All);
                }
            }
            else if(vehiclePrefab != missileVehiclePrefab)
            {
                Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.6f));
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100))
                {
                    Debug.Log(hit.collider.gameObject.name);
                    // check if you shot a player and didnt shoot urself, then broadcasts the damage to the server
                    if (hit.collider.gameObject.CompareTag("Player") && !hit.collider.gameObject.GetComponent<PhotonView>().IsMine)
                    {
                        hit.collider.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, 10);
                    }
                }
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        Destroy(other.gameObject);
        if (other.gameObject.CompareTag("Bullet"))
        {
            Debug.Log("Player Hit!");
            photonView.RPC("TakeDamage", RpcTarget.AllBuffered, 25);
        }
    }

    [PunRPC]
    public void ShootParticle()
    {
        Rigidbody bulletClone = Instantiate(bulletPrefab, bulletSpawn.transform.position, bulletSpawn.transform.rotation);

        bulletClone.velocity = transform.TransformDirection(new Vector3(0, 0, bulletSpeed));
    }

    [PunRPC]
    public void TakeDamage(int damage, PhotonMessageInfo info)
    {
        this.health -= damage;
        this.healthBar.fillAmount = health / startHealth;

        if (health == 0)
        {
            if (lives == 0)
            {
                GetComponent<VehicleMovement>().enabled = false;
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
        if (photonView.IsMine)
        {
            StartCoroutine(Respawn());
        }
    }

    IEnumerator Respawn()
    {
        GameObject respawnText = GameObject.Find("RespawnText");
        float respawnTime = 5.0f;

        while (respawnTime > 0)
        {
            yield return new WaitForSeconds(1.0f);
            respawnTime--;

            Debug.Log("Respawning in: " + respawnTime);
            stopPlayer();
            respawnText.GetComponent<Text>().text = "You died! Respawning in " + respawnTime.ToString(".00");
        }

        transform.GetComponent<VehicleMovement>().enabled = true;
        this.gameObject.GetComponent<Collider>().enabled = true;
        this.GetComponent<ShootingParticle>().enabled = true;

        respawnText.GetComponent<Text>().text = "";

        photonView.RPC("regainHealth", RpcTarget.AllBuffered);
    }


    IEnumerator PermaKillFeed(PhotonMessageInfo info)
    {
        GameObject permaKillFeedText = GameObject.Find("PermaKillFeedText");
        float activeTime = 5.0f;

        stopPlayer();

        while (activeTime > 0)
        {
            yield return new WaitForSeconds(1.0f);
            activeTime--;

            permaKillFeedText.GetComponent<Text>().text = info.photonView.Owner.NickName + " has been permanently killed";
        }
        permaKillFeedText.GetComponent<Text>().text = "";
        PhotonNetwork.LeaveRoom();
    }

    IEnumerator KillFeed(PhotonMessageInfo info)
    {
        GameObject killFeedText = GameObject.Find("KillFeedText");
        GameObject remainingLivesText = GameObject.Find("RemainingLivesText");

        float activeTime = 4.0f;

        while (activeTime > 0)
        {
            yield return new WaitForSeconds(1.0f);
            activeTime--;

            killFeedText.GetComponent<Text>().text = info.photonView.Owner.NickName + " has been killed by " + info.Sender.NickName;
            remainingLivesText.GetComponent<Text>().text = info.photonView.Owner.NickName + " remaining extra lives: " + this.lives;
        }

        remainingLivesText.GetComponent<Text>().text = "";
        killFeedText.GetComponent<Text>().text = "";
    }


    [PunRPC]
    public void regainHealth()
    {
        this.health = 100;
        this.healthBar.fillAmount = health / startHealth;
    }

    public void stopPlayer()
    {
        transform.GetComponent<VehicleMovement>().enabled = false;
        this.gameObject.GetComponent<Collider>().enabled = false;
        this.GetComponent<ShootingParticle>().enabled = false;
    }
}
