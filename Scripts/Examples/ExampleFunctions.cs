using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fjord.XRInteraction.Examples
{
    public class ExampleFunctions : MonoBehaviour
    {
        public void LoadLevel(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}