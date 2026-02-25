using UnityEngine;

public class PauseGameScript : MonoBehaviour
{
    void Update()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);

         bool anyPanelActive = false;
        foreach (Canvas canvas in canvases)
        {
             foreach (Transform panel in canvas.transform)
            {

                if (canvas.renderMode == RenderMode.WorldSpace && panel.CompareTag("Menu") && canvas.gameObject.activeInHierarchy)
                {
                 anyPanelActive = true;
                    break;
                }
            }
            if (anyPanelActive) 
                break;
        }
        Time.timeScale = anyPanelActive ? 0f : 1f;
     }
}
