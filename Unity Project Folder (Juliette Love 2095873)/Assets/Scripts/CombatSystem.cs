﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CombatState { START, PLAYERROLL, PLAYERCOMBAT, ENEMYTURN, WON, LOST }
public class CombatSystem : MonoBehaviour
{
    public bool CanRoll = true;
    public bool EnemyCanRoll = false;
    public CombatState state;

    public GameObject LoseUI;
    public GameObject WinUI;

    public GameObject RollButton;

    public GameObject MeleeAttackButton;
    public GameObject FireballAttackButton;
    public GameObject HealButton;
    public GameObject DefenseButton;

    public GameObject enemyMissExplanation;
    public bool FirstTimeEnemyMiss = true;

    public GameObject PlayerHealthGlow;
    public GameObject EnemyMissText;
    public GameObject PlayerMissText;

    public Text PlayerDamage;

    public float fillAmountHealth;
    public float fillAmountFireball;

    public Image FireballCharge;

    void Start()
    {
        state = CombatState.PLAYERROLL;
    }

    void Update()
    {
        Player player = GameObject.FindWithTag("Player").GetComponent<Player>();
        Enemy enemy = GameObject.FindWithTag("Enemy").GetComponent<Enemy>();
        DiceScript diceScript = GameObject.FindWithTag("Dice").GetComponent<DiceScript>();
        AttackScript attackScript = GameObject.FindWithTag("CombatSystem").GetComponent<AttackScript>();

        if (state == CombatState.PLAYERROLL)
        {
            RollButton.SetActive(true);
            fillAmountFireball = (attackScript.BurnChance - 1) / 3; //Because the BurnChance is a value between 2 and 4, I am subtracting 1 to make it out of 3.
            FireballCharge.fillAmount = fillAmountFireball / 1;
        }
        else
        {
            RollButton.SetActive(false);
        }

        if (state == CombatState.PLAYERCOMBAT)
        {
            EnemyCanRoll = false;

            EnemyChange enemyChange = GameObject.FindWithTag("EnemyChange").GetComponent<EnemyChange>();
               
            if (FireballAttackButton != null)
            {
                FireballAttackButton.SetActive(true);
            }

            if (/*hasUsed == false && */HealButton != null)
            {
                HealButton.SetActive(true);
            }    
            MeleeAttackButton.SetActive(true);

            if (DefenseButton != null && player.defenceNumber < 3)
            {
                DefenseButton.SetActive(true);
            }
        }

        else
        {
            if (HealButton != null)
            {
                HealButton.SetActive(false);
            }
            
            if (FireballAttackButton != null)
            {
                FireballAttackButton.SetActive(false);
            }

            if (DefenseButton != null)
            {
                DefenseButton.SetActive(false);
            }

            MeleeAttackButton.SetActive(false);
        }

        if (state == CombatState.ENEMYTURN)
        {
            Invoke("EnemyRoll", 2f);
        }

        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
    }


    public void EnemyRoll() //CODE DEALING WITH ENEMY TURN
    {
        if (EnemyCanRoll == true)
        {
            DiceScript diceScript = GameObject.FindWithTag("Dice").GetComponent<DiceScript>();

            diceScript.EnemyNumberRolled = Random.Range(1, 6);
            diceScript.EnemyDiceText.GetComponent<UnityEngine.UI.Text>().text = diceScript.EnemyNumberRolled.ToString("F0");
            
            ScreenshakeController screenShakeController = GameObject.FindWithTag("MainCamera").GetComponent<ScreenshakeController>();
            Player player = GameObject.FindWithTag("Player").GetComponent<Player>();
            Enemy enemy = GameObject.FindWithTag("Enemy").GetComponent<Enemy>();

            if (diceScript.EnemyNumberRolled <= player.defenceNumber) //Enemy misses.
            {
                EnemyMissText.SetActive(true);
                Invoke("FeedbackTextDisappear", 2f);
                Invoke("PlayerTurn", 2f);

                if(FirstTimeEnemyMiss == true && enemyMissExplanation != null)
                {
                    enemyMissExplanation.SetActive(true);
                    Invoke("MissTextDisappear", 3f);
                    FirstTimeEnemyMiss = false;
                }
            }

            if (player.defenceNumber < diceScript.EnemyNumberRolled) //Deals damage to player.
            {
                player.currentHealth -= enemy.attackPower;
                fillAmountHealth = player.currentHealth / 10;
                diceScript.playerHealth.fillAmount = fillAmountHealth / 1;

                Invoke("PlayerTurn", 2f);

                PlayerHealthGlow.SetActive(true);
                Invoke("GlowDisappear", 0.5f);

                PlayerDamage.text = enemy.attackPower.ToString();
                Invoke("DamageDisappear", 2f);

                StartCoroutine(screenShakeController.CameraShake(0.1f, 0.05f)); //Screenshake

                if (player.currentHealth < 1f) //Player Loses
                {
                    Debug.Log("Has Lost");
                    LoseUI.SetActive(true);
                }
            }

            AttackScript attackScript = GameObject.FindWithTag("CombatSystem").GetComponent<AttackScript>();

            EnemyCanRoll = false;
        }
    }

    void PlayerTurn()
    {
        DiceScript diceScript = GameObject.FindWithTag("Dice").GetComponent<DiceScript>();
        AttackScript attackScript = GameObject.FindWithTag("CombatSystem").GetComponent<AttackScript>();

        state = CombatState.PLAYERROLL;
        diceScript.ConsoleText.text = "Your turn";
        CanRoll = true;
    }

    void MissTextDisappear()
    {
        enemyMissExplanation.SetActive(false);
    }

    void GlowDisappear()
    {
        PlayerHealthGlow.SetActive(false);
    }

    void FeedbackTextDisappear()
    {
        EnemyMissText.SetActive(false);
        PlayerMissText.SetActive(false);
    }

    void DamageDisappear()
    {
        PlayerDamage.text = "";
    }
}
