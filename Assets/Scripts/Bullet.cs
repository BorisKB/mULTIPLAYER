using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private float speed;

    private Rigidbody rigidbody;
    private float damage = 0;
    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.AddForce(transform.forward * speed);
        StartCoroutine(Destroy(gameObject, 15f));

    }

    public void SetDamage(float amount) 
    {
        damage = amount;
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null)
        {
            if (hasAuthority)
            {
                CmdSendDamage(player.GetComponent<PlayerHealth>());
            }
        }
        NetworkServer.Destroy(gameObject);
    }
    [Command]
    private void CmdSendDamage(PlayerHealth player) 
    {
        SrvGendDamage(player);
    }
    [Server]
    private void SrvGendDamage(PlayerHealth player) 
    {
        player.SrvTakeDamage(damage);
    }
    private IEnumerator Destroy(GameObject _gameObject, float time) 
    {
        yield return new WaitForSeconds(time);
        NetworkServer.Destroy(_gameObject);
    }
}
