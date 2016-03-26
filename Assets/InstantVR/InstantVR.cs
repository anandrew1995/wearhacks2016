/* InstantVR
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.2.4
 * date: January 29, 2016
 * 
 * - Added collided flag
 */

using UnityEngine;

public class InstantVR : MonoBehaviour {

    [Tooltip("Target Transform for the head")]
	public Transform headTarget;
    [Tooltip("Target Transform for the left hand")]
    public Transform leftHandTarget;
    [Tooltip("Target Transform for the right hand")]
	public Transform rightHandTarget;
    [Tooltip("Target Transform for the hip")]
	public Transform hipTarget;
    [Tooltip("Target Transform for the left foot")]
	public Transform leftFootTarget;
    [Tooltip("Target Transform for the right foot")]
	public Transform rightFootTarget;

	public enum BodySide {
		Unknown = 0,
		Left = 1,
		Right = 2,
	};

	[HideInInspector] private IVR_Extension[] extensions;

	[HideInInspector] private IVR_Controller[] headControllers;
	[HideInInspector] private IVR_Controller[] leftHandControllers, rightHandControllers;
	[HideInInspector] private IVR_Controller[] hipControllers;
	[HideInInspector] private IVR_Controller[] leftFootControllers, rightFootControllers;

	private IVR_Controller headController;
	public IVR_Controller HeadController { get { return headController; } set { headController = value; } }
	private IVR_Controller leftHandController, rightHandController;
	public IVR_Controller LeftHandController { get { return leftHandController; } set { leftHandController = value; } }
	public IVR_Controller RightHandController { get { return rightHandController; } set { rightHandController = value; } }
	private IVR_Controller hipController;
	public IVR_Controller HipController { get { return hipController; } set { hipController = value; } }
	private IVR_Controller leftFootController, rightFootController;
	public IVR_Controller LeftFootController { get { return leftFootController; } set { leftFootController = value; } }
	public IVR_Controller RightFootController { get { return rightFootController; } set { rightFootController = value; } }

	[HideInInspector] private IVR_Input leftInput, rightInput;
	[HideInInspector] public IVR_Movements leftMovements, rightMovements;

	[HideInInspector] public Transform characterTransform;

	[HideInInspector] public int playerID = 0;

    //[HideInInspector]
    public bool triggerEntered = false, collided = false;
    [HideInInspector] public Vector3 hitNormal = Vector3.zero;
	[HideInInspector] public Vector3 inputDirection;

	protected virtual void Awake() {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        extensions = this.GetComponents<IVR_Extension>();
		foreach (IVR_Extension extension in extensions)
			extension.StartExtension();

		headControllers = headTarget.GetComponents<IVR_Controller>();
		leftHandControllers = leftHandTarget.GetComponents<IVR_Controller>();
		rightHandControllers = rightHandTarget.GetComponents<IVR_Controller>();
		hipControllers = hipTarget.GetComponents<IVR_Controller>();
		leftFootControllers = leftFootTarget.GetComponents<IVR_Controller>();
		rightFootControllers = rightFootTarget.GetComponents<IVR_Controller>();

		headController = FindActiveController(headControllers);
		leftHandController = FindActiveController(leftHandControllers);
		rightHandController = FindActiveController(rightHandControllers);
		hipController = FindActiveController(hipControllers);
		leftFootController = FindActiveController(leftFootControllers);
		rightFootController = FindActiveController(rightFootControllers);
        
		leftInput = leftHandTarget.GetComponent<IVR_Input>();
		rightInput = rightHandTarget.GetComponent<IVR_Input>();

        leftMovements = leftHandTarget.GetComponent<IVR_Movements>();
		rightMovements = rightHandTarget.GetComponent<IVR_Movements>();

		Animator[] animators = GetComponentsInChildren<Animator>();
		for (int i = 0; i < animators.Length; i++) {
			Avatar avatar = animators[i].avatar;
			if (avatar.isValid && avatar.isHuman) {
				characterTransform = animators[i].transform;
				
				AddRigidbody(characterTransform.gameObject);
			}
		}
		
		foreach (IVR_Controller c in headControllers)
			c.StartController(this);
		foreach (IVR_Controller c in leftHandControllers)
			c.StartController(this);
		foreach (IVR_Controller c in rightHandControllers)
			c.StartController(this);
		foreach (IVR_Controller c in hipControllers)
			c.StartController(this);
		foreach (IVR_Controller c in leftFootControllers)
			c.StartController(this);
		foreach (IVR_Controller c in rightFootControllers)
			c.StartController(this);

		IVR_BodyMovements bm = GetComponent<IVR_BodyMovements>();
		if (bm != null)
			bm.StartMovements();

		if (leftMovements != null && leftMovements.enabled)
			leftMovements.StartMovements(this);
		if (rightMovements != null && rightMovements.enabled)
			rightMovements.StartMovements(this);
	}

