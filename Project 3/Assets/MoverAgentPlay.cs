﻿using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;

public class MoverAgentPlay : Agent
{
    // Start is called before the first frame update
    Rigidbody rb;
    float episodeTime;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update() {
        episodeTime += Time.deltaTime;

        // Penalize getting stuck for a long time (usually due to getting stuck on the obstacles)
        if (episodeTime > 60) { 
            SetReward(-1f);
            EndEpisode(); 
        }
    }

    public Transform Target;
    public Transform Obstacle1;
    public Transform Obstacle2;
    public override void OnEpisodeBegin()
    {
        // If the Agent fell, zero its momentum
        if (this.transform.localPosition.y < 0)
        {
            this.rb.angularVelocity = Vector3.zero;
            this.rb.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3( 0, 0.5f, 0);
        }

        this.episodeTime = 0.0f;

        this.transform.localPosition = new Vector3(UnityEngine.Random.value * 8 - 4, 0.5f, UnityEngine.Random.value * 8 - 4);

        //Move the obstacles to new spots
        Obstacle1.localPosition = new Vector3(UnityEngine.Random.value * 8 - 4, 0.5f, UnityEngine.Random.value * 8 - 4);
        Obstacle2.localPosition = new Vector3(UnityEngine.Random.value * 8 - 4, 0.5f, UnityEngine.Random.value * 8 - 4);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target amd Agent positions
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        // Agent Velocity
        sensor.AddObservation(this.rb.velocity.x);
        sensor.AddObservation(this.rb.velocity.z);

        // Obstacle Positions
        sensor.AddObservation(Obstacle1.localPosition);
        sensor.AddObservation(Obstacle2.localPosition);
    }
    
    public float forceMultiplier = 10;
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        rb.AddForce(controlSignal * forceMultiplier);

        // Rewards
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

        float distanceToObstacle = Math.Min(Vector3.Distance(this.transform.localPosition, Obstacle1.localPosition), Vector3.Distance(this.transform.localPosition, Obstacle2.localPosition));

        // Reached target
        if (distanceToTarget < 1.42f)
        {
            SetReward(1.0f);
            EndEpisode();
        }

        // Fell off platform
        else if (this.transform.localPosition.y < 0)
        {
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    IEnumerator<UnityEngine.WaitForSeconds> waitAtBegin()
    {
        yield return new WaitForSeconds(1);
        Debug.Log("Wait over");
    }

    public void OnColisionEnter(Collision collision)
    {
        // Penalize contact with the obstacles
        if (collision.gameObject.name == "Obstacle1" || collision.gameObject.name == "Obstacle2") {
            AddReward(-0.25f);
        }
    }
}