﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;

public class KitchenScript : MonoBehaviour
{
    private List<Food> foodPrepared;
    private List<Food> orders;
    private List<Food> bills;

	// Use this for initialization
	void Start ()
    {
        foodPrepared = new List<Food>();
        orders = new List<Food>();
        bills = new List<Food>();
	}
	
	// Update is called once per frame
	void Update ()
    {	
	}

    public bool IsFoodPrepared()
    {
        return foodPrepared.Count > 0;
    }

    public Food GetFoodPrepared()
    {
        Food plate = foodPrepared[0];
        foodPrepared.RemoveAt(0);
        bills.Add(plate);
        return plate;
    }

    public void AddFoodPrepared(Food food)
    {
        foodPrepared.Add(food);
        orders.Remove(food);
    }

    public bool IsOrderPending()
    {
        return orders.Count > 0;
    }

    public Food GetOrder()
    {
        Food order = orders[0];
        orders.RemoveAt(0);
        return order;
    }

    public void AddOrder(Food order)
    {
        orders.Add(order);
    }

    public bool IsBillReady()
    {
        return bills.Count > 0;
    }

    public Food GetBill()
    {
        Food bill = bills[0];
        bills.RemoveAt(0);
        return bill;
    }
}
