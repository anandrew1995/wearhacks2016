/* Instant Body Movements
 * author: Pascal Serrarnes
 * email: support@passervr.com
 * version: 3.2.3
 * date: January 2, 2016

Changes: 
 * - Fixed hand rotation issue
 */

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class IVR_BodyMovements: MonoBehaviour {
    [HideInInspector]
    private InstantVR ivr;

    private Transform headTarget;
    private Transform rightHandTarget, leftHandTarget;
    private Transform hipTarget;
    private Transform rightFootTarget, leftFootTarget;
    
    [HideInInspector]
    private GameObject rightPalmTarget, leftPalmTarget;
    [HideInInspector]
	private Transform rightHandOTarget, leftHandOTarget;

    [Tooltip("Are upper body movements allowed?")]
	public bool enableTorso = true;
    [Tooltip("Are leg movements allowed?")]
	public bool enableLegs = true;

    [HideInInspector]
    private Animator animator = null;
    [HideInInspector]
    public ArmMovements leftArm, rightArm;
    private Torso torso;
    private Leg leftLeg, rightLeg;

    [HideInInspector]
    private Rigidbody characterRigidbody;
    [HideInInspector]
    private Transform characterTransform ;

	
	public static float maxHipAngle = 70;
	
	//private float palmLength = 0; //.05f;

    private bool fromNormCalculated = false;

  
	public void SetRightHandTarget(Transform newTarget) {
		rightHandOTarget = newTarget;
        rightHandTarget = newTarget;
	}

    public void SetLeftHandTarget(Transform newTarget) {
        leftHandOTarget = newTarget;
        leftHandTarget = newTarget;
    }

	public void StartMovements() {
		ivr = this.GetComponent<InstantVR>();

		headTarget = ivr.headTarget;
		leftHandTarget = ivr.leftHandTarget;
		rightHandTarget = ivr.rightHandTarget;
		hipTarget = ivr.hipTarget;
		leftFootTarget = ivr.leftFootTarget;
		rightFootTarget = ivr.rightFootTarget;

		animator = ivr.transform.GetComponentInChildren<Animator>();
        if (animator == null) {
            StopMovements();
        } else {

            characterRigidbody = animator.GetComponent<Rigidbody>();

            if (characterRigidbody != null)
                characterTransform = characterRigidbody.GetComponent<Transform>();

            if (leftArm == null)
                leftArm = new ArmMovements(ivr, ArmMovements.BodySide.Left, this);
            leftArm.Initialize(ivr, ArmMovements.BodySide.Left, this);

            if (rightArm == null)
                rightArm = new ArmMovements(ivr, ArmMovements.BodySide.Right, this);
            rightArm.Initialize(ivr, ArmMovements.BodySide.Right, this);

            Transform neck, spine, hips;

            neck = animator.GetBoneTransform(HumanBodyBones.Neck);
            if (neck == null)
                neck = animator.GetBoneTransform(HumanBodyBones.Head);
            spine = animator.GetBoneTransform(HumanBodyBones.Spine);
            hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            torso = new Torso(neck, spine, hips, hipTarget, headTarget, rightArm);

            leftLeg = new Leg(ArmMovements.BodySide.Left, animator, hipTarget);
            rightLeg = new Leg(ArmMovements.BodySide.Right, animator, hipTarget);

            if (rightHandTarget != null) {
                rightHandOTarget = rightHandTarget;
                /*
                float targetScaleZ = rightHandTarget.transform.localScale.z;
                rightPalmTarget = new GameObject();
                rightPalmTarget.name = "Palm_R_Target";
                rightPalmTarget.transform.parent = rightHandTarget.transform;
                rightPalmTarget.transform.localPosition = new Vector3(0, 0, -palmLength/targetScaleZ);
                rightPalmTarget.transform.localEulerAngles = Vector3.zero;
				
                rightHandTarget = rightPalmTarget.transform;
                */
            }

            if (leftHandTarget != null) {
                leftHandOTarget = leftHandTarget;
                /*
                float targetScaleZ = leftHandTarget.transform.localScale.z;
                leftPalmTarget = new GameObject();
                leftPalmTarget.name = "Palm_L_Target";
                leftPalmTarget.transform.parent = leftHandTarget.transform;
                leftPalmTarget.transform.localPosition = new Vector3(0, 0, -palmLength/targetScaleZ);
                leftPalmTarget.transform.localEulerAngles = Vector3.zero;

                leftHandTarget = leftPalmTarget.transform;
                */
            }

            if (headTarget == null && enableTorso) {
                GameObject neckTargetGO = new GameObject("Neck_Target");
                headTarget = neckTargetGO.transform;
                headTarget.parent = characterTransform;
                headTarget.position = torso.neck.position;
                headTarget.rotation = characterTransform.rotation;
            }

            if (enableLegs) {
                if (rightFootTarget == null) {
                    //userLegTargets = false;
                    GameObject rightFootTargetGO = new GameObject("Foot_R_Target");
                    rightFootTarget = rightFootTargetGO.transform;
                    rightFootTarget.parent = characterTransform;
                    rightFootTarget.position = rightLeg.foot.position;
                    rightFootTarget.rotation = characterTransform.rotation;
                }

                if (leftFootTarget == null) {
                    //userLegTargets = false;
                    GameObject leftFootTargetGO = new GameObject("Foot_L_Target");
                    leftFootTarget = leftFootTargetGO.transform;
                    leftFootTarget.parent = characterTransform;
                    leftFootTarget.position = leftLeg.foot.position;
                    leftFootTarget.rotation = characterTransform.rotation;
                }
            }


            if (IsInTPose(leftArm, rightArm)) {
                CalculateFromNormTPose(leftArm, rightArm);
            } else {
                CalculateFromNormTracking(leftArm, rightArm, leftHandTarget, rightHandTarget);
            }
        }
    }

    public void StopMovements() {
        fromNormCalculated = false;
    }
	
	bool crouching = false;
	float bendAngle = 0;

	void Update () {
        if (!fromNormCalculated || torso == null || leftArm == null || rightArm == null)
            StartMovements();
        
        if (animator == null)
            StopMovements();
        else
            UpdateBodyMovements();
    }

	public void UpdateBodyMovements() {
		if (torso.userNeckTarget) {
			if (enableTorso)
				torso.CalculateHorizontal(headTarget);
			
			if (enableLegs)
				torso.CalculateVertical(headTarget);

            leftArm.Calculate(leftHandTarget);
            rightArm.Calculate(rightHandTarget);

			CalculateHeadOrientation(torso.neck, headTarget);
		} else {
			if (bendAngle <= 0 && !crouching) {
				leftArm.Calculate(leftHandTarget);
				rightArm.Calculate(rightHandTarget);
			}
			
			if (enableTorso && !crouching) 
				bendAngle = torso.AutoHorizontal(rightHandOTarget, rightHandTarget, rightArm, leftHandOTarget, leftHandTarget, leftArm, headTarget);
			
			if (enableLegs && bendAngle >= maxHipAngle)
				crouching = torso.AutoVertical(rightHandOTarget, rightHandTarget, rightArm, leftHandOTarget, leftHandTarget, leftArm, headTarget);
		}
		
		if (enableLegs) {
			rightLeg.Calculate(rightFootTarget.transform);
			leftLeg.Calculate(leftFootTarget.transform);
		}
	}
	
	void CalculateHeadOrientation(Transform neck, Transform neckTarget) {
		neck.rotation = neckTarget.rotation * torso.fromNormNeck;
	}

    public bool IsInTPose(ArmMovements leftArm, ArmMovements rightArm) {
        float d;
        Ray hand2hand = new Ray(leftArm.hand.position, rightArm.hand.position - leftArm.hand.position);

        // All lined up?
        d = DistanceToRay(hand2hand, leftArm.forearm.position);
        if (d > 0.1f) return false;

        d = DistanceToRay(hand2hand, leftArm.upperArm.position);
        if (d > 0.1f) return false;

        d = DistanceToRay(hand2hand, rightArm.upperArm.position);
        if (d > 0.1f) return false;

        d = DistanceToRay(hand2hand, rightArm.forearm.position);
        if (d > 0.1f) return false;

        // Arms stretched?
        d = Vector3.Distance(leftArm.upperArm.position, leftArm.hand.position);
        if (d < leftArm.length - 0.1f) return false;

        d = Vector3.Distance(rightArm.upperArm.position, rightArm.hand.position);
        if (d < rightArm.length - 0.1f) return false;

        Debug.Log("Avatar is in T-pose");
        return true;
    }

    private static float DistanceToRay(Ray ray, Vector3 point) {
        return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
    }

    public void CalculateFromNormTPose(ArmMovements leftArm, ArmMovements rightArm) {
        if (leftArm != null) {
            leftArm.CalculateFromNormTPose();
        }

        if (rightArm != null) {
            rightArm.CalculateFromNormTPose();
        }
        fromNormCalculated = true;
    }

    public void CalculateFromNormTracking(ArmMovements leftArm, ArmMovements rightArm, Transform leftHandTarget, Transform rightHandTarget) {
        if (leftArm != null) {
            leftArm.CalculateFromNormTracking(leftHandTarget);
        }

        if (rightArm != null) {
            rightArm.CalculateFromNormTracking(rightHandTarget);
        }
        fromNormCalculated = true;
    }

	private float AngleDifference(float a, float b) {
		float r = b - a;
		while (r < -180) r += 360;
		while (r > 180) r -= 360;
		return r;
	}

}

