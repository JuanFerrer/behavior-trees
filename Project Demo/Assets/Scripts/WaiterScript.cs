﻿using UnityEngine;
using FluentBehaviorTree;
using UnityEngine.AI;
using Data;

public class WaiterScript : MonoBehaviour
{
    GameManagerScript gm;                   // Game manager
    NavMeshAgent agent;                     // Navigation agent
    BehaviorTree bt;                        // Behaviour tree
    BehaviorTree bringOrderSequence;        // Subtree for getting the order to the kitchen
    BehaviorTree bringFoodToTableSequence;  // Subtree for giving food to customer
    BehaviorTree getFoodSequence;           // Subtree for fetching food from kitchen
    BehaviorTree receiveCustomerSequence;   // Subtree for receiving customer
    BehaviorTree getBillSequence;           // Subtree for fetching bill from kitchen
    BehaviorTree bringBillSequence;         // Subtree for giving bill to customer
    BehaviorTree attendCustomerSequence;    // Subtree for attending waiting customer
    QueueScript queue;                      // Queue
    KitchenScript kitchen;                  // Kitchen
    TableScript table;                      // Usually empty. Set when going to a certain table
    CustomerScript customer;                // Usually empty. Set when interacting with a customer
    BlackboardScript blackboard;            // Global information
    ThoughtScript thought;                  // Thought
    public Inventory Inventory;             // Inventory of entity

    private bool isRecalculatingTree = false;
    private bool alreadyGoingSomewhereThisFrame = false;

    /// <summary>
    /// Use Unity's meshnav to travel to given position
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private Status GoTo(Vector3 pos)
    {
        if (alreadyGoingSomewhereThisFrame)
        {
            return Status.FAILURE;
        }
        alreadyGoingSomewhereThisFrame = true;
        if (agent.destination != transform.position && !CloseEnough(pos, agent.destination) && !CloseEnough(transform.position, agent.destination))
        {
             agent.ResetPath();
            return Status.RUNNING;
        }

        // Set a new destination
        if (agent.isStopped)
        {
            agent.SetDestination(pos);
            agent.isStopped = false;
            return Status.RUNNING;
        }

        bool reachedPos = !agent.pathPending && (agent.remainingDistance <= agent.stoppingDistance) && (!agent.hasPath || Mathf.Approximately(agent.velocity.sqrMagnitude, 0.0f));
        bool onTheWay = agent.remainingDistance != Mathf.Infinity && (agent.remainingDistance > agent.stoppingDistance);

        if (reachedPos)
        {
            Debug.Log("Waiter reached destination: " + pos.ToString("G2"));
            agent.isStopped = true;
            return Status.SUCCESS;
        }
        else if (onTheWay) return Status.RUNNING;
        else return Status.FAILURE;
    }

    /// <summary>
    /// Get position from object and call overloaded GoTo
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private Status GoTo(MonoBehaviour obj)
    {
        Vector3 pos = obj.transform.position;
        if (obj.tag != "Queue" && blackboard.WaiterTakingCareOfQueue == this) blackboard.StopTakingCareOfQueue(this);
        if (obj.tag != "Customer" && obj.tag != "Table")
        {
            if (customer != null)
            {
                customer = null;
            }
            if (table != null)
            {
                table.HasWaiterEnRoute = false;
                table = null;
            }
        }

        if (obj.tag == "Customer")
        {
            var c = obj.GetComponent<CustomerScript>();
            if (!c.IsWaiting || c.IsLeaving)
            {
                customer = null;
                table = null;
                table.HasWaiterEnRoute = false;
                return Status.FAILURE;
            }
        }
        return GoTo(pos);
    }

    private Status ShowThought(ThoughtType thoughtType)
    {
        if (!thought.IsShowing)
        {
            thought.Show(thoughtType);
        }
        return Status.SUCCESS;
    }

    /// <summary>
    /// Check if waiter is close enough to object
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private bool CloseEnough(Vector3 obj1, Vector3 obj2)
    {
        return (obj1 - obj2).sqrMagnitude <= (agent.stoppingDistance * agent.stoppingDistance) + 2.0f;
    }

    /// <summary>
    /// Transfer order from entity's inventory to kitchen
    /// </summary>
    /// <returns></returns>
    private Status GiveOrderToKitchen()
    {
        if (!CloseEnough(transform.position, kitchen.transform.position))
        {
            isRecalculatingTree = true;
            return Status.FAILURE;
        }
        if (!Inventory.order.table.Customer.IsWaiting)
        {
            Inventory.order = new Food();
            return Status.FAILURE;
        }
        kitchen.AddOrder(Inventory.order);
        Inventory.order = new Food();
        Debug.Log("Waiter left order in kitchen");
        return Status.SUCCESS;
    }

