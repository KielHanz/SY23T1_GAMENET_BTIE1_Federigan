using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using Unity.VisualScripting;
using Photon.Pun.Demo.Cockpit;

public class Shooting : MonoBehaviourPunCallbacks
{
    public Camera camera;
    public GameObject hitEffectPrefab;

    [Header("HP Related Stuff")]
    public float startHealth = 100;
    private float health;
    public Image healthBar;

    public int killCount;
    public GameObject killListPrefab;
    public GameObject killListParent;

    private bool isDead;
    private bool hasDied;

    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
        {
            health = startHealth;
            healthBar.fillAmount = health / startHealth;
            
            photonView.RPC("SyncHealth", RpcTarget.OthersBuffered, health);
        }


        animator = this.GetComponent<Animator>();

    }

    private void Update()
    {
        photonView.RPC("CheckCollider", RpcTarget.AllBuffered);
    }

    public void Fire()
    {
        RaycastHit hit;
        Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        if (Physics.Raycast(ray, out hit, 200))
        {
            Debug.Log(hit.collider.gameObject.name);

            photonView.RPC("CreateHitEffects", RpcTarget.All, hit.point);

            if (hit.collider.gameObject.CompareTag("Player") && !hit.collider.gameObject.GetComponent<PhotonView>().IsMine)
            {
                hit.collider.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, 25);

                if (hit.collider.gameObject.GetComponent<Shooting>().isDead && hit.collider.gameObject.GetComponent<Shooting>().hasDied)
                {
                    hit.collider.GetComponent<CapsuleCollider>().enabled = false;
                    photonView.RPC("MakeHasDiedFalse", RpcTarget.AllBuffered);
                    photonView.RPC("AddKill", RpcTarget.AllBuffered);
                    GameObject playerUI = GameObject.Find("KillCountText");
                    playerUI.GetComponent<Text>().text = "Kills: " + killCount;
                }
            }
        }
    }

    IEnumerator RespawnCountdown()
    {
        GameObject respawnText = GameObject.Find("RespawnText");
        float respawnTime = 5.0f;

        while (respawnTime > 0)
        {
            yield return new WaitForSeconds(1.0f);
            respawnTime--;

            transform.GetComponent<PlayerMovementController>().enabled = false;
            respawnText.GetComponent<Text>().text = "You are killed. Respawning in " + respawnTime.ToString(".00");
        }


        animator.SetBool("isDead", false);
        respawnText.GetComponent<Text>().text = "";

        int randomSpawnPoint = Random.Range(0, GameManager.Instance.spawnPoints.Count);

        this.transform.position = GameManager.Instance.spawnPoints[randomSpawnPoint].transform.position;
        transform.GetComponent<PlayerMovementController>().enabled = true;

        photonView.RPC("RegainHealth", RpcTarget.AllBuffered);

    }

    #region PunRPCs
    [PunRPC]
    public void TakeDamage(int damage, PhotonMessageInfo info)
    {
        
        this.health -= damage;
        this.healthBar.fillAmount = health / startHealth;

        if (this.isDead && this.hasDied)
        {
            photonView.RPC("MakeDeadFalse", RpcTarget.AllBuffered);
        }

        if (health <= 0 && !this.isDead)
        {
            photonView.RPC("PlayerDie", RpcTarget.AllBuffered);
            Die();
            Debug.Log(info.Sender.NickName + " killed " + info.photonView.Owner.NickName);

            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                GameObject killList = Instantiate(killListPrefab);
                killListParent = GameObject.Find("WhoKilledUIList");
                killList.transform.SetParent(killListParent.transform);
                killList.transform.localScale = Vector3.one;

                killList.transform.Find("KillListText").GetComponent<Text>().text = info.Sender.NickName + " has killed " + info.photonView.Owner.NickName;

                Destroy(killList, 5.0f);
            }

        }

    }


    [PunRPC]
    public void CreateHitEffects(Vector3 position)
    {
        GameObject hitEffectGameObject = Instantiate(hitEffectPrefab, position, Quaternion.identity);
        Destroy(hitEffectGameObject, 0.2f);
    }

    public void Die()
    {
        if(photonView.IsMine)
        {
            animator.SetBool("isDead", true);
            StartCoroutine(RespawnCountdown());
        }
    }

    [PunRPC]
    public void RegainHealth()
    {
        health = 100;
        healthBar.fillAmount = health / startHealth;
        this.isDead = false;
        this.hasDied= false;
        this.GetComponent<CapsuleCollider>().enabled = true;

    }

    [PunRPC]
    public void PlayerDie()
    {
        this.isDead = true;
        this.hasDied = true;

    }

    [PunRPC]
    public void SyncHealth(float newHealth)
    {
        health = newHealth;
        healthBar.fillAmount = health / startHealth;
    }

    [PunRPC]
    public void CheckCollider()
    {
        if (!this.GetComponent<CapsuleCollider>().enabled && !this.isDead) 
        {
            this.GetComponent<CapsuleCollider>().enabled = true;
        }
    }

    [PunRPC]
    public void AddKill()
    {
        killCount++;
        photonView.RPC("WinCondition", RpcTarget.AllBuffered);

    }

    [PunRPC]
    public void MakeDeadFalse()
    {
        this.isDead = false;
    }


    [PunRPC]
    public void MakeHasDiedFalse()
    {
        this.hasDied = false;
    }

    [PunRPC]
    public void WinCondition(PhotonMessageInfo info)
    {
        if (killCount >= GameManager.Instance.killsToWin)
        {
            Debug.Log(info.photonView.Owner.NickName + " is the winner");
            GameManager.Instance.winnerText.text = info.photonView.Owner.NickName + " is the winner";
            GameManager.Instance.gameOver = true;
        }
    }
    #endregion

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {

        if (photonView.IsMine && newPlayer.IsLocal)
        {
            //send the local player's health to the newly joined player
            photonView.RPC("SyncHealth", newPlayer, health);
        }

        photonView.RPC("MakeHasDiedFalse", RpcTarget.AllBuffered);
        
    }
}
