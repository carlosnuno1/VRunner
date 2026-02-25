using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OpenWatchScript : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] public GameObject watchPanel;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference openMenuAction;


    private bool isWatchOpen = false;

    private void OnEnable()
    {
        openMenuAction.action.performed += openMenuActionPerformed;
        openMenuAction.action.Enable();
    }

    private void OnDisable()
    {
        openMenuAction.action.performed -= openMenuActionPerformed;
        openMenuAction.action.Disable();
    }

    private void openMenuActionPerformed(InputAction.CallbackContext context)
    {
        isWatchOpen = !isWatchOpen;
        watchPanel.SetActive(isWatchOpen);
    }

}
