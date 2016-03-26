/* InstantVR Extension
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.2.2
 * date: December 23, 2015
 * 
 * - Added TrackerEulerAngles
 */

using UnityEngine;

public class IVR_Extension : MonoBehaviour {
    [HideInInspector]
    public int priority = -1;
    [HideInInspector]
    public bool present = false;

    // It is not always a real tracker (e.g. Lighthouse, Hydea), I know...
    public Vector3 trackerPosition = Vector3.zero;
    public Vector3 trackerEulerAngles = Vector3.zero;
    [HideInInspector]
    public Quaternion trackerRotation = Quaternion.identity;

    public virtual void StartExtension() {
        trackerRotation = Quaternion.Euler(trackerEulerAngles);
    }
    public virtual void UpdateExtension() {
        trackerRotation = Quaternion.Euler(trackerEulerAngles);
    }
    public virtual void LateUpdateExtension() { }
}

public class IVR_HandController : IVR_Controller {
    protected float thumbInput;
    protected float indexInput;
    protected float middleInput;
    protected float ringInput;
    protected float littleInput;
}

public class IVR_Controller : MonoBehaviour {

    [HideInInspector]
    protected InstantVR ivr;
    [HideInInspector] 
    public IVR_Extension extension;

    protected Vector3 startPosition;
    public Vector3 GetStartPosition() { return startPosition; }
    protected Vector3 position = Vector3.zero;

    protected Quaternion startRotation;
    public Quaternion GetStartRotation() { return startRotation; }
    protected Quaternion rotation = Quaternion.identity;

    protected bool extrapolation = false;

    protected bool present = false;
    public bool isPresent() {
        return present;
    }
    protected bool tracking = false;
    public bool isTracking() {
        return tracking;
    }
    public void setTracking(bool b) {
        tracking = b;
    }

    protected bool selected = false;
    public bool isSelected() {
        return selected;
    }
    public void SetSelection(bool selection) {
        selected = selection;
    }

    [HideInInspector]
    private float updateTime;

    [HideInInspector]
    private Vector3 lastPosition = Vector3.zero;
    [HideInInspector]
    private Quaternion lastRotation = Quaternion.identity;
    private Vector3 positionalVelocity = Vector3.zero;
    private float angularVelocity = 0;
    private Vector3 velocityAxis = Vector3.one;

    void Start() {
        updateTime = Time.time;
    }

    public virtual void StartController(InstantVR ivr) {
        this.ivr = ivr;

        startPosition = transform.position - ivr.transform.position;
        startRotation = Quaternion.Inverse(ivr.transform.rotation) * transform.rotation;

        lastPosition = startPosition;
        lastRotation = startRotation;
    }

    public virtual void OnTargetReset() {
        if (selected)
            Calibrate(true);
    }

    public void Calibrate(bool calibrateOrientation) {
        //extension.trackerPosition = ...

        if (calibrateOrientation) {
            extension.trackerRotation = Quaternion.Inverse(rotation);
        }
    }

    public void TransferCalibration(IVR_Controller lastController) {
    }

    [HideInInspector]
    private bool indirectUpdate = false;

    public virtual void UpdateController() {
        if (selected) { // this should be moved out of here in the future, because it removes the possibility to combine controllers
            Vector3 localPosition = extension.trackerPosition + extension.trackerRotation * position;
            Quaternion localRotation = extension.trackerRotation * rotation;

            Vector3 newPosition = ivr.transform.position + ivr.transform.rotation * localPosition;
            Quaternion newRotation = ivr.transform.rotation * localRotation;

            if (extrapolation == false) {
                this.transform.position = newPosition;
                this.transform.rotation = newRotation;
            } else {
                float deltaTime = Time.time - updateTime;

                if (deltaTime > 0) {
                    float angle = 0;
                    Quaternion rotationalChange = Quaternion.Inverse(lastRotation) * newRotation;
                    rotationalChange.ToAngleAxis(out angle, out velocityAxis);
                    if (angle == 0)
                        velocityAxis = Vector3.one;
                    else {
                        while (angle < -180) angle += 360;
                        while (angle > 180) angle -= 360;
                    }

                    positionalVelocity = (newPosition - lastPosition) / deltaTime;
                    angularVelocity = angle / deltaTime;

                    lastPosition = newPosition;
                    lastRotation = newRotation;

                    updateTime = Time.time;
                    indirectUpdate = true;
                }
            }
        }
    }

    void Update() {
        if (indirectUpdate) {
            float dTime = Time.time - updateTime;
            if (dTime < 0.1f) { // do not extrapolate for more than 1/10th second
                this.transform.position = lastPosition + positionalVelocity * dTime;
                this.transform.rotation = lastRotation * Quaternion.AngleAxis(angularVelocity * dTime, velocityAxis);
            } else {
                indirectUpdate = false;
            }
        }
    }

}
