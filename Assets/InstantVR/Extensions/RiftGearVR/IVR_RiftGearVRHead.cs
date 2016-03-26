/* InstantVR Oculus Rift head controller
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.2.3
 * date: December 29, 2015
 * 
 * - Fixed starting point when positional tracking is started
 */

using UnityEngine;

using UnityEngine.VR;

public class IVR_RiftGearVRHead : IVR_Controller {
#if (UNITY_STANDALONE_WIN || UNITY_ANDROID)

    [HideInInspector]
    private GameObject headcamRoot;
    [HideInInspector]
    private Transform headcam;

    [HideInInspector]
    private Vector3 localNeckOffset;

    // This dummy code is here to ensure the checkbox is present in editor
    void Start() { }

    public override void StartController(InstantVR ivr) {
        if (VRSettings.enabled) {
            if (extension == null)
                extension = ivr.GetComponent<IVR_RiftGearVR>();

            present = VRDevice.isPresent;
            extension.present = present;

            headcam = this.transform.FindChild("Headcam");
            localNeckOffset = headcam.localPosition;

            headcamRoot = new GameObject("HeadcamRoot");
            headcamRoot.transform.parent = ivr.transform;
            headcamRoot.transform.position = this.transform.position;
            headcamRoot.transform.rotation = this.transform.rotation;

            headcam.parent = headcamRoot.transform;

            base.StartController(ivr);
            this.position = startPosition - extension.trackerPosition;
            this.rotation = Quaternion.identity;

            OVRManager ovrManager = this.gameObject.AddComponent<OVRManager>();
            ovrManager.resetTrackerOnLoad = true;

            positionTracking = false;
        }
    }

    public override void UpdateController() {
        if (present && enabled) {
            UpdateRift();
        } else {
            tracking = false;
        }
    }

    private bool positionTracking = false;
    private void UpdateRift() {
        Quaternion calibrationRotation = extension.trackerRotation * Quaternion.AngleAxis(180, Vector3.up) * Quaternion.Inverse(OVRManager.tracker.GetPose(0).orientation);

        if (OVRManager.tracker.isPositionTracked) {
            if (!positionTracking) {

                Vector3 eye2tracker =
                    (calibrationRotation * -OVRManager.tracker.GetPose(0).position)
                    + (calibrationRotation * InputTracking.GetLocalPosition(VRNode.Head));

                Vector3 worldEyePosition = ivr.transform.position + ivr.transform.rotation * (extension.trackerPosition + eye2tracker);

                headcamRoot.transform.Translate(worldEyePosition - headcam.position, Space.World);
                headcamRoot.transform.rotation = ivr.transform.rotation * calibrationRotation;

                positionTracking = true;
            }
        } else {
            positionTracking = false;
            tracking = true;
        }

        Quaternion inverseIvrRotation = Quaternion.Inverse(ivr.transform.rotation);
        Quaternion inverseTrackerRotation = Quaternion.Inverse(extension.trackerRotation);

        this.rotation = inverseIvrRotation * inverseTrackerRotation * headcam.rotation;
        
        Vector3 localPosition = inverseIvrRotation * (headcam.position - ivr.transform.position);
        Vector3 localNeckPosition = localPosition - extension.trackerRotation * this.rotation * localNeckOffset;
        this.position = inverseTrackerRotation * (localNeckPosition - extension.trackerPosition);
        /*
        Debug.DrawRay(ivr.transform.position + ivr.transform.rotation * ( extension.trackerPosition),
            ivr.transform.rotation * (calibrationRotation * -OVRManager.tracker.GetPose(0).position), Color.green);
        Debug.DrawRay(ivr.transform.position + ivr.transform.rotation * ( extension.trackerPosition - (calibrationRotation * OVRManager.tracker.GetPose(0).position)),
            ivr.transform.rotation * (-(calibrationRotation * -InputTracking.GetLocalPosition(VRNode.Head))), Color.red);
        Debug.DrawRay(ivr.transform.position + ivr.transform.rotation * ( extension.trackerPosition - (calibrationRotation * OVRManager.tracker.GetPose(0).position)
            - (calibrationRotation * -InputTracking.GetLocalPosition(VRNode.Head))), ivr.transform.rotation * (-(this.rotation * localNeckOffset)), Color.blue);
        */
        base.UpdateController();
    }

    public override void OnTargetReset() {
        extension.trackerPosition = startPosition - this.position;
        positionTracking = false;
    }
#endif
}