[System.Serializable]
public class Torso {
	private Transform hipTarget;
	
	public Transform neck;
	public Transform spine;
	public Transform hips;
	
	public Quaternion fromNormNeck;
	public Quaternion fromNormTorso;
	public Quaternion fromNormHips;

	public float length;
	
	private Vector3 neckStartPosition;
	private Vector3 hipStartPosition;
	private Vector3 spineStartOrientation;
	public Quaternion spineStartRotation;
	
	public bool userNeckTarget;
	private Vector3 spineAxis;

    public Torso(Transform neck, Transform spine, Transform hips, Transform hipTarget, Transform neckTarget, ArmMovements arm) {
		this.hipTarget = hipTarget;
		
		this.neck = neck;
		neckStartPosition = neck.position;

		this.spine = spine;
		spineStartRotation = spine.rotation;
		spineStartOrientation = neck.position - spine.position;
		
		this.hips = hips;
		hipStartPosition = hips.position;

		Vector3 neckAtUpperArm = new Vector3(neck.position.x, arm.upperArm.position.y, neck.position.z);
		length = Vector3.Distance(spine.position, neckAtUpperArm);
		
		fromNormNeck = Quaternion.Inverse(Quaternion.LookRotation(hipTarget.forward)) * neck.rotation;
		fromNormTorso = Quaternion.Inverse(Quaternion.LookRotation(neck.position - spine.position, hipTarget.forward)) * spine.rotation;
		fromNormHips = Quaternion.Inverse(Quaternion.LookRotation(hipTarget.forward)) * hips.rotation;

		userNeckTarget = (neckTarget != null);
		spineAxis = spine.InverseTransformDirection(hipTarget.right);
	}
	
