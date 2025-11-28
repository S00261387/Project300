using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;



public struct ProjectileProperties
{
    public Vector3 direction;
    public Vector3 initialPosition;
    public float initialSpeed;
    public float mass;
    public float drag;
}

public class PlayerThrow : MonoBehaviour
{
    public TrajectoryPredictor trajectoryPredictor;
    public Transform attackPoint;
    public GameObject objectToThrow;

    public int totalThrows;
    public float throwCooldown;

    public float throwForce;
    //public float throwUpwardForce;

    bool readyToThrow;

    private void Awake()
    {
        trajectoryPredictor = GetComponent<TrajectoryPredictor>();
        readyToThrow = true;
    }

    private void Update()
    {
        //Predict();
    }

    //void Predict()
    //{
    //    trajectoryPredictor.PredictTrajectory(ProjectileData());
    //}

    //ProjectileProperties ProjectileData()
    //{
    //    ProjectileProperties properties = new ProjectileProperties();
    //    Rigidbody r = objectToThrow.GetComponent<Rigidbody>();

    //    properties.direction = attackPoint.forward;
    //    properties.initialPosition = attackPoint.position;
    //    properties.initialSpeed = throwForce;
    //    properties.mass = r.mass;
    //    properties.drag = r.linearDamping;

    //    return properties;
    //}

    public void OnThrow(InputAction.CallbackContext obj)
    {
        readyToThrow = false;
        GameObject projectile = Instantiate(objectToThrow, attackPoint.position, gameObject.transform.rotation);
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
        Vector3 forceToAdd = gameObject.transform.forward * throwForce;
           // + transform.up * throwUpwardForce
        projectileRb.AddForce(forceToAdd, ForceMode.Impulse);
        totalThrows--;

        Invoke(nameof(ResetThrow), throwCooldown);
    }

    private void ResetThrow()
    {
        readyToThrow = true;
    }
}
