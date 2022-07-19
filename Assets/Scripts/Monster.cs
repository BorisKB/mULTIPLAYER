using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;

public class Monster : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float meleeRange = 2f;
    [SerializeField] private List<Transform> targets = new List<Transform>();
    [SerializeField] private Transform currentTarget;
    [SerializeField] private float damage = 1f;
    [SerializeField] private bool isRange;
    [SerializeField] private Bullet bullet;
    [SerializeField] private Transform firePoint;

    [SerializeField]private MonsterMeleeAttack monsterMelee;
    [SerializeField]private MonsterMeleeAttack monsterRange;

    private Animator animator;
    private NetworkAnimator netAnimator;
    private bool isAttack;
    private bool isDead = false;

    [SyncVar][SerializeField]private PlayerMovement currentAttackingTarget;
    private void Awake()
    {
        if (isRange)
        {
            monsterRange.onEnterPlayer += Attack;
        }
        else {
            monsterMelee.onEnterPlayer += Attack;
        }
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        netAnimator = GetComponent<NetworkAnimator>();
    }

    #region Server
    [Server]
    private void FoundTarget(Transform _target)
    {
        currentTarget = _target;
    }
    [Server]
    private void Move(Vector3 position)
    {
        agent.SetDestination(position);
    }
    [Server]
    private void Attacking(PlayerMovement player) 
    {
        player.GetComponent<PlayerHealth>().SrvTakeDamage(damage);
    }
    [Server]
    private void SrvClearAttackTarget() 
    {
        currentAttackingTarget = null;
    }
    [Server]
    private void SrvCasting() 
    {
        Bullet _bullet = Instantiate(bullet, firePoint.position, firePoint.rotation);
        _bullet.SetDamage(damage);
        NetworkServer.Spawn(_bullet.gameObject, connectionToClient);
    }
    [Command]
    private void CmdFoundTarget() 
    {
        if (targets.Count == 0) { return; }
        Transform closeTarget = targets[0];
        if(targets.Count > 1) 
        { 
            for (int i = 1; i < targets.Count; i++)
            {
                if (Vector3.Distance(targets[i - 1].position, transform.position) >= Vector3.Distance(targets[i].position, transform.position))
                {
                    closeTarget = targets[i];
                }
                else 
                {
                    closeTarget = targets[i-1];
                }
            }
        }
        FoundTarget(closeTarget);
    }
    [Command]
    private void CmdMove() 
    {
        Move(currentTarget.position);
    }
    [Command]
    private void CmdCasting() 
    {
        SrvCasting();
    }
    [Command]
    private void CmdClearAttackTarget() 
    {
        SrvClearAttackTarget();
    }
    [Command]
    private void CmdAttack(PlayerMovement player) 
    {
        if (Vector3.Distance(player.transform.position, transform.position) <= meleeRange) 
        {
            Attacking(player);
        }
    }
    [ClientCallback]
    void Update()
    {
        if (hasAuthority)
        {
            if (!isDead)
            {
                if (currentAttackingTarget != null)
                {
                    transform.LookAt(currentAttackingTarget.transform);
                }
                if (currentTarget == null)
                {
                    CmdFoundTarget();
                }
                else
                {
                    CmdMove();
                }
                animator.SetFloat("Velocity", agent.velocity.sqrMagnitude, 0.1f, Time.deltaTime);
            }
        }
    }
    #endregion

    #region AnimationScripts

    public void StartAttack() => isAttack = true;
    public void EndAttack() => isAttack = false;

    public void WhenAttackingMelee() 
    {
        if (hasAuthority)
        {
            if (currentAttackingTarget != null)
            {
                transform.LookAt(currentAttackingTarget.transform.position);
            }
            if (Vector3.Distance(currentAttackingTarget.transform.position, transform.position) <= meleeRange)
            {
                CmdAttack(currentAttackingTarget);
            }
        }
    }
    public void WhenAttackingRange()
    {
        if (hasAuthority)
        {
            if (currentAttackingTarget != null)
            {
                transform.LookAt(currentAttackingTarget.transform.position);
            }
            CmdCasting();
        }
    }
    public void ClearAttackTarget() 
    {
        if (hasAuthority)
        {
            CmdClearAttackTarget();
        }
    }
    #endregion
    private void Attack(PlayerMovement player) 
    {
        if (hasAuthority) 
        {
            if (currentAttackingTarget == null)
            {
                if (!isAttack)
                {
                    if (isRange)
                    {
                        netAnimator.SetTrigger("Casting");
                    }
                    else
                    {
                        netAnimator.SetTrigger("Attack");
                    }
                    currentAttackingTarget = player;
                }
            }            
        }
    }
    public void SetTargets(List<Transform> _targets)
    {
        targets = _targets;
    }
    public void Deactivated() 
    {
        isDead = true;
        if (isRange)
        {
            monsterRange.onEnterPlayer -= Attack;
        }
        else
        {
            monsterMelee.onEnterPlayer -= Attack;
        }
        netAnimator.enabled = false;
        animator.enabled = false;
        agent.ResetPath();
        agent.enabled = false;
        GetComponent<Collider>().enabled = false;
    }
}
