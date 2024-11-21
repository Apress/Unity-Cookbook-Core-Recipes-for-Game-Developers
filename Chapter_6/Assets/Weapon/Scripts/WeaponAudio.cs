using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WeaponAudio : MonoBehaviour
{
    //Different Audio sounds
    [SerializeField][Tooltip("Sound played when Weapon fires")] private AudioClip fireSound;
    [SerializeField][Tooltip("Dry Fire Sound played when Magazine empty or No Magazine in weapon")] private AudioClip dryFireSound;
    [SerializeField][Tooltip("Sound played when Magazine loads/Unloads within Weapon")] private AudioClip magazineLoadedSound;

    private Weapon weapon; //A reference to the Weapon component on this game object that will be using this script.
    private AudioSource audioSource;

    private void Awake()
    {
        weapon = GetComponent<Weapon>(); //cache a reference to the weapon component.
    }

    private void OnEnable()
    {
        weapon.OnWeaponFired += WeaponFired; //this gets invoked when a bullet is fired.
        weapon.OnWeaponDryFired += DryFire;//Gets invoked when weapon has either no magzine or magazine has no bullets
        weapon.OnMagazineAttached += MagazineLoad; //this will be invoked when a magazine is loaded
        weapon.OnMagazineDetatched += MagazineUnLoad; //this will be invoked when a magazine is unloaded
    }

    private void OnDisable()
    {
        weapon.OnWeaponFired -= WeaponFired; //this gets invoked when a bullet is fired.
        weapon.OnWeaponDryFired -= DryFire;//Gets invoked when weapon has either no magzine or magazine has no bullets
        weapon.OnMagazineAttached -= MagazineLoad;
        weapon.OnMagazineDetatched -= MagazineUnLoad;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>(); // cache the audio source as you will use it in various methods
    }

    void DryFire() // When weapon tries to fire without ammo in it
    {
        //Debug.Log("Playing Dry Fire - No Magazine/Ammo Sound");
        audioSource.PlayOneShot(dryFireSound);
    }

    void MagazineLoad() //When magazine is loaded into  weapon
    {
        audioSource.PlayOneShot(magazineLoadedSound);
    }

    void MagazineUnLoad() //When magazine is unloaded from  weapon
    {
        audioSource.PlayOneShot(magazineLoadedSound);
    }

    void WeaponFired()
    {
        //Debug.Log("Sound of Weapon Fired");
        audioSource.PlayOneShot(fireSound);
    }
}
