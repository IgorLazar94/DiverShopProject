using DG.Tweening;
using Player;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Shop : GenericBuild
{
    [SerializeField] private Transform foodContainer;
    [SerializeField] private GameObject dollarContainer;
    [SerializeField] private GameObject dollarPrefab;
    private List<Food> foodInShop = new List<Food>();
    private List<GameObject> dollarsInShop = new List<GameObject>();
    private Vector3 foodPos;
    private ushort currentPrice;
    private Transform currentCustomer;
    // Food pack
    private float offsetXFood = 0.5f;
    private float offsetYFood = 0.2f;
    private float offsetZFood = 0.5f;
    private int widthLimitFood = 3;
    private int lengthLimitFood = 4;
    private float lengthFood = 0;
    private float heightFood = 0;
    private float widthFood = 0;
    // DollarPack
    private float offsetXDollar = 0.4f;
    private float offsetYDollar = 0.1f;
    private float offsetZDollar = 0.2f;
    private int widthLimitDollar = 3;
    private int lengthLimitDollar = 4;
    private float lengthDollar = 0;
    private float heightDollar = 0;
    private float widthDollar = 0;

    private ushort friedFishPrice = 5;
    private ushort sandwichPrice = 10;
    private ushort fishburgerPrice = 15;
    public void GetFoodFromPlayer()
    {
        foodInShop = playerInventory.SetFoodToShop();
        PlayerLogic.isBusyHands = false;
        float counter = 0.2f;
        foreach (var food in foodInShop)
        {
            counter += 0.1f;
            foodPos = CalculateFoodPosition();
            //food.transform.parent = null;
            food.transform.DOJump(foodPos, 3f, 1, counter).OnComplete(() => food.transform.parent = foodContainer);
            CalculateNewPosition();
        }
        PlayerAnimatorFXController.OnPlayerHandsFree.Invoke();
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CalculateFoodPosition();
        }
    }

    private Vector3 CalculateFoodPosition()
    {
        Vector3 spawnPosition = foodContainer.position + new Vector3(lengthFood * offsetXFood, widthFood * offsetYFood, -(heightFood * offsetZFood));
        return spawnPosition;
    }

    private void CalculateNewPosition()
    {
        lengthFood++;
        if (lengthFood > widthLimitFood)
        {
            lengthFood = 0;
            heightFood++;
            if (heightFood > lengthLimitFood)
            {
                lengthFood = 0;
                heightFood = 0;
                widthFood++;
            }
        }
    }

    public void SetProductToCustomer(CustomerBehaviour customer)
    {
        if (foodInShop.Count > 0)
        {
            Food lastFood = foodInShop[foodInShop.Count - 1];
            customer.SetFood(lastFood);
            lastFood.transform.DOJump(customer.transform.position, 2f, 1, 0.25f).OnComplete(() => Transaktion(customer, lastFood));
            if (foodInShop.Count == 1)
            {
                ResetFoodPosition();
            }
        }
        else
        {
            customer.NoneFood();
        }
    }

    private void ResetFoodPosition()
    {
        lengthFood = 0f;
        widthFood = 0f;
        heightFood = 0f;
    }

    private void Transaktion(CustomerBehaviour customer, Food food)
    {
        currentCustomer = customer.transform;
        foodInShop.Remove(food);
        customer.GiveMoney(CheckPrice(food));
        Destroy(food.gameObject);
    }

    private ushort CheckPrice(Food food)
    {
        switch (food.typeOfFood)
        {
            case TypeOfFood.FriedFish:
                currentPrice = friedFishPrice;
                return currentPrice;
            case TypeOfFood.Sandwich:
                currentPrice = sandwichPrice;
                return currentPrice;
            case TypeOfFood.Fishburger:
                currentPrice = fishburgerPrice;
                return currentPrice;
            default:
                return currentPrice;
        }
    }

    public void InstantDollar()
    {
        var dollar = Instantiate(dollarPrefab, currentCustomer.position, dollarPrefab.transform.rotation, dollarContainer.transform);
        dollarsInShop.Add(dollar);
        var dollarPos = ChooseDollarPos();
        dollar.transform.DOJump(dollarPos, 3f, 1, 0.2f)/*.OnComplete(() => food.transform.parent = foodContainer)*/;
        CalculateNewDollarPosition();
    }

    private Vector3 ChooseDollarPos()
    {
        Vector3 spawnPosition = dollarContainer.transform.position + new Vector3(lengthDollar * offsetXDollar, 
                                                                                 (widthDollar * offsetYDollar) - 0.15f, 
                                                                                 -(heightDollar * offsetZDollar));
        return spawnPosition;
    }

    private void CalculateNewDollarPosition()
    {
        lengthDollar++;
        if (lengthDollar > widthLimitDollar)
        {
            lengthDollar = 0;
            heightDollar++;
            if (heightDollar > lengthLimitDollar)
            {
                lengthDollar = 0;
                heightDollar = 0;
                widthDollar++;
            }
        }
    }

    public void SetDollarsForPlayer()
    {
        float jumpDuration = 0.1f;
        foreach (var dollar in dollarsInShop)
        {
            dollar.transform.DOJump(playerInventory.transform.position, 2.5f, 1, jumpDuration).OnComplete(() => TransitDollarToPlayer(dollar));
            jumpDuration += 0.01f;
        }
        dollarsInShop.Clear();
        ResetDollarsPos();
    }

    private void TransitDollarToPlayer(GameObject _dollar)
    {
        //playerInventory.SetDollarsToInventory(1);
        PlayerInventoryPresenter.OnCurrentDollarsChanged.Invoke(1);
        dollarsInShop.Remove(_dollar);
        Destroy(_dollar);
    }

    private void ResetDollarsPos()
    {
        lengthDollar = 0f;
        heightDollar = 0f;
        widthDollar = 0f;
    }
}