    /// <summary>
    /// Get prepared food from kitchen's inventory
    /// </summary>
    /// <returns></returns>
    private Status GetFoodFromKitchen()
    {
        // Since the players is doing something else, let's reset everything we can reset here
        if (table != null) table.HasWaiterEnRoute = false;
        customer = null;
        table = null;
        if (blackboard.WaiterTakingCareOfQueue == this) blackboard.StopTakingCareOfQueue(this);

        if (!CloseEnough(transform.position, kitchen.transform.position))
        {
            isRecalculatingTree = true;
            return Status.FAILURE;
        }
        Inventory.food = kitchen.GetFoodPrepared();
        table = Inventory.food.table;
        if (!Inventory.food.table.Customer.IsWaiting)
        { 
            // Customer left, throw this away
            // TODO: Maybe can reuse?
            Inventory.food = new Food();
            table = null;
            return Status.FAILURE;
        }
        Debug.Log("Waiter got food from kitchen");
        return Status.SUCCESS;
    }

    /// <summary> 
    /// 
    /// </summary>
    /// <returns></returns>
    private Status ServeFood()
    {
        if (!CloseEnough(transform.position, Inventory.food.table.Customer.transform.position))
        {
            isRecalculatingTree = true;
            return Status.FAILURE;
        }
        if (!Inventory.food.table.Customer.IsWaiting)
        {
            Inventory.food = new Food();
            return Status.FAILURE;
        }
        Inventory.food.table.Customer.Serve();
        Inventory.GiveTo(ItemType.FOOD, Inventory.food.table.Customer.Inventory);
        Debug.Log("Waiter served food");
        return Status.SUCCESS;
    }

    /// <summary>
    /// Get bill from kitchen's inventory
    /// </summary>
    /// <returns></returns>
    private Status GetBillFromKitchen()
    {
        // Reset everything that can be reset, since we might have stopped doing something else
        if (table != null) table.HasWaiterEnRoute = false;
        customer = null;
        table = null;
        if (blackboard.WaiterTakingCareOfQueue == this) blackboard.StopTakingCareOfQueue(this);

        if (!CloseEnough(transform.position, kitchen.transform.position))
        {
            isRecalculatingTree = true;
            return Status.FAILURE;
        }
        Inventory.bill = kitchen.GetBill();
        table = Inventory.bill.table;
        if (!Inventory.bill.table.Customer.IsWaiting)
        {
            // Customer left... Well, destroy the bill
            Inventory.bill = new Food();
            table = null;
            return Status.FAILURE;
        }
        Debug.Log("Waiter got bill from kitchen");
        return Status.SUCCESS;
    }

    private Status GiveBillToCustomer()
    {
        if (!CloseEnough(transform.position, Inventory.bill.table.Customer.transform.position))
        {
            isRecalculatingTree = true;
            return Status.FAILURE;
        }
        if (!Inventory.bill.table.Customer.IsWaiting)
        {
            // Customer left... Well, destroy the bill
            Inventory.bill = new Food();
            table = null;
            return Status.FAILURE;
        }
        Inventory.bill.table.Customer.BringBill();
        Inventory.GiveTo(ItemType.BILL, Inventory.bill.table.Customer.Inventory);
        Debug.Log("Waiter gave bill to customer");
        return Status.SUCCESS;
    }

    /// <summary>
    /// Check what customer has not been attended yet
    /// </summary>
    /// <returns></returns>
    private Status GetCustomerToAttend()
    {
        if (blackboard.WaiterTakingCareOfQueue == this) blackboard.StopTakingCareOfQueue(this);

        if (table == null && customer == null)
        {
            table = blackboard.GetTableToAttend();
            customer = table.Customer;
        }
        return Status.SUCCESS;
    }

    /// <summary>
    /// Attend customer and get their order
    /// </summary>
    /// <returns></returns>
    private Status AttendCustomer()
    {
        if (!CloseEnough(transform.position, customer.transform.position))
        {
            isRecalculatingTree = true;
            return Status.FAILURE;
        }
        if (!customer.IsWaiting)
        {
            customer = null;
            return Status.FAILURE;
        }
        Inventory.GetFrom(ItemType.ORDER, customer.Inventory);
        customer.Attend();
        table.HasWaiterEnRoute = false;

        // Now that the customer has been attended, prepare for next customer
        customer = null;
        table = null;

        Debug.Log("Waiter attended customer");
        return Status.SUCCESS;
    }

