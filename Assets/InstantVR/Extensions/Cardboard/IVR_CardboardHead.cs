/* InstantVR Cardboard head controller
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.1.0
 * date: August 19, 2015
 * 
 * - Bugfixes
 */

using UnityEngine;
#if !UNITY_4_5
using UnityEngine.EventSystems;
#endif

public class IVR_CardboardHead : IVR_Controller {
#if !UNITY_4_5
    [Tooltip("Adds a PhysicsRaycaster for gaze selection")]
    public bool raycaster = false;
#endif

    private Cardboard cardboard;

    [HideInInspector]
    private Vector3 cardboardStartPoint;
    [HideInInspector]
    private Vector3 baseStartPoint;
    [HideInInspector]
    private Quaternion cardboardStartRotation;
    [HideInInspector]
    private float baseStartAngle;

    public override void StartController(InstantVR ivr) {
        base.StartController(ivr);
#if UNITY_ANDROID
        Transform headcam = this.transform.FindChild("Headcam");
        if (headcam != null) {
            headcam.gameObject.SetActive(false);

            Cardboard cardboardPrefab = Resources.Load<Cardboard>("CardboardPrefab");
            cardboard = (Cardboard)Instantiate(cardboardPrefab, headcam.position, headcam.rotation);


            if (cardboard == null)
                Debug.LogError("Could not instantiate Cardboard. CardboardCameraRig is missing?");
            else {
                cardboard.transform.parent = ivr.transform;

                cardboardStartPoint = cardboard.transform.position;
                baseStartPoint = ivr.transform.position;
                cardboardStartRotation = cardboard.transform.rotation;
                baseStartAngle = ivr.transform.eulerAngles.y;

                present = true;
                if (!present) {
                    cardboard.gameObject.SetActive(false);
                    headcam.gameObject.SetActive(true);
                } else
                    headcam.gameObject.SetActive(false);
#if !UNITY_4_5
                if (raycaster) {
                    GameObject mainCamera = cardboard.GetComponentInChildren<StereoController>().gameObject;
                    mainCamera.AddComponent<PhysicsRaycaster>();
                }
#endif
            }
            startRotation = Quaternion.Inverse(ivr.transform.rotation);
        }
#endif
    }

    public override void UpdateController() {
#if UNITY_ANDROID
        if (present && this.enabled)
            UpdateCardboard();
        else
            tracking = false;
#endif
    }

#if UNITY_ANDROID
    private void UpdateCardboard() {
        if (cardboard != null) {
            Vector3 baseDelta = ivr.transform.position - baseStartPoint;
            cardboard.transform.position = cardboardStartPoint + baseDelta;

            float baseAngleDelta = ivr.transform.eulerAngles.y - baseStartAngle;
            cardboard.transform.rotation = cardboardStartRotation * Quaternion.AngleAxis(baseAngleDelta, Vector3.up);

            if (cardboard.HeadPose != null) {
                this.position = startPosition;
                this.rotation = cardboard.HeadPose.Orientation * Quaternion.AngleAxis(-baseAngleDelta, Vector3.up); ;

                if (!tracking) {
                   //Calibrate(false);
                    tracking = true;
                }

                base.UpdateController();
            }
        }
    }
#endif
}
