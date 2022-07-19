using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EnemyManager : NetworkBehaviour
{

    [SerializeField] private List<Transform> _players;
    [SerializeField] private Monster[] monster;
    [SerializeField] private Transform[] spawnPoints;

    [SerializeField] private float spawnRate = 1f;
    [SerializeField] private float minSpawnRate = 5f;

    private bool canSpawn = true;
    // Start is called before the first frame update
    void Start()
    {

    }
    [Server]
    private void Spawn() {
        Monster _monster = Instantiate(monster[Random.Range(0, monster.Length)], spawnPoints[Random.Range(0, spawnPoints.Length)].position,Quaternion.identity);
        NetworkServer.Spawn(_monster.gameObject, connectionToClient);
        _monster.SetTargets(_players);
    }

    [Command]
    private void CmdSpawnMonster() 
    {
        Spawn();
    }

    [ClientCallback]
    void Update()
    {
        if (hasAuthority)
        {
            if (canSpawn)
            {
                StartCoroutine(SpawnRate());
                CmdSpawnMonster();
            }
        }
    }

    public void SetPlayers(List<Transform> targets) 
    {
        _players = targets;
    }

    private IEnumerator SpawnRate() 
    {
        canSpawn = false;
        yield return new WaitForSeconds(spawnRate);
        canSpawn = true;
    }
}