    /// <summary>
    /// Get an empty table and send the first customer from the queue to that table
    /// </summary>
    /// <returns></returns>
    private Status SendCustomerToTable()
    {
        if (!CloseEnough(transform.position, queue.transform.position))
        {
            isRecalculatingTree = true;
            return Status.FAILURE;
        }
        Debug.Log("Waiter sent customer to table");
        table = blackboard.GetEmptyTable();
        customer = queue.GetNextCustomer();
        customer.Receive(table);
        table.IsAssigned = true;

        // Now that the customer has been received, so prepare for next customer
        customer = null;
        table = null;

        // And, if there's no customers left (only the one that's being received right now), tell everyone you're not in the queue anymore
        if (blackboard.WaiterTakingCareOfQueue == this && queue.CustomerCount() == 0) blackboard.StopTakingCareOfQueue(this);

        return Status.SUCCESS;
    }

    private Status SetGoingToQueue()
    {
        // Reset everything that can be reset, since we might have stopped doing something else
        if (table != null) table.HasWaiterEnRoute = false;
        customer = null;
        table = null;

        blackboard.SetTakingCareOfQueue(this);
        return Status.SUCCESS;
    }

    /// <summary>
    /// Clean the table after client left
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    private Status CleanTable(TableScript table)
    {
        Debug.Log("Waiter cleaned table");
        var tableScript = table.GetComponent<TableScript>();
        tableScript.Clean();
        return tableScript.IsClean ? Status.SUCCESS : Status.FAILURE;
    }

