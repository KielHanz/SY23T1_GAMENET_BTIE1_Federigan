using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class TakingDamage : MonoBehaviourPunCallbacks
{
    [SerializeField] private Image healthBar;

    private float initialHealth = 100;
    public float health;

    void Start()
    {
        health = initialHealth;
        healthBar.fillAmount = health / initialHealth;
    }

    [PunRPC]
    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log(health);

        healthBar.fillAmount = health / initialHealth;

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (photonView.IsMine)
        {
            GameManager.instance.LeaveRoom();
        }
    }
}
