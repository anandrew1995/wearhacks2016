using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(IVR_BodyMovements))]
public class IVR_Editor : Editor {
	private IVR_BodyMovements bodyMovements;
	private GameObject avatarGameObject;

	private bool prefabPose = true;

	void OnDestroy() {
		if (bodyMovements == null) {
			if (avatarGameObject != null && prefabPose == false) {
				ResetAvatarToPrefabPose(avatarGameObject);
			}
		}
	}

	void Reset() {
        bodyMovements = (IVR_BodyMovements)target;
		InstantVR ivr = bodyMovements.GetComponent<InstantVR>();

		if (ivr == null) {
			Debug.LogWarning("Body Movements script requires Instant VR script on the game object");
			DestroyImmediate(bodyMovements);
			return;
		}

        IVR_BodyMovements[] bodyMovementsScripts = bodyMovements.GetComponents<IVR_BodyMovements>();
		if (bodyMovementsScripts.Length > 1) {
			Debug.LogError("You cant have more than one BodyMovements script");
			DestroyImmediate(bodyMovements); // why does it delete the first script, while target should be the new script...
			return;
		}

		Animator animator = bodyMovements.transform.GetComponentInChildren<Animator>();
		if (animator) {
			avatarGameObject = animator.gameObject;

			prefabPose = false;
			bodyMovements.StartMovements();
		}
	}

	private void ResetAvatarToPrefabPose(GameObject avatarGameObject) {
		Transform[] avatarBones = avatarGameObject.GetComponentsInChildren<Transform>();
		foreach (Transform bone in avatarBones)
			PrefabUtility.ResetToPrefabState(bone);
		prefabPose = true;
	}
}
