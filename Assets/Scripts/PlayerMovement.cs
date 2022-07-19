using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerMovement : NetworkBehaviour
{
    [SyncVar(hook = nameof(HandleColorUpdated))] public Color playerColor;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private SkinnedMeshRenderer playerMaterial;
    [SerializeField] private ParticleSystem playerTrail;
    [SerializeField] private NetworkAnimator netAnimator;
    [SerializeField] private Animator animator;
    [SerializeField] private float _speed;
    [SerializeField] private LayerMask _aimLayerMask;
    [SerializeField] private GameObject attackSpell1Fx;
    [SerializeField] private GameObject attackSpell2Fx;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float damage;
    [SerializeField] private float DamageSpell1;
    [SerializeField] private float DamageSpell2;
    public float mana;
    [SerializeField] private float maxMana;
    public float stamina;
    [SerializeField] private float maxStamina;
    [SerializeField] private float staminaAttackCost;
    [SerializeField] private float staminaSpell1Cost;
    [SerializeField] private float manaSpell2Cost;
    [SerializeField] private float regenPerSec;
    [SerializeField] private GameObject particleEffectSword;


    private bool canMovement = true;
    private bool canRotate = true;


    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        CameraPlayer camera = FindObjectOfType<CameraPlayer>();
        camera.SetCameraFollow(transform);
        FindObjectOfType<UIStats>().SetParametrs(this);
    }
    [Server]
    public void SetColor(Color color)
    {
        playerColor = color;
    }
    [Server]
    private void SrvAttack(PlayerHealth enemy, float amount)
    {
        enemy.SrvTakeDamage(amount);
    }
    [Server]
    private void SrvSpawnParticles(Vector3 position)
    {
        GameObject vfx = Instantiate(attackSpell2Fx, position, Quaternion.identity);
        NetworkServer.Spawn(vfx, connectionToClient);
        StartCoroutine(DestroyParticles(vfx, 4f));
    }
    [Command]
    private void CmdAttack(PlayerHealth enemy, float amount)
    {
        if (enemy != null)
        {
            SrvAttack(enemy, amount);
        }
    }
    [Command]
    private void CmdSpawnParticles(Vector3 postion) 
    {
        SrvSpawnParticles(postion);
    }

    [ClientCallback]
    private void Update()
    {
        if (hasAuthority)
        {
            if (mana < maxMana) 
            {
                mana += regenPerSec * Time.deltaTime;
            }
            if (stamina < maxStamina) 
            {
                stamina += regenPerSec * Time.deltaTime;
            }
            if (canMovement)
            {
                Movement();
            }
            if (canRotate)
            {
                AimTowardMouse();
            }
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (stamina >= staminaAttackCost)
                {
                    netAnimator.SetTrigger("Attack");
                    canMovement = false;
                    
                }
            }
            if (canMovement)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (stamina >= staminaSpell1Cost)
                    {
                        netAnimator.SetTrigger("JumpAttack");
                        canMovement = false;
                        stamina -= staminaSpell1Cost;
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Space))
                {
                    netAnimator.SetTrigger("Rolling");
                }
                else if (Input.GetKeyDown(KeyCode.Q))
                {
                    if (mana >= manaSpell2Cost)
                    {
                        netAnimator.SetTrigger("SpellCasting2");
                        mana -= manaSpell2Cost;
                    }
                }
            }
        }
    }

    private void Attack()
    {
        if (hasAuthority)
        {
            Collider[] enemy = Physics.OverlapSphere(transform.position + transform.forward + Vector3.up, 1f, enemyLayer);
            foreach (var item in enemy)
            {
                CmdAttack(item.GetComponent<PlayerHealth>(), damage);
            }
            stamina -= staminaAttackCost;
        }
    }
    private void SpellAttack1() 
    {
        Collider[] enemy = Physics.OverlapSphere(transform.position + transform.forward + Vector3.up, 4f, enemyLayer);
        foreach (var item in enemy)
        {
            CmdAttack(item.GetComponent<PlayerHealth>(), DamageSpell1);
        }
    }
    private IEnumerator SpellAttack2()
    {
        if (hasAuthority)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, _aimLayerMask))
            {
                CmdSpawnParticles(hitInfo.point);
            }
            yield return new WaitForSeconds(3f);

            Collider[] enemy = Physics.OverlapSphere(hitInfo.point + Vector3.up, 3f, enemyLayer);
            foreach (var item in enemy)
            {
                CmdAttack(item.GetComponent<PlayerHealth>(), DamageSpell2);
            }
        }
    }
    private IEnumerator DestroyParticles(GameObject fx, float time) 
    {
        yield return new WaitForSeconds(time);
        NetworkServer.Destroy(fx);
    }
    private void HandleColorUpdated(Color oldColor, Color newColor)
    {
        playerMaterial.material.SetColor("_BaseColor", newColor);
        playerTrail.startColor = newColor;
    }
    private void Movement()
    {
        if (transform.position.y < 0) { transform.position += Vector3.up * 0.05f; }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0f, vertical);

        if (movement.magnitude > 0)
        {
            transform.Translate(movement.normalized * (_speed * Time.deltaTime), Space.World);
        }

        float velocityZ = Vector3.Dot(movement.normalized, transform.right);
        float velocityX = Vector3.Dot(movement.normalized, transform.forward);


        animator.SetFloat("VelocityX", velocityX, 0.1f, Time.deltaTime);
        animator.SetFloat("VelocityZ", velocityZ, 0.1f, Time.deltaTime);
    }
    private void AimTowardMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, _aimLayerMask))
        {
            var _direction = hitInfo.point - transform.position;
            _direction.y = 0;
            _direction.Normalize();
            transform.forward = _direction;
        }
    }
    #region AnimationScripts

    public void CantMovement() => canMovement = false;
    public void CanMovement() => canMovement = true;
    public void CanRotate() => canRotate = true;
    public void CantRotate() => canRotate = false;
    public void RollingTrue() => playerHealth.SetRollingStatus(true);
    public void RollingFalse() => playerHealth.SetRollingStatus(false);
    public void DisableSwordEf() => particleEffectSword.SetActive(false);
    public void EnableSwordEf() => particleEffectSword.SetActive(true);

    public void StartEffect1() 
    {
        GameObject vfx = Instantiate(attackSpell1Fx, transform.position + transform.forward, Quaternion.identity);
        SpellAttack1();
        Destroy(vfx, 6f);
    }
    public void StartEffect2()
    {
        StartCoroutine(SpellAttack2());   
    }
    public void AttackAnim() 
    {
        if (hasAuthority) 
        {
            Attack();
        }
    }
    #endregion

}