    // Use this for initialization
    void Start()
    {
        // TODO: Get references to each object
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManagerScript>();
        kitchen = gm.kitchen;
        queue = gm.queue;
        blackboard = gm.blackboard;
        thought = GetComponentInChildren<ThoughtScript>();
        agent = GetComponent<NavMeshAgent>();
        agent.isStopped = true;
        Inventory = new Inventory();

        // Do we have a customer's order? We should be taking that to the kitchen
        bringOrderSequence = new BehaviorTreeBuilder("BringOrderSequence")
            .Sequence("BringOrder")
                .Sequence("ShouldBringOrder")
                    .If("HasOrder", () => { return Inventory.Has(ItemType.ORDER); })
                    .Not("IsNotRecalculating")
                        .If("Recalculating", () => { return isRecalculatingTree; })
                    .End()
                .Do("ShowThought", () => { return ShowThought(ThoughtType.ORDER); })
                .Do("GoToKitchen", () => { return GoTo(kitchen); })
                .Do("GiveOrderToKitchen", GiveOrderToKitchen)
                .End()
            .End();

        // We need to keep dishes warm (and we have one in our hands!), so do that now
        bringFoodToTableSequence = new BehaviorTreeBuilder("BringFoodToTableSequence")
            .Sequence("BringFoodToTable")
                .Sequence("ShouldServeFood")
                    .If("HasFood", () => { return Inventory.Has(ItemType.FOOD); })
                    .Not("IsNotRecalculating")
                        .If("Recalculating", () => { return isRecalculatingTree; })
                    .End()
                .Do("ShowThought", () => { return ShowThought(ThoughtType.FOOD); })
                .Do("GoToTable", () => { return GoTo(Inventory.food.table.Customer); })
                .Do("GiveFoodToCustomer", ServeFood)
                .End()
            .End();

        // We need to keep dishes warm, so do that now
        getFoodSequence = new BehaviorTreeBuilder("GetFoodSequence")
           .Sequence("GetFood")
                .Sequence("ShouldServeFood")
                    .If("IsFoodPrepared", kitchen.IsFoodPrepared)
                    .Not("DoesNotHaveFood")
                        .If("HasFood", () => { return Inventory.Has(ItemType.FOOD); })
                    .Not("IsNotRecalculating")
                        .If("Recalculating", () => { return isRecalculatingTree; })
                    .End()
                .Do("ShowThought", () => { return ShowThought(ThoughtType.FOOD); })
                .Do("GoToKitchen", () => { return GoTo(kitchen); })
                .Do("PickupFood", GetFoodFromKitchen)
                .End()
            .End();

        // We have no food ready to be served
        // There needs to be free tables before we receive a new customer
        // Also, make sure someone is in the queue
        receiveCustomerSequence = new BehaviorTreeBuilder("ReceiveCustomerSequence")
            .Sequence("ReceiveCustomer")
                .Sequence("ShouldReceiveCustomer")
                    .If("NoOneOrMeIsTakingCareOfQueue", () => { return blackboard.WaiterTakingCareOfQueue == null || blackboard.WaiterTakingCareOfQueue == this; })
                    .If("AreThereEmptyTables", () => { return blackboard.EmptyTablesCount > 0; })
                    .If("AreThereCustomersInQueue", () => { return queue.CustomerCount() > 0; })
                    .Not("IsNotRecalculating")
                        .If("Recalculating", () => { return isRecalculatingTree; })
                    .End()
                .Do("ShowThought", () => { return ShowThought(ThoughtType.QUEUE); })
                .Do("SetGoingToQueue", SetGoingToQueue)
                .Do("GoToQueue", () => { return GoTo(queue); })
                .Do("SendCustomerToTable", SendCustomerToTable)
                .End()
            .End();

        // Someone is already receiving customers or there is noone to receive
        // Bring bill to customers, lest they leave without paying
        // STOP THE SINPA! (https://en.wiktionary.org/wiki/sinpa)
        getBillSequence = new BehaviorTreeBuilder("GetBillSequence")
            .Sequence("GetBill")
                .Sequence("ShouldGetBill")
                    .If("IsBillReady", kitchen.IsBillReady)
                    .Not("DoesNotHaveBill")
                        .If("HasBill", () => { return Inventory.Has(ItemType.BILL); })
                    .Not("DoesNotHaveFood")
                        .If("HasFood", () => { return Inventory.Has(ItemType.FOOD); })
                    .Not("IsNotRecalculating")
                        .If("Recalculating", () => { return isRecalculatingTree; })
                    .End()
                .Do("ShowThought", () => { return ShowThought(ThoughtType.BILL); })
                .Do("GoToKitchen", () => { return GoTo(kitchen); })
                .Do("PickupBill", GetBillFromKitchen)
                .End()
            .End();

        // We have the bill and need to take it to the customer
        bringBillSequence = new BehaviorTreeBuilder("BringBillSequence")
            .Sequence("BringBill")
                .Sequence("ShouldBringBill")
                    .If("HasBill", () => { return Inventory.Has(ItemType.BILL); })
                    .Not("IsNotRecalculating")
                        .If("Recalculating", () => { return isRecalculatingTree; })
                    .End()
                .Do("ShowThought", () => { return ShowThought(ThoughtType.BILL); })
                .Do("GoToTable", () => { return GoTo(Inventory.bill.table.Customer); })
                .Do("GiveBillToCustomer", GiveBillToCustomer)
                .Do("GetMoneyFromCustomer", () => { return Inventory.GetFrom(ItemType.MONEY, customer.Inventory); })
                .Do("CleanTable", () => { return CleanTable(table); })
                .End()
            .End();

        // Since the bills have been taken care of, check if any sitting customer
        // is still to be attended
        attendCustomerSequence = new BehaviorTreeBuilder("AttendCustomerSequence")
            .Sequence("AttendCustomer")
                .Sequence("ShouldAttendCustomer")
                    .If("AreThereCustomerToAttendOrAlreadyhaveOne", () =>
                    {
                        return blackboard.CustomersToAttendCount > 0 ||
                        (table!= null && table.HasWaiterEnRoute && !customer.HasBeenAttended);
                    })
                    .Not("IsNotRecalculating")
                        .If("Recalculating", () => { return isRecalculatingTree; })
                    .End()
                .Do("ShowThought", () => { return ShowThought(ThoughtType.CUSTOMER); })
                .Do("GetCustomerToAttend", GetCustomerToAttend)
                .Do("GoToCustomer", () => { return GoTo(customer); })
                .Do("Attend", AttendCustomer)
                .End()
            .End();

        // Declare BT
        bt = new BehaviorTreeBuilder("WaiterBT")
        .RepeatUntilFail("Loop")
            .Selector("Selector")
                .Do("BringOrderSequence", bringOrderSequence)
                .Do("BringFoodToTableSequence", bringFoodToTableSequence)
                .Do("BringBillSequence", bringBillSequence)
                .Do("GetFoodSequence", getFoodSequence)
                .Do("GetBillSequence", getBillSequence)
                .Do("ReceiveCustomerSequence", receiveCustomerSequence)
                .Do("AttendCustomerSequence", attendCustomerSequence)
                .Do("JustWait", () =>
                {
                    isRecalculatingTree = false;
                    return Status.SUCCESS;
                })
                .End()
        .End();
    }

    // Update is called once per frame
    void Update()
    {
        bt.Tick();
        alreadyGoingSomewhereThisFrame = false;
    }
}
