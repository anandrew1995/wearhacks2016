/* InstantVR walking
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.2.4
 * date: January 29, 2016
 * 
 * - Fixed collisions while walking
 * - Added walking speed setting
 */

using UnityEngine;

public class IVR_Walking : MonoBehaviour {
    [HideInInspector]
    private InstantVR ivr;

    public bool walking = true;
    public float walkingSpeed = 1;
    public bool sidestepping = true;
    public bool rotating = false;
    public float rotationSpeedRate = 60;

    public bool proximitySpeed = true;
    public float proximitySpeedRate = 0.8f;
    private const float proximitySpeedStep = 0.05f;

    [HideInInspector]
    private IVR_Input leftInput;
    [HideInInspector]
    private CapsuleCollider bodyCapsule;

    void Start() {
        ivr = this.GetComponent<InstantVR>();

        leftInput = ivr.leftHandTarget.GetComponent<IVR_Input>();
        bodyCapsule = AddHipCollider(ivr.hipTarget.gameObject);
    }

    private CapsuleCollider AddHipCollider(GameObject hipObject) {
        Rigidbody rb = hipObject.AddComponent<Rigidbody>();
        if (rb != null) {
            rb.mass = 1;
            rb.useGravity = false;
            rb.isKinematic = true;
            //rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        CapsuleCollider collider = hipObject.AddComponent<CapsuleCollider>();
        if (collider != null) {
            collider.isTrigger = true;
            if (proximitySpeed) {
                collider.height = 0.80f;
                collider.radius = 1f;
            } else {
                collider.height = 1.60f;
                collider.radius = 0.20f;
            }
            collider.center = new Vector3(-hipObject.transform.localPosition.x, 0.2f, -hipObject.transform.localPosition.z);
        }


        return collider;
    }

    public void Update() {
        if (leftInput != null) {
            Vector3 movement = CheckMovement();
            ivr.MoveMe(movement);
            float angle = CheckRotation();
            ivr.RotateMe(angle);
        }
    }

    private float CheckRotation() {
        if (rotating && leftInput != null) {
            float yRotation = leftInput.yAngle;

            if (yRotation != 0) {
                float dOrientation = (yRotation / 90) * (rotationSpeedRate * Time.deltaTime);
                return dOrientation;
            }
        }
        return 0;
    }

    private float curProximitySpeed = 1;
    private Vector3 directionVector = Vector3.zero;

    private Vector3 CheckMovement() {
        float maxAcceleration = 0;
        float sidewardSpeed = 0;

        float horizontal = 0;
        float vertical = leftInput.stickVertical;
        float forwardSpeed = Mathf.Min(1.0f, Mathf.Abs(vertical));

        if (proximitySpeed)
            curProximitySpeed = CalculateProximitySpeed(bodyCapsule, curProximitySpeed);

        if (walking) {
            if (forwardSpeed != 0 || directionVector.z != 0) {
                forwardSpeed = forwardSpeed * forwardSpeed;
                forwardSpeed *= Mathf.Sign(vertical) * walkingSpeed;
                if (vertical < 0)
                    forwardSpeed *= 0.6f;

                if (proximitySpeed)
                    forwardSpeed *= curProximitySpeed;

                float acceleration = forwardSpeed - directionVector.z;
                maxAcceleration = 1f * Time.deltaTime;
                acceleration = Mathf.Clamp(acceleration, -maxAcceleration, maxAcceleration);
                forwardSpeed = directionVector.z + acceleration;
            }
        }

        if (sidestepping) {
            horizontal = leftInput.stickHorizontal;
            sidewardSpeed = Mathf.Min(1.0f, Mathf.Abs(horizontal));

            if (sidewardSpeed != 0 || directionVector.x != 0) {
                sidewardSpeed = sidewardSpeed * sidewardSpeed;
                sidewardSpeed *= Mathf.Sign(horizontal) * 0.5f * walkingSpeed;

                if (proximitySpeed)
                    sidewardSpeed *= curProximitySpeed;

                float acceleration = sidewardSpeed - directionVector.x;
                maxAcceleration = 1f * Time.deltaTime;
                acceleration = Mathf.Clamp(acceleration, -maxAcceleration, maxAcceleration);
                sidewardSpeed = directionVector.x + acceleration;
            }
        }
        directionVector = new Vector3(sidewardSpeed, 0, forwardSpeed);
        Vector3 worldDirectionVector = ivr.hipTarget.TransformDirection(directionVector);
        ivr.inputDirection = worldDirectionVector;

        if (curProximitySpeed <= 0.15f || (!proximitySpeed && ivr.triggerEntered)) {
            ivr.collided = true;
            float angle = Vector3.Angle(worldDirectionVector, ivr.hitNormal);

            Debug.DrawRay(ivr.transform.position, ivr.hitNormal, Color.magenta);
            if (angle > 90.1) {
                directionVector = Vector3.zero;
                worldDirectionVector = Vector3.zero;
            }
        } else {
            ivr.collided = false;
        }

        return worldDirectionVector;
    }

    private float CalculateProximitySpeed(CapsuleCollider cc, float curProximitySpeed) {
        if (ivr.triggerEntered && curProximitySpeed > 0.15f) {
            curProximitySpeed = CheckProximitySpeedDecrease(cc, curProximitySpeed);
        } else if (curProximitySpeed < 1) {
            curProximitySpeed = CheckProximitySpeedIncrease(cc, curProximitySpeed);
        }
        return curProximitySpeed;
    }

    private float CheckProximitySpeedDecrease(CapsuleCollider cc, float curProximitySpeed) {
        bool collision = false;

        Vector3 castDirection = ivr.transform.rotation * ivr.inputDirection;
        if (ivr.inputDirection.magnitude == 0)
            castDirection = ivr.transform.rotation * -ivr.hitNormal;

        if (castDirection.magnitude > 0) {

            Vector3 top = ivr.hipTarget.position + (cc.radius - 0.8f) * Vector3.up - castDirection * 0.05f;
            Vector3 bottom = ivr.hipTarget.position - (cc.radius - 1.2f) * Vector3.up - castDirection * 0.05f;

            RaycastHit[] hits = Physics.CapsuleCastAll(top, bottom, cc.radius - 0.05f, castDirection, 0.09f);
            for (int i = 0; i < hits.Length && collision == false; i++) {
                if (hits[i].rigidbody == null) {
                    //if (hits[i].point.y > ivr.transform.position.y) {
                        collision = true;
                        cc.radius -= 0.05f / proximitySpeedRate;
                        cc.height += 0.05f / proximitySpeedRate;
                        curProximitySpeed = CalculateProximitySpeed(cc.radius);
                    //}
                }
            }
        }

        return curProximitySpeed;
    }

    private float CheckProximitySpeedIncrease(CapsuleCollider cc, float curProximitySpeed) {
        bool collision = false;

        Vector3 castDirection = ivr.transform.rotation * -ivr.inputDirection;
        if (ivr.inputDirection.magnitude == 0)
            castDirection = ivr.transform.rotation * ivr.hitNormal;

        Vector3 top = ivr.hipTarget.position + (cc.radius - 0.75f) * Vector3.up;
        Vector3 bottom = ivr.hipTarget.position - (cc.radius - 1.15f) * Vector3.up;

        RaycastHit[] hits = Physics.CapsuleCastAll(top, bottom, cc.radius, castDirection, 0.04f);
        for (int i = 0; i < hits.Length && collision == false; i++) {
            if (hits[i].rigidbody == null) {
                //if (hits[i].normal.x != 0 || hits[i].normal.z != 0) {
                    collision = true;
                //}
            }
        }

        if (collision == false) {
            cc.radius += 0.05f / proximitySpeedRate;
            cc.height -= 0.05f / proximitySpeedRate;
            curProximitySpeed = CalculateProximitySpeed(cc.radius);
        }

        return curProximitySpeed;
    }

    private float CalculateProximitySpeed(float distance) {
        return EaseIn(1, (-0.90f), 1 - distance, 0.75f);
    }

    private static float EaseIn(float start, float distance, float elapsedTime, float duration) {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
        return distance * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
    }
}