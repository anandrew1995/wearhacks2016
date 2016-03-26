/* InstantVR Oculus Rift / Samsung Gear VR extension editor
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.2.2
 * date: December 28, 2015
 * 
 * - Added Warning for default tracker position
 */

using UnityEditor;

[CustomEditor(typeof(IVR_RiftGearVR))] 
public class IVR_RiftGearVR_Editor : IVR_Extension_Editor {

	private InstantVR ivr;
	private IVR_RiftGearVR ivrRift;

	private IVR_RiftGearVRHead riftHead;

    public override void OnInspectorGUI() {
#if !UNITY_ANDROID
        if (PlayerSettings.virtualRealitySupported == false)
            EditorGUILayout.HelpBox("VirtualRealitySupported needs to be enabled in Player Settings for Rift support", MessageType.Warning, true);

        ivrRift = (IVR_RiftGearVR)target;
        if (ivrRift.trackerPosition == new UnityEngine.Vector3(0, 2, 1))
            EditorGUILayout.HelpBox("Please check the tracker position with the physical position of the Rift Camera", MessageType.Warning, true);

#else
        if (PlayerSettings.virtualRealitySupported == false)
            EditorGUILayout.HelpBox("VirtualRealitySupported needs to be enabled in Player Settings for Gear VR support", MessageType.Warning, true);
#endif
        base.OnInspectorGUI();
    }

    void OnDestroy() {
		if (ivrRift == null && ivr != null) {
			riftHead = ivr.headTarget.GetComponent<IVR_RiftGearVRHead>();
			if (riftHead != null)
				DestroyImmediate(riftHead, true);
		}
	}
	
	void OnEnable() {

		ivrRift = (IVR_RiftGearVR) target;
		ivr = ivrRift.GetComponent<InstantVR>();

		if (ivr != null) {
			riftHead = ivr.headTarget.GetComponent<IVR_RiftGearVRHead>();
			if (riftHead == null) {
				riftHead = ivr.headTarget.gameObject.AddComponent<IVR_RiftGearVRHead>();
				riftHead.extension = ivrRift;
			}

			IVR_Extension[] extensions = ivr.GetComponents<IVR_Extension>();
			if (ivrRift.priority == -1)
				ivrRift.priority = extensions.Length - 1;
			for (int i = 0; i < extensions.Length; i++) {
				if (ivrRift == extensions[i]) {
					while (i < ivrRift.priority) {
						MoveUp(riftHead);
						ivrRift.priority--;
						//Debug.Log ("Rift Move up to : " + i + " now: " + ivrRift.priority);
					}
					while (i > ivrRift.priority) {
						MoveDown(riftHead);
						ivrRift.priority++;
						//Debug.Log ("Rift Move down to : " + i + " now: " + ivrRift.priority);
					}
				}
			}
		}
	}
}
