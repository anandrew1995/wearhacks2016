/* InstantVR Animator hip
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.2.4
 * date: January 29, 2016
 * 
 * - Fixed collisions while walking
 */

using UnityEngine;

public class IVR_AnimatorHip : IVR_Controller {

    public bool followHead = true;
    public enum Rotations {
        NoRotation = 0,
        HandRotation = 1,
        LookRotation = 2,
        Auto = 3
    };
    public Rotations rotationMethod = Rotations.HandRotation;

    [HideInInspector]
    private Vector3 headStartPosition;
    [HideInInspector]
    private Vector3 spineLength;

    void Start() { }

    public override void StartController(InstantVR ivr) {
        base.StartController(ivr);

        present = true;
        headStartPosition = ivr.headTarget.position - ivr.transform.position;
        spineLength = ivr.headTarget.position - ivr.hipTarget.position;
    }

    public override void UpdateController() {
        if (this.enabled) {
            if (followHead)
                FollowHead();

            switch (GetRotationMethod()) {
                case Rotations.HandRotation:
                    HandRotation();
                    break;
                case Rotations.LookRotation:
                    LookRotation();
                    break;
            }

            tracking = true;
            base.UpdateController();
        } else
            tracking = false;
    }

    private Rotations GetRotationMethod() {
        if (rotationMethod == Rotations.Auto) {
            return Rotations.LookRotation;
        } else {
            return rotationMethod;
        }
    }

    private void FollowHead() {
        Vector3 headDelta = Quaternion.Inverse(ivr.transform.rotation) * ((ivr.headTarget.position - ivr.transform.position) - headStartPosition);

        Vector3 head2hip = ivr.headTarget.position - ivr.hipTarget.position;
        Vector3 spineStretch = head2hip - spineLength;

        if (spineStretch.magnitude > 0.01f) {
            Vector3 deltaXZ = new Vector3(head2hip.x, 0, head2hip.z);
            float deltaY;
            if (deltaXZ.magnitude > spineLength.magnitude)
                deltaY = this.position.y;
            else
                deltaY = ivr.headTarget.position.y - ivr.transform.position.y - Mathf.Sqrt(spineLength.magnitude * spineLength.magnitude - deltaXZ.magnitude * deltaXZ.magnitude);

            float angle = Vector3.Angle(headDelta, ivr.hitNormal);
            if (ivr.collided && angle > 90.1) {
                this.position = new Vector3(this.position.x, deltaY, this.position.z);
                /*
                                if (ivr.headTarget.position.y < headStartPosition.y - 0.04f) {
                                    this.position = new Vector3(this.position.x, deltaY, this.position.z);
                                } else {
                */
            } else {
                this.position = new Vector3(headDelta.x, deltaY, headDelta.z);
            }
        }
    }

    private void HandRotation() {
        float dOrientation = 0;

        if (ivr.LeftHandController != null && ivr.RightHandController != null && ivr.LeftHandController.isTracking() && ivr.RightHandController.isTracking()) {
            float dOrientationL = AngleDifference(ivr.hipTarget.eulerAngles.y, ivr.leftHandTarget.eulerAngles.y);
            float dOrientationR = AngleDifference(ivr.hipTarget.eulerAngles.y, ivr.rightHandTarget.eulerAngles.y);

            if (Mathf.Sign(dOrientationL) == Mathf.Sign(dOrientationR)) {
                if (Mathf.Abs(dOrientationL) < Mathf.Abs(dOrientationR))
                    dOrientation = dOrientationL;
                else
                    dOrientation = dOrientationR;
            }

            float neckOrientation = AngleDifference(ivr.headTarget.eulerAngles.y, ivr.hipTarget.eulerAngles.y + dOrientation);
            if (neckOrientation < 90 && neckOrientation > -90) { // head cannot turn more than 90 degrees
                this.rotation *= Quaternion.AngleAxis(dOrientation, Vector3.up);
            }
        } else {
            this.rotation = Quaternion.identity;
        }
    }

    private void LookRotation() {
        this.rotation = Quaternion.Euler(
            this.rotation.eulerAngles.x,
            ivr.headTarget.eulerAngles.y - ivr.transform.eulerAngles.y,
            this.rotation.eulerAngles.z);
    }

    public override void OnTargetReset() {
    }


    private float AngleDifference(float a, float b) {
        float r = b - a;
        return NormalizeAngle(r);
    }

    private float NormalizeAngle(float a) {
        while (a < -180) a += 360;
        while (a > 180) a -= 360;
        return a;
    }

    void OnTriggerStay(Collider otherCollider) {
        ivr.triggerEntered = true;
        //        if (ivr.hitNormal == Vector3.zero)
        //            OnTriggerEnter(otherCollider);        
    }

    void OnTriggerEnter(Collider otherCollider) {
        if (ivr != null && ivr.triggerEntered == false) {
            ivr.triggerEntered = false;
            Vector3 dir = ivr.inputDirection;
            if (dir.magnitude > 0) {
                CapsuleCollider cc = ivr.hipTarget.GetComponent<CapsuleCollider>();
                RaycastHit[] hits = Physics.CapsuleCastAll(ivr.hipTarget.position + (cc.radius - 1) * Vector3.up, ivr.hipTarget.position - (cc.radius - 1) * Vector3.up, cc.radius - 0.05f, ivr.inputDirection, 0.04f);
                for (int i = 0; i < hits.Length && ivr.triggerEntered == false; i++) {
                    if (hits[i].rigidbody == null) {
                        //if (hits[i].normal.x != 0 || hits[i].normal.z != 0) {
                            ivr.triggerEntered = true;
                            ivr.hitNormal = hits[i].normal;
                        //}
                    }
                }
            }

            //            if (!ivr.triggerEntered)
            //                ivr.hitNormal = Vector3.zero;
        }
    }

    void OnTriggerExit() {
        ivr.triggerEntered = false;
    }
}
