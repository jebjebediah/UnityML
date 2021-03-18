using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;

public class CollectorAgentTrain : Agent
{
    // Start is called before the first frame update
    Rigidbody rb;
    RayPerceptionSensorComponent3D rays;
    public GameObject collectablePrefab;
    public GameObject localOrigin;
    public int numCoins;
    List<GameObject> coins;
    public CoinManager coinManager;
    public float forceMultiplier = 10;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rays = GetComponent<RayPerceptionSensorComponent3D>();
    }

    public override void OnEpisodeBegin()
    {
        // If the Agent fell, zero its momentum
        if (this.transform.localPosition.y < 0)
        {
            this.rb.angularVelocity = Vector3.zero;
            this.rb.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3( 0, 0.5f, 0);
        }

        // Delete remaining coins if any are left at end of episode
        GameObject[] coinsToDelete = GameObject.FindGameObjectsWithTag("Collectable");
        for (int i = 0; i < coinsToDelete.Length; i++) {
            if (coinsToDelete[i].transform.parent == localOrigin.transform) {
                Destroy(coinsToDelete[i]);
            }
        }
        
        // Spawn new coins at beginning of episode
        for (int i = 0; i < numCoins; i++) {
            Vector3 toCheck = new Vector3((UnityEngine.Random.value - UnityEngine.Random.value) * 10, 0.6f, (UnityEngine.Random.value - UnityEngine.Random.value) * 10) + localOrigin.transform.position;
            while (Physics.CheckSphere(toCheck, 0.5f)) {
                toCheck = new Vector3((UnityEngine.Random.value - UnityEngine.Random.value) * 10, 0.6f, (UnityEngine.Random.value - UnityEngine.Random.value) * 10) + localOrigin.transform.position;
            }
            Instantiate(collectablePrefab, toCheck, Quaternion.identity, localOrigin.transform);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent position
        sensor.AddObservation(this.transform.localPosition);

        // Agent Velocity
        sensor.AddObservation(this.rb.velocity.x);
        sensor.AddObservation(this.rb.velocity.z);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        //Perform Movement
        MoveAgent(actionBuffers.DiscreteActions);

        // Detect if fallen off of platform
        if (this.transform.localPosition.y < -3)
        {
            EndEpisode();
        }
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = act[0];

        // Agent can go forward or backward, or turn left and right, as discrete actions
        switch (action)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
        }
        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        rb.AddForce(dirToGo * forceMultiplier, ForceMode.VelocityChange);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;
        if (Input.GetKey(KeyCode.LeftArrow))
        { 
            discreteActionsOut[0] = 4;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        { 
            discreteActionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        { 
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        { 
            discreteActionsOut[0] = 2;
        }
    }

    public void OnTriggerEnter(Collider collision)
    {
        // Penalize contact with the obstacles
        if (collision.gameObject.tag == "Collectable") {
            Destroy(collision.gameObject);  
            AddReward(1.0f / (float)numCoins);

            // Decrement number of coins in the coin manager
            coinManager.checkIfOver();
        }
    }
}