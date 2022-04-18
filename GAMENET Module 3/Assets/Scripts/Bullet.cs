using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Bullet : MonoBehaviourPunCallbacks
{
    public float lifeTime = 3.0f;
    private void Start()
    {
        Destroy(this.gameObject, lifeTime);
    }
}
