using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class CarAgent : Agent
{
    public int CurrentLap = 0;
    private CarHUD hud;

    [Header("References")]
    public CarController carController;
    public CarState carState;

    private Rigidbody rigidbody;   // ? named consistently
    private Vector3 startPos;
    private Quaternion startRot;

    public override void Initialize()
    {
        rigidbody = GetComponent<Rigidbody>();
        startPos = transform.position;
        startRot = transform.rotation;

        hud = GetComponent<CarHUD>();
        if (hud == null)
            hud = FindFirstObjectByType<CarHUD>();
    }

    public override void OnEpisodeBegin()
    {
        carController.ResetCar(startPos, startRot);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 1) Speed
        sensor.AddObservation(rigidbody.linearVelocity.magnitude / 20f);   // ? was rb

        // 2–3) Local velocity
        Vector3 localVel = transform.InverseTransformDirection(rigidbody.linearVelocity);  // ? was rb
        sensor.AddObservation(localVel.x / 10f);
        sensor.AddObservation(localVel.z / 10f);

        // 4–8) Ray distances
        sensor.AddObservation(carState.rayDistances);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float steer = actions.ContinuousActions[0];
        float throttle = actions.ContinuousActions[1];
        float brake = actions.ContinuousActions[2];

        // Dead zone
        if (Mathf.Abs(steer) < 0.05f) steer = 0f;
        if (Mathf.Abs(throttle) < 0.05f) throttle = 0f;
        if (Mathf.Abs(brake) < 0.05f) brake = 0f;

        carController.SetInputs(steer, throttle, brake);

        // =====================
        // REWARD SYSTEM
        // =====================
        float forwardSpeed = Vector3.Dot(rigidbody.linearVelocity, transform.forward);
        AddReward(forwardSpeed * 0.0015f);
        AddReward(-Mathf.Abs(LocalLateralSpeed()) * 0.002f);
        AddReward(-0.0005f);

        // =====================
        // UPDATE HUD
        // =====================
        if (hud != null)
            hud.SetReward(GetCumulativeReward());
    }

    private float LocalLateralSpeed()
    {
        Vector3 localVel = transform.InverseTransformDirection(rigidbody.linearVelocity);  // ? was rb
        return localVel.x;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            AddReward(-2f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;
        actions[0] = Input.GetAxis("Horizontal");
        actions[1] = Mathf.Max(0f, Input.GetAxis("Vertical"));
        actions[2] = Input.GetKey(KeyCode.Space) ? 1f : 0f;
    }
}