	private IVR_Controller FindActiveController(IVR_Controller[] controllers) {
		for (int i = 0; i < controllers.Length; i++) {
			if (controllers[i].isTracking())
				return(controllers[i]);
		}
		return null;
	}


	void Update () {
		UpdateExtensions();
        ResetInputs();
        UpdateControllers();
		UpdateMovements();

		CheckCalibrating();

		CheckQuit();
	}

	void LateUpdate() {
		LateUpdateExtensions();
	}

	private void UpdateExtensions() {
		foreach (IVR_Extension extension in extensions)
			extension.UpdateExtension();
	}

	private void LateUpdateExtensions() {
		foreach (IVR_Extension extension in extensions)
			extension.LateUpdateExtension();
	}

    private void ResetInputs() {
        if (leftInput && leftHandControllers.Length > 0)
            leftInput.ResetInput();
        if (rightInput && rightHandControllers.Length > 0)
            rightInput.ResetInput();
    }

    private void UpdateControllers() {
		leftHandController = UpdateController(leftHandControllers, leftHandController);
		rightHandController = UpdateController(rightHandControllers, rightHandController);
		hipController = UpdateController(hipControllers, hipController);
		leftFootController = UpdateController(leftFootControllers, leftFootController);
		rightFootController = UpdateController(rightFootControllers, rightFootController);
		// Head needs to be after hands because of traditional controller.
		headController = UpdateController(headControllers, headController);
	}

	private IVR_Controller UpdateController(IVR_Controller[] controllers, IVR_Controller lastActiveController) {
        if (controllers != null) {
            int lastIndex = 0, newIndex = 0;

            IVR_Controller activeController = null;
            for (int i = 0; i < controllers.Length; i++) {
                if (controllers[i] != null) {
                    controllers[i].UpdateController();
                    if (activeController == null && controllers[i].isTracking()) {
                        activeController = controllers[i];
                        controllers[i].SetSelection(true);
                        newIndex = i;
                    } else
                        controllers[i].SetSelection(false);

                    if (controllers[i] == lastActiveController)
                        lastIndex = i;
                }
            }

            if (lastIndex < newIndex && lastActiveController != null) { // we are degreding
                activeController.TransferCalibration(lastActiveController);
            }

            return activeController;
        } else
            return null;
    }

    private void UpdateMovements() {
		if (leftMovements)
			leftMovements.UpdateMovements();
		if (rightMovements)
			rightMovements.UpdateMovements();

        CheckGrounded();        
	}

    private void CheckGrounded() {
        if (!GrabbedStaticObject()) { 
            RaycastHit hit;
            Vector3 rayStart = transform.position + new Vector3(0, 0.1f, 0);
            if (Physics.Raycast(rayStart, Vector3.down, out hit, 0.15f)) {
                if (hit.distance < 0.5f)
                    transform.position -= Vector3.up * 0.02f; // should be 'falling' 
                if (hit.distance > 0)
                    transform.position = rayStart - Vector3.up * hit.distance;

            } else {
                transform.position -= Vector3.up * 0.02f; // should be 'falling'
            }
        }
    }

    private bool GrabbedStaticObject() {
        if (leftMovements != null &&
            leftMovements.grabbedObject != null &&
            leftMovements.grabbedObject.isStatic == true) {
            return true;
        } else
        if (rightMovements != null &&
            rightMovements.grabbedObject != null &&
            rightMovements.grabbedObject.isStatic == true) {
            return true;
        } else {
            return false;
        }
    }

    [HideInInspector] private bool calibrating = false;

	void CheckCalibrating() {
		if ((leftInput != null || rightInput != null)) {
			if (calibrating == false &&
			    (leftInput == null || leftInput.option) && 
			    (rightInput == null || rightInput.option)) {
				
				calibrating = true;
				Calibrate();
			} else
				if (calibrating == true &&
				    (leftInput == null || !leftInput.option) && 
				   (rightInput == null || !rightInput.option)) {
				calibrating = false;
			}
		}
	}

	public void Calibrate() {
		foreach (Transform t in headTarget.parent) {
			t.gameObject.SendMessage("OnTargetReset", SendMessageOptions.DontRequireReceiver);
		}
	}
	
	public void MoveMe(Vector3 translationVector, bool allowUp = false) {
		if (translationVector.magnitude > 0) {
            Vector3 translation = translationVector * Time.deltaTime;
            if (allowUp)
                transform.position += translation;
            else
                transform.position += new Vector3(translation.x, 0, translation.z);

        }
    }

	public void RotateMe(float angle) {
        transform.rotation *= Quaternion.AngleAxis(angle, Vector3.up);
    }

	protected void CheckQuit() {
		if (Input.GetKeyDown(KeyCode.Escape))
			Application.Quit();
	}
	
	protected void AddRigidbody(GameObject gameObject) {
		Rigidbody rb = gameObject.AddComponent<Rigidbody>();
		if (rb != null) {
			rb.mass = 75;
			rb.useGravity = false;
			rb.isKinematic = true;
		}
	}
}