	public void CalculateHorizontal(Transform neckTarget) {
		if (hipTarget.gameObject.activeSelf)
			hips.position = new Vector3(hipTarget.position.x, hips.position.y, hipTarget.position.z);
		hips.rotation = Quaternion.LookRotation(hipTarget.forward, Vector3.up) * fromNormHips;
		spine.LookAt(neckTarget.transform, hipTarget.forward);
		spine.rotation *= fromNormTorso;			
	}

    public float AutoHorizontal(Transform rightHandOTarget, Transform rightHandTarget, ArmMovements rightArm, Transform leftHandOTarget, Transform leftHandTarget, ArmMovements leftArm, Transform neckTarget) {
		float bendAngle = 0;
		Vector3 torsoTarget = Vector3.zero;
		Vector3 dShoulderNeck = (leftArm.upperArm.position - rightArm.upperArm.position) / 2;
		
		Vector3 leftToTarget = leftHandOTarget.position - leftArm.upperArmStartPosition;
		Vector3 rightToTarget = rightHandOTarget.position - rightArm.upperArmStartPosition;
		float leftOver = leftToTarget.magnitude - leftArm.length;
		float rightOver = rightToTarget.magnitude - rightArm.length;
		
		if (leftOver > 0) {
			if (rightOver > 0) {
				Vector3 torsoTargetR = rightHandOTarget.position + dShoulderNeck;
				Vector3 torsoTargetL = leftHandOTarget.position - dShoulderNeck;
				float bendAngleR = BendAngle(torsoTargetR, rightArm);
				float bendAngleL = BendAngle(torsoTargetL, leftArm);
				
				if (bendAngleR > bendAngleL)
					torsoTarget = torsoTargetR;
				else
					torsoTarget = torsoTargetL;
			} else
				torsoTarget = leftHandOTarget.position - dShoulderNeck;
		} else if (rightOver > 0)
			torsoTarget = rightHandOTarget.position + dShoulderNeck;
		
		if (rightOver > 0 || leftOver >  0) {
			bendAngle = BendAngle(torsoTarget, rightArm); // arm should be left or right
			spine.rotation = spineStartRotation * Quaternion.AngleAxis(bendAngle, spineAxis);
		} else
			spine.rotation = spineStartRotation;
		
		rightArm.Calculate(rightHandTarget);
		leftArm.Calculate(leftHandTarget);
		
		return bendAngle;
	}

