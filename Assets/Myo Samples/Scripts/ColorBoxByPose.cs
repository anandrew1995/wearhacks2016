using UnityEngine;
using System.Collections;

using LockingPolicy = Thalmic.Myo.LockingPolicy;
using Pose = Thalmic.Myo.Pose;
using UnlockType = Thalmic.Myo.UnlockType;
using VibrationType = Thalmic.Myo.VibrationType;

// Change the material when certain poses are made with the Myo armband.
// Vibrate the Myo armband when a fist pose is made.
public class ColorBoxByPose : MonoBehaviour
{
    // Myo game object to connect with.
    // This object must have a ThalmicMyo script attached.
    public GameObject myo = null;
	public GameObject playerCamera;

    // Materials to change to when poses are made.
    public Material waveInMaterial;
    public Material waveOutMaterial;
    public Material doubleTapMaterial;

	public GameObject kittenObject;
	private Vector3 charPosition;

    // The pose from the last update. This is used to determine if the pose has changed
    // so that actions are only performed upon making them rather than every frame during
    // which they are active.
    private Pose _lastPose = Pose.Unknown;

    // Update is called once per frame.
    void Update ()
    {
        // Access the ThalmicMyo component attached to the Myo game object.
        ThalmicMyo thalmicMyo = myo.GetComponent<ThalmicMyo> ();

        // Check if the pose has changed since last update.
        // The ThalmicMyo component of a Myo game object has a pose property that is set to the
        // currently detected pose (e.g. Pose.Fist for the user making a fist). If no pose is currently
        // detected, pose will be set to Pose.Rest. If pose detection is unavailable, e.g. because Myo
        // is not on a user's arm, pose will be set to Pose.Unknown.
        if (thalmicMyo.pose != _lastPose) {
            _lastPose = thalmicMyo.pose;

            // Vibrate the Myo armband when a fist is made.
			if (thalmicMyo.pose == Pose.Fist) {
				//thalmicMyo.Vibrate (VibrationType.Medium);
				if (kittenObject.GetComponent<ThirdPersonController> ().nearMaster) {
					Debug.Log ("start RPS");
					kittenObject.GetComponent<rockPaperScissors> ().Play ();
				}

				//Player plays a hand
				if (kittenObject.GetComponent<rockPaperScissors> ().playerRdy) {
					Debug.Log ("player hand is rock");
					kittenObject.GetComponent<rockPaperScissors> ().setPlayerHand ("rock");
					kittenObject.GetComponent<rockPaperScissors> ().playerHand = true;
				}

				ExtendUnlockAndNotifyUserAction (thalmicMyo);

				// Change material when wave in, wave out or double tap poses are made.
			} else if (thalmicMyo.pose == Pose.WaveIn) {
				GetComponent<Renderer> ().material = waveInMaterial;
				//Move the cat towards the user
				//1. get user position
				//2. move kitten towards it
				charPosition = playerCamera.transform.position;
				Vector3 kittenPosition = kittenObject.transform.position;
				//Vector3 distance = kittenObject - charPosition;
				kittenObject.GetComponent<ThirdPersonController> ().toggleGesture (charPosition);
				//kittenObject.GetComponent<ThirdPersonController>().moveTowardsUser();
				Debug.Log ("Wave in");

				//Player plays a hand
				if (kittenObject.GetComponent<rockPaperScissors> ().playerRdy) {
					Debug.Log ("player hand is scissors");
					kittenObject.GetComponent<rockPaperScissors> ().setPlayerHand ("scissors");
					kittenObject.GetComponent<rockPaperScissors> ().playerHand = true;
				}

				ExtendUnlockAndNotifyUserAction (thalmicMyo);
			} else if (thalmicMyo.pose == Pose.WaveOut) {
				GetComponent<Renderer> ().material = waveOutMaterial;

				Debug.Log ("Wave Out");
				kittenObject.GetComponent<ThirdPersonController> ().toggleWaveOutGesture ();
				kittenObject.GetComponent<rockPaperScissors> ().DonePlaying ();

				ExtendUnlockAndNotifyUserAction (thalmicMyo);
			} else if (thalmicMyo.pose == Pose.DoubleTap) {
				GetComponent<Renderer> ().material = doubleTapMaterial;

				Debug.Log ("Double Tap");
				kittenObject.GetComponent<ThirdPersonController> ().meowAnimation ();

				ExtendUnlockAndNotifyUserAction (thalmicMyo);
			} else if (thalmicMyo.pose == Pose.FingersSpread) {
				Debug.Log ("finger spread");
				//Player plays a hand
				if (kittenObject.GetComponent<rockPaperScissors> ().playerRdy) {
					Debug.Log ("player hand is paper");
					kittenObject.GetComponent<rockPaperScissors> ().setPlayerHand ("paper");
					kittenObject.GetComponent<rockPaperScissors> ().playerHand = true;
				}
			}
        }
    }

    // Extend the unlock if ThalmcHub's locking policy is standard, and notifies the given myo that a user action was
    // recognized.
    void ExtendUnlockAndNotifyUserAction (ThalmicMyo myo)
    {
        ThalmicHub hub = ThalmicHub.instance;

        if (hub.lockingPolicy == LockingPolicy.Standard) {
            myo.Unlock (UnlockType.Timed);
        }

        myo.NotifyUserAction ();
    }
}
