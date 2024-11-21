using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent (typeof (Animator))]
public class NPCController : MonoBehaviour {

    [System.Serializable]
    public struct Arsenal
    {
        public string name;
        public GameObject rightGun;
        public GameObject leftGun;
        public RuntimeAnimatorController controller;
    }

    [SerializeField] private Transform rightGunBone;
    [SerializeField] private Transform leftGunBone;
    [SerializeField] private Arsenal[] arsenal;

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator> ();

        int index = Random.Range(1, arsenal.Length);
        if (arsenal.Length > 0)
			SetArsenal (arsenal[index].name); //Setup NPC to be holding a randomly selected weapon from the arsenal array other than 'Empty' at startup.
	}

	public void SetArsenal(string name) {
		foreach (Arsenal hand in arsenal) {
			if (hand.name == name) {
				if (rightGunBone.childCount > 0)
					Destroy(rightGunBone.GetChild(0).gameObject);
				if (leftGunBone.childCount > 0)
					Destroy(leftGunBone.GetChild(0).gameObject);
				if (hand.rightGun != null) {
					GameObject newRightGun = (GameObject) Instantiate(hand.rightGun);
					newRightGun.transform.parent = rightGunBone;
					newRightGun.transform.localPosition = Vector3.zero;
					newRightGun.transform.localRotation = Quaternion.Euler(90, 0, 0);
					}
				if (hand.leftGun != null) {
					GameObject newLeftGun = (GameObject) Instantiate(hand.leftGun);
					newLeftGun.transform.parent = leftGunBone;
					newLeftGun.transform.localPosition = Vector3.zero;
					newLeftGun.transform.localRotation = Quaternion.Euler(90, 0, 0);
				}
				animator.runtimeAnimatorController = hand.controller;
				return;
			}
		}
	}
}
