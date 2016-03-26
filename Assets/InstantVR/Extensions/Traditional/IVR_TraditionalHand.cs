/* InstantVR Traditional hand
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.2.3
 * date: January 3, 2016
 * 
 * - Split Trigger and Bumper axis
 */

using UnityEngine;

public class IVR_TraditionalHand : IVR_HandController {

	public bool mouseInput = true;

	[HideInInspector]
    private IVR_Input ivrInput;
    [HideInInspector]
    private IVR_HandMovementsBase handMovements;
    [HideInInspector]
    private bool joystick2available, bumpersAvailable, triggersAvailable, startBackAvailable;

	[HideInInspector] private float hipStartRotationY;

	void Start() {
	}

	public override void StartController(InstantVR ivr) {
		base.StartController(ivr);
		present = true;

		ivrInput = GetComponent<IVR_Input>();
        handMovements = GetComponent<IVR_HandMovementsBase>();

        joystick2available = CheckJoystick2Present();
        bumpersAvailable = CheckBumpersAvailable();
        triggersAvailable = CheckTriggersAvailable();
        startBackAvailable = CheckStartBackAvailable();

        hipStartRotationY = ivr.hipTarget.eulerAngles.y;
	}

	public override void UpdateController() {
		if (this.enabled) {
			UpdateInput();
			this.position = Vector3.zero;
			this.rotation = Quaternion.identity;
			base.UpdateController();
		}
	}

    private void UpdateInput() {
		if (ivrInput != null) {
			if (this.transform == ivr.leftHandTarget) {
				ivrInput.stickHorizontal += Input.GetAxis("Horizontal");
				ivrInput.stickVertical += Input.GetAxis("Vertical");
				ivrInput.yAngle = calculateStickYAngle();
				ivrInput.xAngle = calculateStickXAngle();

                if (bumpersAvailable)
                    ivrInput.bumper = Input.GetAxis("Bumper L");
                if (triggersAvailable)
                    ivrInput.trigger = Input.GetAxis("Trigger L");
                
                if (startBackAvailable) {
                    ivrInput.option = (Input.GetAxis("Back") > 0);
                }
			} else {
				if (joystick2available) {
					ivrInput.stickHorizontal += Input.GetAxis("Horizontal R");
					ivrInput.stickVertical += Input.GetAxis("Vertical R");
				}
				ivrInput.yAngle = calculateStickYAngle();
				ivrInput.xAngle = calculateStickXAngle();

                if (bumpersAvailable)
                    ivrInput.bumper = Input.GetAxis("Bumper R");
                    if (triggersAvailable)
                        ivrInput.trigger = Input.GetAxis("Trigger R");

                if (startBackAvailable) {
                    ivrInput.option = (Input.GetAxis("Start") > 0);
                }
            }
            ivrInput.option |= Input.GetKey(KeyCode.Tab);

            if (handMovements && selected) {
                if (bumpersAvailable) {
                    if (this.transform == ivr.leftHandTarget) {
                        indexInput = Input.GetAxis("Bumper L");
                        handMovements.indexInput = indexInput;
                    } else {
                        indexInput = Input.GetAxis("Bumper R");
                        handMovements.indexInput = indexInput;
                    }
                }
                if (triggersAvailable) {
                    if (this.transform == ivr.leftHandTarget) {
                        middleInput = Input.GetAxis("Trigger L");
                        handMovements.middleInput = middleInput;
                    } else {
                        middleInput = Input.GetAxis("Trigger R");
                        handMovements.middleInput = middleInput;
                    }
                    thumbInput = handMovements.middleInput;
                    handMovements.thumbInput = thumbInput;
                    ringInput = handMovements.middleInput;
                    handMovements.ringInput = ringInput;
                    littleInput = handMovements.middleInput;
                    handMovements.littleInput = littleInput;
                }
            }
        }
	}

	public override void OnTargetReset() {
		if (selected) {
			Calibrate(true);
		}
	}

	private bool CheckJoystick2Present() {
		bool joy4available = IsAxisAvailable("Horizontal R");
		bool joy5available = IsAxisAvailable("Vertical R");
		return (joy4available && joy5available);
	}

    private bool CheckBumpersAvailable() {
        bool bumpersAvailable = IsAxisAvailable("Bumper L") && IsAxisAvailable("Bumper R");
        return bumpersAvailable;
    }

    private bool CheckTriggersAvailable() {
        bool triggersavailable = IsAxisAvailable("Trigger L") && IsAxisAvailable("Trigger R");
        return triggersavailable;
    }

    private bool CheckStartBackAvailable() {
        bool startAvailable = IsAxisAvailable("Start");
        bool backAvailable = IsAxisAvailable("Back");
        return (startAvailable && backAvailable);
    }

	private bool IsAxisAvailable(string axisName)
	{
		try
		{
			Input.GetAxis(axisName);
			return true;
		}
		catch (System.Exception)
		{
			return false;
		}
	}

	private static float maxXangle = 60;
	private static float sensitivityX = 5;
	
	private float calculateStickXAngle() {
		float joy5 = 0;

		if (this.transform == ivr.leftHandTarget)
			joy5 -= Input.GetAxis("Vertical");
		else {
			if (joystick2available)
				joy5 -= Input.GetAxis("Vertical R");
		}

		if (joy5 != 0) {
			xAngle = joy5 * maxXangle;
			lastJoy5 = joy5;
		} else if (lastJoy5 != 0) {
			xAngle = 0;
			lastJoy5 = 0;
		}
		
		if (this.transform == ivr.rightHandTarget) {
			if (mouseInput)
			xAngle -= Input.GetAxis("Mouse Y") * sensitivityX;
		}

		xAngle = Mathf.Clamp (xAngle, -maxXangle, maxXangle);
		
		return xAngle;
	}
	
	[HideInInspector] private float xAngle = 0;
	[HideInInspector] private float yAngle = 0;
	[HideInInspector] private float lastJoy4, lastJoy5 = 0;

	private static float maxYangle = 70;
	private static float sensitivityY = 5;
	
	private float calculateStickYAngle() {
		float joy4 = 0;

		if (this.transform == ivr.leftHandTarget)
			joy4 = Input.GetAxis("Horizontal");
		else {
			if (joystick2available)
				joy4 = Input.GetAxis("Horizontal R");
		}

		if (joy4 != 0) {
			yAngle = joy4 * maxYangle;
			lastJoy4 = joy4;
		} else if (lastJoy4 != 0) {
			yAngle = 0;
			lastJoy4 = 0;
		}
		
		float correctedYAngle = NormalizeAngle180(yAngle);
		if (this.transform == ivr.rightHandTarget) {
			if (mouseInput)
				yAngle += Input.GetAxis("Mouse X") * sensitivityY;

			float deltaHipRot = NormalizeAngle180(ivr.hipTarget.localEulerAngles.y - hipStartRotationY);
			while (deltaHipRot - correctedYAngle > 180)		deltaHipRot -= 360;
			while (deltaHipRot - correctedYAngle < -180)	deltaHipRot += 360;

			float maxHipYangle = maxYangle + deltaHipRot;
			float minHipYangle = -maxYangle + deltaHipRot;
			correctedYAngle = Mathf.Clamp(correctedYAngle, minHipYangle, maxHipYangle);

			correctedYAngle = NormalizeAngle180(correctedYAngle);
		}
		
		return correctedYAngle;
	}

	private float NormalizeAngle180(float angle) {
		while (angle > 180)		angle -= 360;
		while (angle < -180)	angle += 360;
		return angle;
	}
}
