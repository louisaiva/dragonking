using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Being : MonoBehaviour, I_HasUI
{

    // Basic properties
    public string name;
    public int age;
    public int gender; // 0 = male, 1 = female, 2 = other

    // Race and origin
    public string race;
    public string origin;
    public string classe;


    // social net
    private SocialNet socialNet;

    // ui
    public GameObject ui {get; set;}


    // unity functions

    public void init(SocialNet socialNet){

        // social net
        this.socialNet = socialNet;

        // init ui
        InitUI();

    }

    public void Update(){
        


    }

    // getters

    public string GetName(){
        return name;
    }

    public int GetAge(){
        return age;
    }

    public string GetRace(){
        return race;
    }

    public string GetOrigin(){
        return origin;
    }

    public string GetClasse(){
        return classe;
    }

    public int GetReputation(){
        return Mathf.FloorToInt(socialNet.GetReputation(this));
    }

    public string Get(string cat){

        switch (cat)
        {
            case "age":
                return age.ToString();
            case "name":
                return name;
            case "race":
                return race;
            case "origin":
                return origin;
            case "classe":
                return classe;
            case "reputation":
                // Debug.Log("get reputation of " + name);
                return Mathf.FloorToInt(socialNet.GetReputation(this)).ToString();
        }
        return "/";

    }

    // ui
    public void ShowUI()
    {
        ui.SetActive(true);
    }

    public void HideUI()
    {
        ui.SetActive(false);
    }

    public void SwitchUI()
    {
        ui.SetActive(!ui.activeSelf);
    }

    void InitUI(){
        // create ui
        ui = Instantiate(Resources.Load("Prefabs/UI/beingUI"),GameObject.Find("/canva/beings").transform) as GameObject;
        ui.gameObject.name = this.name + "_UI";

        // set ui parameters
        ui.GetComponent<BeingUI>().init(this);

        HideUI();
    }

}