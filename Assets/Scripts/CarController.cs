using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    public enum ControlMode
    {
        MLAgent,
        Human,
        Scripted
    }

    [Header("Control Mode")]
    public ControlMode controlMode = ControlMode.MLAgent;
    public Rigidbody rigidbody;
    public Transform carCentreOfMassTransform;

    public enum DriveType { FrontWheel, RearWheel, AllWheel }

    [Header("Drive Settings")]
    public DriveType driveType = DriveType.FrontWheel;
    public float motorForce = 1200f;
    public float maxSteerAngle = 30f;
    public float brakeForce = 3000f;
    public float handbrakeForce = 4000f;
    [Range(0f, 1f)] public float frontBrakeBias = 0.6f;

    [Header("Assists")]
    public bool tractionControl = true;
    public float tractionControlSlipLimit = 0.3f;
    public bool abs = true;
    public float absSlipLimit = 0.25f;

    [Header("Anti-Roll Bars")]
    public bool useAntiRollBars = true;
    public float antiRollForce = 5000f;

    [Header("Wheel Colliders")]
    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;

    [Header("Wheel Visuals")]
    public Transform frontLeftTransform;
    public Transform frontRightTransform;
    public Transform rearLeftTransform;
    public Transform rearRightTransform;

    private float steerInput;
    private float throttleInput;
    private bool brakeInput;
    private bool handbrakeInput;

    private float currentSteerAngle;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();

        if (carCentreOfMassTransform != null)
            rigidbody.centerOfMass = carCentreOfMassTransform.localPosition;
        else
            rigidbody.centerOfMass = new Vector3(0f, -0.6f, 0f);

        SetupWheelFriction(frontLeftWheel);
        SetupWheelFriction(frontRightWheel);
        SetupWheelFriction(rearLeftWheel);
        SetupWheelFriction(rearRightWheel);
    }

    private void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void Update()
    {
        if (controlMode == ControlMode.Human)
            ReadHumanInput();
        else if (controlMode == ControlMode.Scripted)
            ReadScriptedInput();
    }

    private void FixedUpdate()
    {
        HandleSteering();
        HandleMotor();
        HandleBraking();

        if (useAntiRollBars)
            AntiRollBars();

        UpdateWheels();
    }

    public void SetInputs(float steer, float throttle, float brake, bool handbrake = false)
    {
        steerInput = Mathf.Clamp(steer, -1f, 1f);
        throttleInput = Mathf.Clamp(throttle, -1f, 1f);
        brakeInput = brake > 0.5f;
        handbrakeInput = handbrake;
    }

    private void ReadHumanInput()
    {
        SetInputs(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical"),
            Input.GetKey(KeyCode.Space) ? 1f : 0f,
            Input.GetKey(KeyCode.LeftShift)
        );
    }

    private void ReadScriptedInput()
    {
        SetInputs(0f, 0.6f, 0f);
    }
    private float GetWheelSlip(WheelCollider wheel)
    {
        if (wheel.GetGroundHit(out WheelHit hit))
            return Mathf.Abs(hit.forwardSlip);

        return 0f;
    }
    private float ApplyTractionControl(float torque)
    {
        float maxSlip = 0f;

        if (driveType != DriveType.RearWheel)
        {
            maxSlip = Mathf.Max(maxSlip, GetWheelSlip(frontLeftWheel));
            maxSlip = Mathf.Max(maxSlip, GetWheelSlip(frontRightWheel));
        }

        if (driveType != DriveType.FrontWheel)
        {
            maxSlip = Mathf.Max(maxSlip, GetWheelSlip(rearLeftWheel));
            maxSlip = Mathf.Max(maxSlip, GetWheelSlip(rearRightWheel));
        }

        if (maxSlip > tractionControlSlipLimit)
        {
            float excess = maxSlip - tractionControlSlipLimit;
            torque *= Mathf.Clamp01(1f - excess * 4f);
        }

        return torque;
    }

    private void HandleMotor()
    {
        float speed = rigidbody.linearVelocity.magnitude * 3.6f;
        float torqueLimiter = Mathf.Clamp01(1f - speed / 120f);
        float motor = throttleInput * motorForce * torqueLimiter;

        if (tractionControl)
            motor = ApplyTractionControl(motor);

        ApplyDriveTorque(motor);
    }

    private void ApplyDriveTorque(float motor)
    {
        switch (driveType)
        {
            case DriveType.FrontWheel:
                frontLeftWheel.motorTorque = motor;
                frontRightWheel.motorTorque = motor;
                break;

            case DriveType.RearWheel:
                rearLeftWheel.motorTorque = motor;
                rearRightWheel.motorTorque = motor;
                break;

            case DriveType.AllWheel:
                float split = motor * 0.5f;
                frontLeftWheel.motorTorque = split;
                frontRightWheel.motorTorque = split;
                rearLeftWheel.motorTorque = split;
                rearRightWheel.motorTorque = split;
                break;
        }
    }

    private void HandleSteering()
    {
        float speed = rigidbody.linearVelocity.magnitude;
        float speedFactor = Mathf.Lerp(1f, 0.3f, speed / 30f);

        currentSteerAngle = maxSteerAngle * steerInput * speedFactor;
        frontLeftWheel.steerAngle = currentSteerAngle;
        frontRightWheel.steerAngle = currentSteerAngle;
    }

    private void HandleBraking()
    {
        float brake = brakeInput ? brakeForce : 0f;
        float handbrake = handbrakeInput ? handbrakeForce : 0f;

        float front = brake * frontBrakeBias;
        float rear = brake * (1f - frontBrakeBias);

        frontLeftWheel.brakeTorque = front;
        frontRightWheel.brakeTorque = front;
        rearLeftWheel.brakeTorque = rear + handbrake;
        rearRightWheel.brakeTorque = rear + handbrake;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Reset car when driving manually
        if (controlMode == ControlMode.Human)
        {
            ResetCar(startPosition, startRotation);
        }
    }
    public float CurrentSteerAngle
    {
        get { return currentSteerAngle; }
    }

    public void ResetCar(Vector3 position, Quaternion rotation)
    {
        rigidbody.isKinematic = true;

        rigidbody.position = position;
        rigidbody.rotation = rotation;

        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;

        frontLeftWheel.motorTorque = 0f;
        frontRightWheel.motorTorque = 0f;
        rearLeftWheel.motorTorque = 0f;
        rearRightWheel.motorTorque = 0f;

        frontLeftWheel.brakeTorque = 0f;
        frontRightWheel.brakeTorque = 0f;
        rearLeftWheel.brakeTorque = 0f;
        rearRightWheel.brakeTorque = 0f;

        frontLeftWheel.steerAngle = 0f;
        frontRightWheel.steerAngle = 0f;

        steerInput = 0f;
        throttleInput = 0f;
        brakeInput = false;
        handbrakeInput = false;
        currentSteerAngle = 0f;

        rigidbody.isKinematic = false;
    }

    private void SetupWheelFriction(WheelCollider wheel)
    {
        var f = wheel.forwardFriction;
        f.extremumSlip = 0.4f;
        f.extremumValue = 1.2f;
        f.asymptoteSlip = 0.8f;
        f.asymptoteValue = 0.8f;
        f.stiffness = 2f;
        wheel.forwardFriction = f;

        var s = wheel.sidewaysFriction;
        s.extremumSlip = 0.2f;
        s.extremumValue = 1.1f;
        s.asymptoteSlip = 0.5f;
        s.asymptoteValue = 0.75f;
        s.stiffness = 2.2f;
        wheel.sidewaysFriction = s;
    }

    private void UpdateWheels()
    {
        UpdateWheelPose(frontLeftWheel, frontLeftTransform);
        UpdateWheelPose(frontRightWheel, frontRightTransform);
        UpdateWheelPose(rearLeftWheel, rearLeftTransform);
        UpdateWheelPose(rearRightWheel, rearRightTransform);
    }

    private void UpdateWheelPose(WheelCollider collider, Transform t)
    {
        collider.GetWorldPose(out Vector3 pos, out Quaternion rot);
        t.position = pos;
        t.rotation = rot;
    }

    private void AntiRollBars()
    {
        ApplyAntiRoll(frontLeftWheel, frontRightWheel);
        ApplyAntiRoll(rearLeftWheel, rearRightWheel);
    }

    private void ApplyAntiRoll(WheelCollider left, WheelCollider right)
    {
        float leftTravel = 1f;
        float rightTravel = 1f;

        if (left.GetGroundHit(out WheelHit leftHit))
            leftTravel = (-left.transform.InverseTransformPoint(leftHit.point).y - left.radius) / left.suspensionDistance;

        if (right.GetGroundHit(out WheelHit rightHit))
            rightTravel = (-right.transform.InverseTransformPoint(rightHit.point).y - right.radius) / right.suspensionDistance;

        float force = (leftTravel - rightTravel) * antiRollForce;

        if (left.isGrounded)
            rigidbody.AddForceAtPosition(left.transform.up * -force, left.transform.position);

        if (right.isGrounded)
            rigidbody.AddForceAtPosition(right.transform.up * force, right.transform.position);
    }
}