    public float BendAngle(Vector3 torsoTarget, ArmMovements arm) {
		float baseAngle = Vector3.Angle (spineStartOrientation, torsoTarget - spine.position);
		
		float dSpine2Target = Vector3.Distance (spine.position, torsoTarget);
		float spineAngle = Mathf.Acos((dSpine2Target * dSpine2Target + length*length - arm.length * arm.length)/ (2 * dSpine2Target * length)) * Mathf.Rad2Deg;
		if (float.IsNaN(spineAngle)) spineAngle = 0;
		
		return Mathf.Min(baseAngle - spineAngle, IVR_BodyMovements.maxHipAngle);
	}
	
	public void CalculateVertical(Transform neckTarget) {
		float dY = hipTarget.position.y - hipStartPosition.y;
		hips.position = new Vector3(hips.position.x , hipStartPosition.y + dY, hips.position.z);
	}

    public bool AutoVertical(Transform rightHandOTarget, Transform rightHandTarget, ArmMovements rightArm, Transform leftHandOTarget, Transform leftHandTarget, ArmMovements leftArm, Transform neckTarget) {
		Vector3 neckDelta = Vector3.zero;
		
		Vector3 leftToTarget = leftHandOTarget.position - leftArm.upperArmStartPosition;
		Vector3 rightToTarget = rightHandOTarget.position - rightArm.upperArmStartPosition;
		float leftOver = leftToTarget.magnitude - leftArm.length;
		float rightOver = rightToTarget.magnitude - rightArm.length;
		if (leftOver > 0) {
			if (rightOver > leftOver)
				neckDelta = rightToTarget.normalized * rightOver;
			else
				neckDelta = leftToTarget.normalized * leftOver;
		} else if (rightOver > 0)
			neckDelta = rightToTarget.normalized * rightOver;
		
		neckTarget.position = neckStartPosition + new Vector3(0, neckDelta.y, 0);

		float dY = neckTarget.transform.position.y - neck.position.y;
		if (hips.position.y + dY < hipStartPosition.y)  {
			hips.Translate(0, dY, 0, Space.World);
		} else if (hips.position.y + dY > hipStartPosition.y) {
			hips.position = new Vector3(hips.position.x, hipStartPosition.y, hips.position.z);
		}
		
		rightArm.Calculate(rightHandTarget);
		leftArm.Calculate(leftHandTarget);
		
		return (hips.position.y < hipStartPosition.y);
	}
}
	public abstract class Digit {
        public abstract void Init(GameObject hand, Vector3 handRightAxis, Vector3 axis2, bool bodySideLeft, int fingerIndex);
		
		public abstract void Update(float inputValue);
	}
	
	[System.Serializable]
	public class Finger : Digit {
		public Transform transform;
        private int fingerIndex;
		public float input;
		private float val;
		private Vector3 axis1, axis2;
		private float grabAmount = 0;
		private int nPhalanges;
		private Phalanx[] phalanges;

        public override void Init(GameObject hand, Vector3 fingerAxis, Vector3 armAxis, bool bodySideLeft, int fingerIndex) {
            if (transform != null) {
                this.fingerIndex = fingerIndex;
				this.axis1 = fingerAxis;
                this.axis2 = Vector3.Cross(fingerAxis, armAxis);
				
				phalanges = new Phalanx[3];
				phalanges[0] = new Phalanx(transform);
				if (phalanges[0].transform.childCount == 1) {
					phalanges[1] = new Phalanx(phalanges[0].transform.GetChild(0).transform);
					if (phalanges[1].transform.childCount == 1) {
						phalanges[2] = new Phalanx(phalanges[1].transform.GetChild(0).transform);
						nPhalanges = 3;
					} else {
						phalanges[2] = null;
						nPhalanges = 2;
					}
				} else {
					phalanges[1] = null;
					nPhalanges = 1;
				}
			}
		}
		
		private float grabSpeed = 0.1f;
		public override void Update(float inputValue) {
			if (transform != null) {
				input = inputValue;
				if (grabAmount > 0)
					val = grabAmount;
				else {
					float d = input - val;
					if (d > grabSpeed)
						val += grabSpeed;
					else if (d < -grabSpeed)
						val -= grabSpeed;
					else
						val += d;
				}
                float val2 = Mathf.Max(0, 0.1f - val) * 20;
                val2 *= (fingerIndex - 2);
				Bend(val, val2);
				
				if (grabAmount > 0) {
					if (input < grabAmount) {
						// drop the object
						grabAmount = 0f;
					}
				}
			}
		}
		
		private void Bend(float val1, float val2) {
			switch (nPhalanges) {
			case 3:
				phalanges[0].Bend(val1 * 45, axis1, val2, axis2);
                phalanges[1].Bend(val1 * 90, axis1, val2, axis2);
                phalanges[2].Bend(val1 * 90, axis1, val2, axis2);
				break;
			case 2:
                phalanges[0].Bend(val1 * 90, axis1, val2, axis2);
                phalanges[1].Bend(val1 * 90, axis1, val2, axis2);
				break;
			case 1:
                phalanges[0].Bend(val1 * 135, axis1, val2, axis2);
				break;
			}
		}
		
		public bool hasGrabbed() {
			return (grabAmount > 0);
		}
		
		public void Grab() {
			grabAmount = val;
		}
	}
	
	[System.Serializable]
	public class Thumb : Digit {
		public Transform transform;
		public float input;
		public Transform characterTransform;
		private float val, grabAmount = 0;
		private Vector3 axis1;
		private Quaternion z;

        public override void Init(GameObject hand, Vector3 fingerBendAxis, Vector3 forearmAxis, bool bodySideLeft, int fingerIndex) {
            if (transform != null) {
				if (bodySideLeft) {
                    axis1 = -Vector3.Cross(fingerBendAxis, forearmAxis);
                } else {
                    axis1 = Vector3.Cross(fingerBendAxis, forearmAxis);
                }
				z = transform.localRotation * 
					Quaternion.AngleAxis(-20, axis1);
			}
		}
		
		private float grabSpeed = 0.1f;
		public override void Update(float inputValue) {
			if (transform != null) {
				input = inputValue;
				if (grabAmount > 0)
					val = grabAmount;
				else {
					float d = input - val;
					if (d > grabSpeed)
						val += grabSpeed;
					else if (d < -grabSpeed)
						val -= grabSpeed;
					else
						val += d;
				}

                transform.localRotation = z *
                    Quaternion.AngleAxis(val * 50, axis1);
            }
		}
	}
	
	[System.Serializable]
	public class Phalanx {
		public Transform transform;
		public Quaternion z;
		
		public Phalanx(Transform newTransform) {
			transform = newTransform;
			z = transform.localRotation;
		}
		
		public void Bend(float bendFactor1, Vector3 axis1, float bendFactor2, Vector3 axis2) {
			transform.localRotation = z * Quaternion.AngleAxis(bendFactor1, axis1) * Quaternion.AngleAxis(bendFactor2, axis2);

		}
	}

