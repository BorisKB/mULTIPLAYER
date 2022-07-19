using System.Collections;
using System;
using UnityEngine;
using Mirror;

public class PlayerHealth : NetworkBehaviour
{
    public Action<float> onHealthChanged;

    [SyncVar] public float health;
    [SerializeField] private bool isRolling;
    [SerializeField] private GameObject hitFx;
    [SerializeField] private Rigidbody[] ragdolls;
    [SerializeField] private GameObject unit;

    #region Server
    [Server]
    public void SrvTakeDamage(float amount)
    {
        if (!isRolling)
        {
            health -= amount;
            GameObject _fx = Instantiate(hitFx, transform.position, transform.rotation);
            NetworkServer.Spawn(_fx, connectionToClient);
            StartCoroutine(Destroy(_fx, 6f));
            if (health <= 0)
            {
                SetRagdoll();
            }
        }
    }
    [ClientRpc]
    private void SetRagdoll() 
    {
        Monster monster = unit.GetComponent<Monster>();
        if (monster != null)
        {
            monster.Deactivated();
            foreach (var item in ragdolls)
            {
                item.GetComponent<Collider>().enabled = true;
                item.useGravity = true;
            }
            foreach (var item in ragdolls)
            {
                item.isKinematic = false;
            }
            StartCoroutine(Destroy(gameObject, 30f));
        }
    }
    #endregion

    public void SetRollingStatus(bool state) 
    {
        isRolling = state;
    }

    private IEnumerator Destroy(GameObject fx, float time) 
    {
        yield return new WaitForSeconds(time);
        NetworkServer.Destroy(fx);
    }
}
