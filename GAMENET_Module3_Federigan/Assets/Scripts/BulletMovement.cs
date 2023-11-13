using Photon.Pun.Demo.Asteroids;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BulletMovement : MonoBehaviourPunCallbacks
{
    private Rigidbody rigidBody;
    public float bulletSpeed = 100;
    public float bulletDamage;

    public bool isNotOwner;
    // Start is called before the first frame update
    void Start()
    {
        rigidBody = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        rigidBody.velocity = (transform.forward * bulletSpeed) * Time.deltaTime;
        StartCoroutine(DestroyBullet(5));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<PhotonView>() != null && collision.gameObject.GetComponent<PhotonView>().IsMine)
        {
            return;
        }

        if(collision.gameObject.GetComponent<PhotonView>() != null && isNotOwner)
        {
            collision.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, bulletDamage);
        }

        Destroy(gameObject);
    }

    IEnumerator DestroyBullet(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
