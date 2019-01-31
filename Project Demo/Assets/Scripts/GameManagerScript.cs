﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{

	public KitchenScript kitchen;
    public QueueScript queue;
	public GameObject exit;
    public GameObject customerPrefab;
    private new GameObject camera;

    private CustomerScript customer;

    public float LeftMostCamPos;
    public float RightMostCamPos;
    public float CamSpeed;
    public BlackboardScript blackboard;
    public float CustomerSpawnCounter;

	// Use this for initialization
	void Start ()
	{
        camera = GameObject.FindGameObjectWithTag("MainCamera");
        queue = GameObject.FindGameObjectWithTag("Queue").GetComponent<QueueScript>();
        kitchen = GameObject.FindGameObjectWithTag("Kitchen").GetComponent<KitchenScript>();
        camera = GameObject.FindGameObjectWithTag("MainCamera");
        blackboard = GetComponent<BlackboardScript>();
        SpawnCustomer();
    }

    void SpawnCustomer()
    {
        var newPos = new Vector3(Random.Range(0.0f, -30.0f), 0.0f, Random.Range(-10.0f, 10.0f));
        GameObject.Instantiate(customerPrefab, newPos, Quaternion.identity);
        CustomerSpawnCounter = Random.Range(2f, 20f);
    }
	
	// Update is called once per frame
	void Update ()
	{
        CustomerSpawnCounter -= Time.deltaTime;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            if (camera.transform.position.x < LeftMostCamPos)
            {
                camera.transform.Translate(-CamSpeed * Time.deltaTime, 0.0f, 0.0f);
            }
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            if (camera.transform.position.x > RightMostCamPos)
            {
                camera.transform.Translate(CamSpeed * Time.deltaTime, 0.0f, 0.0f);
            }
        }

        // DEBUG
        if (Input.GetKeyDown(KeyCode.Space) || CustomerSpawnCounter <= 0)
        {
            SpawnCustomer();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Preparing food");
            kitchen.AddFoodPrepared(kitchen.GetOrder()); 
        }
    }
}
