﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;
using FluentBehaviorTree;

[System.Serializable]
public class Inventory
{
    public Food food;
    public Food bill;
    public bool money;
    public Food order;
    public Ingredients ingredients;

    public Inventory()
    {
        food = new Food();
        bill = new Food();
        money = false;
        order = new Food();
        ingredients = new Ingredients();
    }

    public bool Has(ItemType type)
    {
        switch (type)
        {
            case ItemType.FOOD: return this.food.food != FoodType.NONE;
            case ItemType.BILL: return this.bill.table != null;
            case ItemType.MONEY: return this.money;
            case ItemType.ORDER: return this.order.food != FoodType.NONE;
            case ItemType.INGREDIENTS: return this.ingredients.food != FoodType.NONE;
            default: return false;
        }
    }

    public Status GetFrom(ItemType type, Inventory entity)
    {
        return entity.GiveTo(type, this);
    }

    public Status GiveTo(ItemType type, Inventory entity)
    {
        if (!Has(type)) return Status.FAILURE;
        switch (type)
        {
            case ItemType.FOOD:
                entity.food = this.food;
                this.food = new Food();
                break;
            case ItemType.BILL:
                entity.bill = this.bill;
                this.bill = new Food();
                break;
            case ItemType.MONEY:
                entity.money = this.money;
                this.money = false;
                break;
            case ItemType.ORDER:
                entity.order = this.order;
                this.order = new Food();
                break;
        }
        return Status.SUCCESS;
    }
}
