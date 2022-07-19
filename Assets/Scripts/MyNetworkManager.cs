using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MyNetworkManager : NetworkManager
{
    [SerializeField] EnemyManager enemyManager;

    private List<Transform> players = new List<Transform>();

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);
        EnemyManager _enemyManager = Instantiate(enemyManager, Vector3.zero, Quaternion.identity);
        NetworkServer.Spawn(_enemyManager.gameObject, conn);
        players.Add(conn.identity.transform);
        _enemyManager.SetPlayers(players);
        conn.identity.GetComponent<PlayerMovement>().SetColor(new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
    }
}
