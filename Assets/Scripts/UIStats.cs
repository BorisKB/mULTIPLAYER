using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStats : MonoBehaviour
{
    public Slider healthBar;
    public Slider staminaBar;
    public Slider manaBar;

    private PlayerMovement player;
    [SerializeField]private bool isGood = false;

    private PlayerHealth health;
      
    private void Update()
    {
        if (isGood)
        {
            staminaBar.value = Mathf.Min(player.stamina, staminaBar.maxValue);
            manaBar.value = Mathf.Min(player.mana, manaBar.maxValue);
            healthBar.value = Mathf.Min(health.health, healthBar.maxValue);
        }
    }

    public void SetParametrs(PlayerMovement _player) 
    {
        player = _player;
        if (player == null) { return; }
        healthBar.maxValue = player.GetComponent<PlayerHealth>().health;
        health = player.GetComponent<PlayerHealth>();

        manaBar.maxValue = player.mana;
        staminaBar.maxValue = player.stamina;
        isGood = true;
    }

}
