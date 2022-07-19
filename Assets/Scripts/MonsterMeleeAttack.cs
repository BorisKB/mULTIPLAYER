using System;
using System.Collections;
using UnityEngine;
using Mirror;

public class MonsterMeleeAttack : MonoBehaviour
{
    public Action<PlayerMovement> onEnterPlayer;

    private float timeCheck = 0.5f;
    private bool isCheck = false;
    private PlayerMovement currentPlayer;
    private Collider playerCollider;

    private void OnTriggerStay(Collider other)
    {
        if (!isCheck)
        {
            if (!playerCollider == other)
            {
                StartCoroutine(Check());
                PlayerMovement player = other.GetComponent<PlayerMovement>();
                if (player != null)
                {
                    playerCollider = other;
                    currentPlayer = player;
                }
            }
            onEnterPlayer?.Invoke(currentPlayer);
        }
    }
    /*private void OnTriggerExit(Collider other)
    {
        other.TryGetComponent<PlayerMovement>(out PlayerMovement player);
        if (player != null)
        {
            onExitPlayer?.Invoke(player);
        }
    }*/
    private IEnumerator Check() 
    {
        isCheck = true;
        yield return new WaitForSeconds(timeCheck);
        isCheck = false;
    }
}