[System.Serializable]	
public class ArmMovements {
    private InstantVR ivr;

    private Animator animator;
    public Transform upperArm;
    public Transform forearm;
    public Transform hand;

    public float length;
    private float upperArmLength, forearmLength;
    [HideInInspector]
    private float upperArmLength2, forearmLength2;

    [HideInInspector]
    public Vector3 upperArmStartPosition;

    public enum BodySide {
        Left,
        Right
    };
    private BodySide bodySide;

    [HideInInspector]
    public Quaternion fromNormUpperArm = Quaternion.identity;
    [HideInInspector]
    public Quaternion fromNormForearm = Quaternion.identity;
    [HideInInspector]
    public Quaternion fromNormHand = Quaternion.identity;

    public ArmMovements(InstantVR ivr, BodySide bodySide, IVR_BodyMovements bodyMovements) {
        Initialize(ivr, bodySide, bodyMovements);
    }

    public void Initialize(InstantVR ivr, BodySide bodySide, IVR_BodyMovements bodyMovements) {
        this.ivr = ivr;

        this.bodySide = bodySide;
        animator = ivr.GetComponentInChildren<Animator>();

        if (bodySide == BodySide.Left) {
            upperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            forearm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            hand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        } else {
            upperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            forearm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        }

        upperArmLength = Vector3.Distance(upperArm.position, forearm.position);
        forearmLength = Vector3.Distance(forearm.position, hand.position);
        length = upperArmLength + forearmLength;
        if (length == 0)
            Debug.LogError("Avatar arm positions are incorrect. Please restore avatar in T-pose.");

        upperArmLength2 = upperArmLength * upperArmLength;
        forearmLength2 = forearmLength * forearmLength;

        upperArmStartPosition = upperArm.position;
    }

