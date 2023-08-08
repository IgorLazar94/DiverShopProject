using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TypeOfParameter
{
    Harpoon,
    Backpack,
    Speed,
    FOV
}
public class Parameter : MonoBehaviour
{
    [SerializeField] private TypeOfParameter typeOfParameter;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Image updateLevelButtonImage;
    [SerializeField] private UIController uIController;
    [SerializeField] private GameObject maxLabel;
    private int level = 1;
    private int price;
    private string violetHEX = "F359EC";
    private string orangeHEX = "FD9C68";
    private int price_1Level;
    private int price_2Level;
    private int price_3Level;
    private int price_4Level;
    private int price_5Level;

    private void Start()
    {
        CalculatePrices();
        CalculateNewLevelParameters();
    }

    private void CalculatePrices()
    {
        price_1Level = GameSettings.Instance.GetParameterPriceLevelOne();
        price_2Level = GameSettings.Instance.GetParameterPriceLevelTwo();
        price_3Level = GameSettings.Instance.GetParameterPriceLevelThree();
        price_4Level = GameSettings.Instance.GetParameterPriceLevelFour();
        price_5Level = GameSettings.Instance.GetParameterPriceLevelFive();
    }

    //public void SetUIParentObject(UIController _uiController)
    //{
    //    uIController = _uiController;
    //}

    private void CalculateNewLevelParameters()
    {
        switch (level)
        {
            case 1:
                price = price_1Level;
                levelText.color = Color.grey;
                break;
            case 2:
                price = price_2Level;
                levelText.color = Color.green;
                break;
            case 3:
                price = price_3Level;
                levelText.color = Color.blue;
                if (typeOfParameter == TypeOfParameter.Harpoon)
                {
                    ActivateMaxLabel();
                }
                break;
            case 4:
                price = price_4Level;
                levelText.color = HexToColor(orangeHEX);
                break;
            case 5:
                price = price_5Level;
                levelText.color = HexToColor(violetHEX);
                ActivateMaxLabel();
                break;
            default:
                Debug.LogWarning("Undefined parameter level update");
                break;
        }
        UpdateLevelText();
        uIController.CheckTrainingButtonsActive();
    }

    private void UpdateParameter()
    {
        switch (typeOfParameter)
        {
            case TypeOfParameter.Harpoon:
                TrainingZone.OnHarpoonUpdateParameter.Invoke();
                break;
            case TypeOfParameter.Backpack:
                TrainingZone.OnBackpackUpdateParameter.Invoke();
                break;
            case TypeOfParameter.Speed:
                TrainingZone.OnSpeedUpdateParameter.Invoke();
                break;
            case TypeOfParameter.FOV:
                TrainingZone.OnFOVUpdateParameter.Invoke();
                break;
            default:
                break;
        }
    }

    private void UpdateLevelText()
    {
        levelText.text = "Lvl " + level.ToString();
        costText.text = "Cost: " + price.ToString();
    }

    private Color HexToColor(string hex)
    {
        hex = hex.Replace("#", "");
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        byte a = 255;
        return new Color32(r, g, b, a);
    }

    public void LevelUpdate()
    {
        Debug.Log(typeOfParameter + "type of parameter update");
        PlayerInventoryPresenter.OnCurrentDollarsChanged(-price);
        UpdateParameter();
        level++;
        CalculateNewLevelParameters();
        //CheckReadyUpdateButton();
    }

    public void CheckReadyUpdateButton()
    {
        if (PlayerInventoryModel.dollarsInInventory >= price)
        {
            ActivateButton();
        }
        else
        {
            DeactivateButton();
        }
    }

    private void ActivateButton()
    {

        updateLevelButtonImage.color = Color.green;
        updateLevelButtonImage.gameObject.GetComponent<Button>().enabled = true;
    }

    private void DeactivateButton()
    {
        updateLevelButtonImage.color = Color.red;
        updateLevelButtonImage.gameObject.GetComponent<Button>().enabled = false;
    }

    private void ActivateMaxLabel()
    {
        maxLabel.SetActive(true);
    }
}