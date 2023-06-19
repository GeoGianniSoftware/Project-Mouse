using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    PlayerMovement PM;

    public Slider staminaBar;
    // Start is called before the first frame update
    void Start()
    {
        PM = FindObjectOfType<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        staminaBar.maxValue = PM.maxStamina;
        if(PM.getCurrentStamina() == PM.maxStamina){
            staminaBar.fillRect.GetComponent<Image>().enabled = false;
        }
        else {
            staminaBar.fillRect.GetComponent<Image>().enabled = true;
            staminaBar.value = PM.getCurrentStamina();

        }
    }
}
