using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class mainMenu_Script : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void PlayGame()
    {
         StartCoroutine(Playgame());
    }

    IEnumerator Playgame()
    {
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadSceneAsync (1);
    }
}
