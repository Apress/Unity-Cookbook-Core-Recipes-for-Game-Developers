using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;
using UnityEngine.EventSystems;

public class ActionMapManager : MonoBehaviour
{
    public InputActionAsset actionAsset; // Assign this in the Inspector
    public GameObject gameUICanvas;     //  Assign this in the Inspector
    public GameObject rebindControlsCanvas; // ACC - Assign this in the Inspector
    public GameObject gameUIDefaultButton; // Assign this in the Inspector

    private PlayerInput playerInput; 
    private StarterAssetsInputs starterAssetsInput; // ACC
    private InputActionMap playerMap;
    private InputActionMap uiMap;
    private InputActionMap universalMap;
    private bool isActiveGameUICanvas = true;
    private InputAction toggleUIAction;
    private InputAction toggleRebindControlsAction; // ACC 
    
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        starterAssetsInput = GetComponent<StarterAssetsInputs>(); // ACC
        
        //cursorLocked should be false to receive standard input when using Rebind Controls UI.
        starterAssetsInput.cursorLocked = false; // ACC

        // Get all three action maps
        playerMap = actionAsset.actionMaps[0];
        uiMap = actionAsset.actionMaps[1];
        universalMap = actionAsset.actionMaps[2];
        Debug.Log($"playerMap points to : {playerMap.name} uiMap points to : {uiMap.name} universalMap points to : {universalMap.name}");

        // Get the ToggleUI and ToggleRbindControls action
        toggleUIAction = universalMap.actions[0];
        toggleRebindControlsAction = universalMap.actions[1]; // ACC
    }

    private void OnEnable()
    {
        toggleUIAction.performed += ToggleUIPerformed;
        toggleRebindControlsAction.performed += ToggleRebindControlsPerformed;  // ACC

        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
            actionAsset.LoadBindingOverridesFromJson(rebinds);
    }
    private void OnDisable()
    {
        toggleUIAction.performed -= ToggleUIPerformed;
        toggleRebindControlsAction.performed -= ToggleRebindControlsPerformed; // ACC
    }
    
    private void ToggleRebindControlsPerformed(InputAction.CallbackContext context) // ACC
    {
        //Universal Map Needs to be enabled as it holds the Touch Key binding
        universalMap.Enable();

        //Disable playerInput so that Rebind Controls UI can receive standard input.
        playerInput.enabled = false;
        rebindControlsCanvas.SetActive(true);
    }
    
    private void ToggleUIPerformed(InputAction.CallbackContext context)
    {

        isActiveGameUICanvas = !isActiveGameUICanvas;

        gameUICanvas.SetActive(isActiveGameUICanvas);

        if (!isActiveGameUICanvas)
            playerMap.Enable();
        else
        {
            EventSystem.current.SetSelectedGameObject(gameUIDefaultButton);
            playerMap.Disable();
        }
        
        Debug.Log($"PlayerMap Enabled: {playerMap.enabled} ; UIMap Enabled: {uiMap.enabled} ; " +
                  $"UniversalMap Enabled: {universalMap.enabled}");
        Debug.Log($"Current Action Map is: {playerInput.currentActionMap.name}");
     
        //Universal Map Needs to be enabled at all times as it holds the Esc Key binding
        universalMap.Enable(); 
    }

    public void OnrebindControlsCanvasClose() // ACC
    {
        rebindControlsCanvas.SetActive(false);
        playerInput.enabled = true;

        if (isActiveGameUICanvas)
        {
            universalMap.Enable();
            playerMap.Disable();
            EventSystem.current.SetSelectedGameObject(gameUIDefaultButton); 
        }
    }
}
