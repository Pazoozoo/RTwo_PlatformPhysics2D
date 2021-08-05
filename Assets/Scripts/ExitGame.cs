using UnityEngine;

public class ExitGame : MonoBehaviour {

#if !UNITY_EDITOR
    void FixedUpdate() {
        if (Input.GetButton("Cancel")) 
            Application.Quit();
    }
#endif
}

