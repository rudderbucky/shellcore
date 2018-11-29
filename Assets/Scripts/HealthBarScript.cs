using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarScript : MonoBehaviour {

    public UnityEngine.UI.Image[] barsInputArray;
    private UnityEngine.UI.Image[] barsArray;
    public UnityEngine.UI.Image[] gleamInputArray;
    private UnityEngine.UI.Image[] gleamArray;
    private bool initialized;
    private bool[] gleaming;
    private bool[] gleamed;
    /*public UnityEngine.UI.Image shell; // shell bar
    public UnityEngine.UI.Image gleamShell;
    public UnityEngine.UI.Image core; // core bar
    public UnityEngine.UI.Image gleamCore;
    public UnityEngine.UI.Image energy; // energy bar
    public UnityEngine.UI.Image gleamEnergy;*/
    public PlayerCore player; // associated player

    public void Initialize()
    {
        barsArray = new UnityEngine.UI.Image[barsInputArray.Length];
        gleamArray = new UnityEngine.UI.Image[barsInputArray.Length];
        gleaming = new bool[barsArray.Length];
        gleamed = new bool[barsArray.Length];
        for (int i = 0; i < barsArray.Length; i++) {
            barsArray[i] = Instantiate(barsInputArray[i]) as UnityEngine.UI.Image;
            barsArray[i].fillAmount = 0;
            barsArray[i].transform.SetParent(transform, false);

            gleamArray[i] = Instantiate(gleamInputArray[i]) as UnityEngine.UI.Image;
            gleamArray[i].fillAmount = 0;
            gleamArray[i].transform.SetParent(transform, false);
        }
        initialized = true;
    }
    public void Deinitialize() {
        for (int i = 0; i < barsArray.Length; i++) {
            Destroy(barsArray[i].gameObject);
            Destroy(gleamArray[i].gameObject);
        }
    }

    private void Gleam(int index) {
        gleamArray[index].fillAmount = barsArray[index].fillAmount;
        Color tmpColor = gleamArray[index].color;
        tmpColor.a = Mathf.Max(0, tmpColor.a - 0.05F);
        gleamArray[index].color = tmpColor;
        if (tmpColor.a == 0) gleaming[index] = false;
    }

    private float UpdateBar(float fillAmount, float currentHealth, float maxHealth) {
        if (fillAmount < currentHealth / maxHealth)
        {
            fillAmount += 0.05F;
            if (fillAmount > currentHealth / maxHealth)
            {
                return currentHealth / maxHealth;
            }
            else return fillAmount;
        }
        else return currentHealth / maxHealth;
    }

    private void Update()
    {
        if (initialized)
        {
            float[] currentHealth = player.GetHealth();
            float[] maxHealth = player.GetMaxHealth();
            for (int i = 0; i < currentHealth.Length; i++)
            {
                if (gleaming[i])
                {
                    Gleam(i);
                }
                if (barsArray[i].fillAmount == UpdateBar(barsArray[i].fillAmount, currentHealth[i], maxHealth[i]) && !gleamed[i])
                {
                    gleamed[i] = true;
                    gleaming[i] = true;
                }
                else barsArray[i].fillAmount = UpdateBar(barsArray[i].fillAmount, currentHealth[i], maxHealth[i]);
            }
            // set fill amounts
            /*if (shell.fillAmount < player.GetShell() / player.GetShellMax()) {
                shell.fillAmount += 0.05F;
            } else shell.fillAmount = player.GetShell() / player.GetShellMax();
            core.fillAmount = player.GetCore() / player.GetCoreMax();
            energy.fillAmount = player.GetEnergy() / player.GetEnergyMax();
            */
            if (barsArray[0].fillAmount > barsArray[1].fillAmount)
            {
                barsArray[0].transform.SetAsFirstSibling(); // sets the shell to render first so core texture doesn't get overlapped
            }
            else if (barsArray[0].fillAmount <= barsArray[1].fillAmount) // explicitly done to minimize method calls
            {
                barsArray[1].transform.SetAsFirstSibling(); // vice-versa
            }
        }
    } 
}
