using UnityEngine;

public class MouseTracker : MonoBehaviour
{
	private void Update()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Debug.DrawRay(hit.point, hit.normal * 2);
        }
	}
}