    public void CalculateFromNormTPose() {

        fromNormUpperArm = Quaternion.Inverse(Quaternion.LookRotation(hand.position - forearm.position)) * upperArm.rotation;
        fromNormForearm = Quaternion.Inverse(Quaternion.LookRotation(hand.position - forearm.position)) * forearm.rotation;
        fromNormHand = Quaternion.Inverse(Quaternion.LookRotation(hand.position - forearm.position)) * hand.rotation;
    }

    public void CalculateFromNormTracking(Transform handTarget) {
        fromNormUpperArm = Quaternion.Inverse(CalculateUpperArmNorm(handTarget)) * upperArm.rotation;
        fromNormForearm = Quaternion.Inverse(CalculateForearmNorm(handTarget)) * forearm.rotation;
        fromNormHand = Quaternion.Inverse(handTarget.rotation) * hand.rotation;
    }

    protected Quaternion CalculateUpperArmNorm(Transform handTarget) {
        float dShoulderTarget = Vector3.Distance(upperArm.position, handTarget.position);
		float shoulderAngle = Mathf.Acos((dShoulderTarget * dShoulderTarget + upperArmLength2 - forearmLength2)/ (2 * dShoulderTarget * upperArmLength)) * Mathf.Rad2Deg;
		if (float.IsNaN(shoulderAngle)) shoulderAngle = 0;

		Vector3 upperArmUp = ivr.hipTarget.InverseTransformDirection(handTarget.up);
		float upperArmUpY, upperArmUpZ;
		if (bodySide == BodySide.Right) {
			upperArmUp = new Vector3(Mathf.Max(0.3f, upperArmUp.x), Mathf.Max(0.01f, upperArmUp.y), Mathf.Max(0.1f, upperArmUp.z));
				
			Vector3 dHandUpper = ivr.hipTarget.InverseTransformDirection(handTarget.position - upperArm.position);
			upperArmUpY = upperArmUp.y;
			upperArmUpZ = upperArmUp.z;
				
			if (dHandUpper.x < 0) upperArmUpZ -= dHandUpper.x * 50;
			if (dHandUpper.y > 0) upperArmUpY += dHandUpper.y * 10;
		} else {
			shoulderAngle = -shoulderAngle;
			upperArmUp = new Vector3(Mathf.Min(-0.3f, upperArmUp.x), Mathf.Max(0.01f, upperArmUp.y), Mathf.Max(0.1f, upperArmUp.z));
				
			Vector3 dHandUpper = ivr.hipTarget.InverseTransformDirection(handTarget.position - upperArm.position);
			upperArmUpY = upperArmUp.y;
			upperArmUpZ = upperArmUp.z;
			if (dHandUpper.x > 0) upperArmUpZ += dHandUpper.x * 50;
			if (dHandUpper.y > 0) upperArmUpY += dHandUpper.y * 10;
		}
		upperArmUp = ivr.hipTarget.TransformDirection(new Vector3(upperArmUp.x, upperArmUpY, upperArmUpZ));
            
		//Debug.DrawRay(upperArm.position, upperArmUp);

        Quaternion upperArmNormRotation = Quaternion.LookRotation(handTarget.position - upperArm.position, upperArmUp);
        upperArmNormRotation = Quaternion.AngleAxis(shoulderAngle, upperArmUp) * upperArmNormRotation;

        return upperArmNormRotation;
    }

