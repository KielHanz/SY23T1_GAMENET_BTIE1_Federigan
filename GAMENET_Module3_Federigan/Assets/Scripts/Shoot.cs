using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviourPunCallbacks
{
    public enum ShootingType
    {
        Laser,
        Projectile
    }

    public bool isShootEnabled;

    public ShootingType shootingType;

    public List<Transform> nozzlePoint = new List<Transform>();

    public GameObject bulletPrefab;

    public Camera cam;

    private LineRenderer lineLaser;

    public Transform laserPoint;

    public float ammo;
    public float maxAmmo;

    public float fireRate;

    public float damage;

    private bool canShoot = true;


    // Start is called before the first frame update
    void Start()
    {
        isShootEnabled = false;

        if (shootingType == ShootingType.Laser)
        {
            lineLaser = GetComponent<LineRenderer>();
            cam = GetComponentInChildren<Camera>();
            
        }

        ammo = maxAmmo;
    }
    // Update is called once per frame
    void Update()
    {
        if (!isShootEnabled)
        {
            return;
        }

        if (photonView.IsMine && canShoot && Input.GetKey(KeyCode.Space))
        {
            if (shootingType == ShootingType.Laser && ammo > 0)
            {
                photonView.RPC("SetLineRendererState", RpcTarget.All, true);

                photonView.RPC("RaycastLaser", RpcTarget.All);

                //RaycastLaser();
            }
            else if (shootingType == ShootingType.Projectile && ammo > 0)
            {
                photonView.RPC("ShootProjectile", RpcTarget.All);
                //ShootProjectile();
            }
        }
        else if ((!canShoot || Input.GetKeyUp(KeyCode.Space)) && shootingType == ShootingType.Laser)
        {
            photonView.RPC("SetLineRendererState", RpcTarget.All, false);
        }

        if (ammo <= 0 && canShoot)
        {
            canShoot = false;
            StartCoroutine(Reload());
        }
    }

    [PunRPC]
    void RaycastLaser()
    {
        if (photonView.IsMine)
        {
            Vector3 ray = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
            RaycastHit hit;
            if (Physics.Raycast(ray, cam.transform.forward, out hit, 50f))
            {
                photonView.RPC("UpdateLineRenderer", RpcTarget.All, laserPoint.position, hit.point);

                ammo -= Time.deltaTime;

                if (hit.collider.CompareTag("Player") && !hit.collider.gameObject.GetComponent<PhotonView>().IsMine)
                {
                    hit.collider.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, damage);
                }
            }
            else
            {
                photonView.RPC("UpdateLineRenderer", RpcTarget.All, laserPoint.position, ray + (cam.transform.forward * 50f));
            }
        }
    }

    [PunRPC]
    void ShootProjectile()
    {
        StartCoroutine(FireRateShoot());

        for (int i = 0; i < nozzlePoint.Count; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab, nozzlePoint[i].transform.position, transform.rotation);
            bullet.GetComponent<BulletMovement>().bulletDamage = damage;
            
            if (photonView.IsMine)
            {
                bullet.GetComponent<BulletMovement>().isNotOwner = true;
            }

            ammo--;
        }
    }

    [PunRPC]
    void SetLineRendererState(bool state)
    {
         lineLaser.enabled = state;
    }

    [PunRPC]
    void UpdateLineRenderer(Vector3 startPosition, Vector3 endPosition)
    {

        lineLaser.SetPosition(0, startPosition);
        lineLaser.SetPosition(1, endPosition);
        
    }

    IEnumerator FireRateShoot()
    {
        canShoot = false;
        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }

    IEnumerator Reload()
    {
        yield return new WaitForSeconds(3);
        ammo = maxAmmo;
        canShoot = true;
    }
}
