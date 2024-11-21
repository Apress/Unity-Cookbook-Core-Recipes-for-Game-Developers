using UnityEngine;

[RequireComponent (typeof (Animator))]
[RequireComponent(typeof(Health))]

public class Actions : MonoBehaviour {

	private Animator animator;

	const int countOfDamageAnimations = 3;
	int lastDamageAnimation = -1;

    /***
	 * This Commented section of code is something you could use along with the OnWeaponDryFired event to play a Weapon reload animation
	 * for the NPC if you had one. You would also need to setup a new WepaonReload behavior for your BehaviorFSM too. 
    private void Start()
    {
		GetComponentInChildren<Weapon>().OnWeaponDryFired += Stay;
    }

    private void OnDestroy()
    {
		Weapon weaponComponent = GetComponentInChildren<Weapon>();
        if (weaponComponent != null)
            weaponComponent.OnWeaponDryFired -= Stay;
    }
	***/

    private void OnEnable()
    {
		GetComponent<IdleState>().OnNpcIdle += Idle;
        GetComponent<PatrolState>().OnNpcPatrol += Walk;
        GetComponent<ChaseState>().OnNpcChase += Run;
        GetComponent<AttackState>().OnNpcAttack += Attack;
        GetComponent<DeathState>().PlayNpcDeadAnim += Death;
        GetComponent<HitState>().PlayNpcHitAnim += Damage;
        GetComponent<WanderState>().OnNpcWander += Walk;
        GetComponent<CoverState>().OnNpcTakeCover += CrouchingRun;
        GetComponent<CoverState>().OnNPCSquat += Squat;
    }

    private void OnDisable()
    {
        GetComponent<IdleState>().OnNpcIdle -= Idle;
        GetComponent<PatrolState>().OnNpcPatrol -= Walk;
        GetComponent<ChaseState>().OnNpcChase -= Run;
        GetComponent<AttackState>().OnNpcAttack -= Attack;
        GetComponent<DeathState>().PlayNpcDeadAnim -= Death;
        GetComponent<HitState>().PlayNpcHitAnim -= Damage;
        GetComponent<WanderState>().OnNpcWander -= Walk;
        GetComponent<CoverState>().OnNpcTakeCover -= CrouchingRun;
        GetComponent<CoverState>().OnNPCSquat -= Squat;
    }
    void Awake () {
		animator = GetComponent<Animator> ();
	}

    //Invoked from the IdleState script 
    private void Idle () {
		animator.SetBool("Aiming", false);
		animator.SetFloat ("Speed", 0f);
	}

    //Invoked from the Patrol/Wander state scripts
    private void Walk () {
		animator.SetBool("Aiming", false);
        animator.SetBool("Squat", false);
        animator.SetFloat ("Speed", 0.5f);
	}

    //Invoked from the Chase state script
    private void Run () {
		animator.SetBool("Aiming", false);
        animator.SetBool("Squat", false);
        animator.SetFloat ("Speed", 0.7f);
	}

    private void CrouchingRun()
    {
        animator.SetBool("Squat", true);
        animator.SetFloat("Speed", 0.7f);
    }

    //Invoked from the Attack state script
    private void Attack () { 
		Aiming ();
		animator.SetTrigger ("Attack");
	}

    //Invoked from the Death state script
    private void Death () {
        //if (animator.GetCurrentAnimatorStateInfo (0).IsName ("Death"))
        //	animator.Play("Idle", 0);
        //else
        animator.SetFloat("Speed", 0f);
        animator.SetTrigger ("Death");

    }

    //Invoked from the HitState script.
    private void Damage () { 
		if (animator.GetCurrentAnimatorStateInfo (0).IsName ("Death")) return;

        // Randomly choose a damage animation to play, ensuring it's different from the last played
        int id = Random.Range(0, countOfDamageAnimations);
        while (countOfDamageAnimations > 1 && id == lastDamageAnimation)
            id = Random.Range(0, countOfDamageAnimations);

        lastDamageAnimation = id;
        animator.SetInteger("DamageID", id);
        animator.SetTrigger("Damage");
	}

    //Can be invoked from a state that requires the NPC to Jump 
    public void Jump () {
		animator.SetBool ("Squat", false);
		animator.SetFloat ("Speed", 0f);
		animator.SetBool("Aiming", false);
		animator.SetTrigger ("Jump");
	}

    //Invoked from the Attack() method above. Can also be invoked directly when NPC is in shooting range of the player 
    //and possibly if player is not facing the NPC.
    private void Aiming () { 
		animator.SetBool ("Squat", false);
		animator.SetFloat ("Speed", 0f);
		animator.SetBool("Aiming", true);
	}

	//Can be utilized when NPC hiding from Player
	public void Squat () {
		//animator.SetBool ("Squat", !animator.GetBool("Squat"));
        animator.SetBool("Squat", true);
        animator.SetBool("Aiming", false);
        animator.SetFloat("Speed", 0f);
    }
}
