using UnityEngine;
using UnityEngine.InputSystem;

public class test : BaseBehaviour
{
    public GameObject GameObject;

    private GameObject testtest;

    private void Update()
    {
        if (Keyboard.current[Key.Space].wasPressedThisFrame)
        {
            testtest = Instantiate(GameObject);
        }

        if (Keyboard.current[Key.Escape].wasPressedThisFrame)
        {
            Destroy(testtest);
        }
    }
}
