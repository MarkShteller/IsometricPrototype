﻿using System;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public static UIManager Instance;

    public Slider manaSlider;
    public Slider healthSlider;
    public Text scoreText;
    public Text scoreMultiplierText;
    public Text coreCount;

    public Transform powerupsGrid;
    public PowerupUIBehaviour powerupUIPrefab;

    [SerializeField] private GameOverDialog gameOverDialog;
    [SerializeField] private EndLevelReportDialog endLevelReportDialog;


    void Awake () {
        Instance = this;
	}

    public void SetScore(int value)
    {
        scoreText.text = value.ToString("N0");
    }

    public void SetMana(float value)
    {
        manaSlider.value = value;
    }

    public void SetHealth(float value)
    {
        healthSlider.value = value;
    }

    public void SetCoreCount(int value)
    {
        coreCount.text = value.ToString("N0");
    }

    public void OpenGameOverScreen(int score)
    {
        GameOverDialog dialog = Instantiate(gameOverDialog, this.transform);
        dialog.Init(score);
    }

    public void SetScoreMultiplier(float scoreMultiplier)
    {
        scoreMultiplierText.text = "x"+scoreMultiplier.ToString("N1");
    }

    public void OpenEndLevelDialog(int score, float maxMultiplier, float time, int enemyCount, float damage, LevelScriptableObject level)
    {
        EndLevelReportDialog dialog = Instantiate(endLevelReportDialog, this.transform);
        dialog.Init(score, maxMultiplier, time, enemyCount, damage, level);
    }

    public void AddPowerup(PowerUpObject powerupData)
    {
        PowerupUIBehaviour pub = Instantiate(powerupUIPrefab, powerupsGrid);
        pub.Init(powerupData);
    }

    public void UpdatePowerupTimer(PowerUpObject powerupData, float newTime)
    {
        foreach (PowerupUIBehaviour pub in powerupsGrid.GetComponentsInChildren<PowerupUIBehaviour>())
        {
            if (pub.powerUpData.Equals(powerupData))
            {
                pub.UpdateTimer(newTime);
                break;
            }
        }
    }

}
