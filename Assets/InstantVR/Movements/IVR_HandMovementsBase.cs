/* InstantVR Hand Movements Base
 * author: Pascal Serrarnes
 * email: support@passervr.com
 * version: 3.2.4
 * date: January 29, 2016
 * 
 * - fixed hand collisions influencing walking speed
 */

using UnityEngine;

public class IVR_HandMovementsBase : IVR_Movements {
    [HideInInspector]
    public InstantVR ivr;
    [HideInInspector]
    protected GameObject handObj;

    public float thumbInput;
    public float indexInput;
    public float middleInput;
    public float ringInput;
    public float littleInput;

    public override void StartMovements(InstantVR ivr) {
        this.ivr = ivr;

        Animator animator = ivr.GetComponentInChildren<Animator>();

        if (this.transform == ivr.leftHandTarget) {
            handObj = animator.GetBoneTransform(HumanBodyBones.LeftHand).gameObject;

        } else {
            handObj = animator.GetBoneTransform(HumanBodyBones.RightHand).gameObject;
        }
    }

    protected bool collisionsIgnored = false;

    protected void IgnoreRigidbodyCollisions(Rigidbody myBody, Rigidbody myHand) {
        Collider[] myBodyColliders = myBody.GetComponentsInChildren<Collider>();
        Collider[] myHandColliders = myHand.GetComponentsInChildren<Collider>();

        for (int i = 0; i < myBodyColliders.Length; i++) {
            for (int j = 0; j < myHandColliders.Length; j++) {
                Physics.IgnoreCollision(myBodyColliders[i], myHandColliders[j]);
            }
        }
    }

    protected void IgnoreHandBodyCollisions() {
        Rigidbody hipRigidbody = ivr.hipTarget.GetComponent<Rigidbody>();
        if (hipRigidbody != null) {
            Rigidbody handRigidbody = handObj.GetComponent<Rigidbody>();
            IgnoreRigidbodyCollisions(hipRigidbody, handRigidbody);
        }
    }
    public override void UpdateMovements() {
        if (!collisionsIgnored) {
            IgnoreHandBodyCollisions();
            collisionsIgnored = true;
        }
    }
}