    protected Quaternion CalculateForearmNorm(Transform handTarget) {
        Quaternion forearmNormRotation = Quaternion.LookRotation(handTarget.position - forearm.position, handTarget.up);

        return forearmNormRotation;
    }

    protected Quaternion CalculateHandNorm(Transform handTarget) {
        Quaternion handNormRotation = Quaternion.LookRotation(handTarget.position - forearm.position, handTarget.up);

        return handNormRotation;
    }

    public void Calculate(Transform handTarget) {
		if (handTarget != null) {
            upperArm.rotation = CalculateUpperArmNorm(handTarget) * fromNormUpperArm;
            forearm.rotation = CalculateForearmNorm(handTarget) * fromNormForearm;
            if (Vector3.Distance(forearm.position, handTarget.position) > forearmLength) {
//                    hand.position = forearm.position + forearmLength * (handTarget.position - forearm.position).normalized;
            }

            hand.rotation = handTarget.rotation * fromNormHand;
        }
	}
}

[System.Serializable]
public class Leg {
    private Transform characterTransform;
    public Transform upperLeg;
    public Transform lowerLeg;
    public Transform foot;

    private Quaternion fromNormUpperLeg;
    private Quaternion fromNormLowerLeg;
    private Quaternion fromNormFoot;

    private float upperLegLength, lowerLegLength;
    private float upperLegLength2, lowerLegLength2;

    public Leg(ArmMovements.BodySide bodySide_in, Animator animator, Transform characterTransform_in) {
        characterTransform = characterTransform_in;

        if (bodySide_in == ArmMovements.BodySide.Left) {
            upperLeg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            lowerLeg = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            foot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        } else {
            upperLeg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            lowerLeg = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            foot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        }

        fromNormUpperLeg = Quaternion.Inverse(Quaternion.LookRotation(foot.position - upperLeg.position, characterTransform.forward)) * upperLeg.rotation;
        fromNormLowerLeg = Quaternion.Inverse(Quaternion.LookRotation(foot.position - lowerLeg.position, characterTransform.forward)) * lowerLeg.rotation;
        fromNormFoot = Quaternion.Inverse(Quaternion.LookRotation(characterTransform.forward)) * foot.rotation;

        upperLegLength = Vector3.Distance(upperLeg.position, lowerLeg.position);
        lowerLegLength = Vector3.Distance(lowerLeg.position, foot.position);

        upperLegLength2 = upperLegLength * upperLegLength;
        lowerLegLength2 = lowerLegLength * lowerLegLength;
    }

    public void Calculate(Transform footTarget) {
        if (upperLeg != null) {
            float dHipTarget = Vector3.Distance(upperLeg.position, footTarget.position);
            float hipAngle = Mathf.Acos((dHipTarget * dHipTarget + upperLegLength2 - lowerLegLength2) / (2 * upperLegLength * dHipTarget)) * Mathf.Rad2Deg;
            if (float.IsNaN(hipAngle)) hipAngle = 0;

            upperLeg.LookAt(footTarget.position, footTarget.forward);
            upperLeg.rotation = Quaternion.AngleAxis(-hipAngle, upperLeg.right) * upperLeg.rotation;
            upperLeg.rotation *= fromNormUpperLeg;

            lowerLeg.LookAt(footTarget.position, footTarget.forward);
            lowerLeg.rotation *= fromNormLowerLeg;

            foot.rotation = footTarget.rotation * fromNormFoot;
        }
    }
}