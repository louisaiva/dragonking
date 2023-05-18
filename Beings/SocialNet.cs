using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SocialNet : MonoBehaviour {
    
    private Dictionary<Being, Dictionary<Being,float>> relationships;
    private Dictionary<Being, float> reputation;

    // unity functions

    public void Start()
    {
        relationships = new Dictionary<Being, Dictionary<Being,float>>();
        reputation = new Dictionary<Being, float>();

        // test
        Debug.Log("social net start");
    }

    // social net functions

    public void simulate(List<Being> beings){

        // simulate the social net of the beings
        // get called each tick of the game

        // choose 2 random beings
        Being giver = beings[Random.Range(0,beings.Count)];
        Being recever = beings[Random.Range(0,beings.Count)];

        // init interaction with the other being
        InitInteraction(giver, recever);


        // calculate the gift based on the relationship
        float gift = relationships[giver][recever] * Random.Range(0.01f, 0.1f);

        if (Mathf.Abs(gift) < 0.01f)
        {
            gift = Random.Range(-1f,1f);
        }

        // if the beings are not the same
        if (giver != recever)
        {
            OneWayInteraction(giver, recever, gift);
        }

    }

    // interactions

    public void AddBeing(Being being){
        if (!relationships.ContainsKey(being))
        {
            relationships.Add(being, new Dictionary<Being, float>());
        }
    }

    public void InitInteraction(Being recever, Being giver){
        AddBeing(recever);
        if (!relationships[recever].ContainsKey(giver))
        {
            relationships[recever].Add(giver, 0);
        }

        AddBeing(giver);
        if (!relationships[giver].ContainsKey(recever))
        {
            relationships[giver].Add(recever, 0);
        }
    }

    public void OneWayInteraction(Being recever, Being giver, float gift){

        // GIVER interact with RECEVER. Only RECEVER is affected by the interaction
        // ex : giver give something to recever -> recever received +1 love regarding giver

        // ex2: giver fait un coup batard à recever -> recever received -1 love regarding giver

        // add the being if they are not in the social net
        InitInteraction(recever, giver);

        // add the relationship
        relationships[recever][giver] += gift;

        // calculate reputation
        CalculateReputation(giver);
    }

    public void SymetricInteraction(Being being1, Being being2, float interactionForce){

        // each being interact with the other. Both are affected by the interaction
        // WITH THE SAME FORCE
        // ex : being1 and being2 are friends -> being1 received +1 love regarding being2 AND being2 received +1 love regarding being1

        // 2 one way interactions
        OneWayInteraction(being1, being2, interactionForce);
        OneWayInteraction(being2, being1, interactionForce);
    }

    // reputation

    public void CalculateReputation(Being being){

        if (!reputation.ContainsKey(being))
        {
            if (!relationships.ContainsKey(being)){
                return;
            }
            reputation.Add(being, 0);
        }

        reputation[being] = 0;
        float totalReputation = 0;
        int count = 0;

        // iterate through the all social net to find who love the being
        // todo : lourd, on devrait mieux stocker qui a déjà interagit avec qui
        foreach (KeyValuePair<Being, Dictionary<Being,float>> entry in relationships)
        {
            if (entry.Value.ContainsKey(being))
            {
                totalReputation += entry.Value[being];
                count++;
            }
        }


        // average

        if (count > 0)
        {
            reputation[being] = totalReputation / count;
        }
    }


    // getters

    public float GetReputation(Being being){
        if (!reputation.ContainsKey(being))
        {
            return 0;
        }
        return reputation[being];
    }

}