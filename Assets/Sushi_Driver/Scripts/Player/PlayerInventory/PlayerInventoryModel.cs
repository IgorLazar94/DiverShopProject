using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerInventoryModel : MonoBehaviour
{

    [SerializeField] private PlayerInventoryView view;
    private int maxFishValue = 0;
    private int currentTotalFishQuantity;
    private int currentFishAValue = 0;
    private int currentFishBValue = 0;
    private int currentFishCValue = 0;

    private void Start()
    {
        CalculateTotalFishQuantity();
        //maxFishValue = 12; // (!) from GameSettings
        PlayerInventoryPresenter.OnMaxFishChanged.Invoke(12);
    }
    private void OnEnable()
    {
        PlayerInventoryPresenter.OnMaxFishChanged += SetMaxFishValue;
        PlayerInventoryPresenter.OnCurrentFishChanged += SetCurrentFishValue;
        //PlayerInventoryPresenter.OnCurrentFishRemoved += RemoveFish;
    }

    private void OnDisable()
    {
        PlayerInventoryPresenter.OnMaxFishChanged -= SetMaxFishValue;
        PlayerInventoryPresenter.OnCurrentFishChanged -= SetCurrentFishValue;
        //PlayerInventoryPresenter.OnCurrentFishRemoved -= RemoveFish;
    }

    private void CalculateTotalFishQuantity()
    {
        currentTotalFishQuantity = currentFishAValue + currentFishBValue + currentFishCValue;
    }

    public void SetView(PlayerInventoryView view)
    {
        this.view = view;
    }

    private void SetMaxFishValue(int value)
    {
        if (value <= 0) return;
        maxFishValue = value;
        view.UpdateMaxFishText(value);
    }
    private void SetCurrentFishValue(int value, TypeOfFish typeOfFish)
    {
        //if (currentFishValue + value > maxFishValue) currentFishValue = maxFishValue;

        switch (typeOfFish)
        {
            case TypeOfFish.FishA:
                currentFishAValue += value;
                break;
            case TypeOfFish.FishB:
                currentFishBValue += value;
                break;
            case TypeOfFish.FishC:
                currentFishCValue += value;
                break;
            default:
                Debug.LogWarning("Undefined type Of Fish");
                break;
        }
        CalculateTotalFishQuantity();
        CheckMaxFish();
        view.UpdateCurrentFishText(currentTotalFishQuantity);
    }

    private void CheckMaxFish()
    {
        if (currentTotalFishQuantity > maxFishValue)
        {
            Debug.Log("Max inventory"); // Action to block fishing
        }
    }

    public int GetCurrentFishAValue()
    {
        return currentFishAValue;
    }

    public int GetCurrentFishBValue()
    {
        return currentFishBValue;
    }
    public int GetCurrentFishCValue()
    {
        return currentFishCValue;
    }

    public void RemoveAllFish()
    {
        currentFishAValue = 0;
        currentFishBValue = 0;
        currentFishCValue = 0;
        CalculateTotalFishQuantity();
        view.UpdateCurrentFishText(currentTotalFishQuantity);
    }

    private void Update()
    {
        Debug.Log(currentFishAValue + "A");
        Debug.Log(currentFishBValue + "B");
        Debug.Log(currentFishCValue + "C");
    }
}
