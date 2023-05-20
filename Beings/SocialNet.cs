using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SocialNet : MonoBehaviour {
    
    private Dictionary<Being, Dictionary<Being,float>> love;
    private Dictionary<Being, float> reputation;

    private Being max_reputation_being;

    // unity functions

    public void Start()
    {
        love = new Dictionary<Being, Dictionary<Being,float>>();
        reputation = new Dictionary<Being, float>();

        // test
        Debug.Log("social net start");
    }

    // social net functions

    public void simulate(List<Being> beings){

        int nb_rounds = 1;
        for (int i=0; i<nb_rounds; i++)
        {
            // simulate the social net of the beings
            // get called each tick of the game

            // choose 2 random beings
            Being giver = beings[Random.Range(0,beings.Count)];
            Being receiver = beings[Random.Range(0,beings.Count)];

            // init interaction with the other being
            InitInteraction(giver, receiver);


            // calculate the gift based on the relationship
            float gift = love[giver][receiver] * Random.Range(0.01f, 0.1f);

            if (Mathf.Abs(gift) < 0.01f)
            {
                gift = Random.Range(-1f,1f);
            }

            // if the beings are not the same
            if (giver != receiver)
            {
                OneWayInteraction(giver, receiver, gift);
            }
        }

        // major reputation shock
        /* if (Random.Range(0, 5000) < 1)
        {
            // choose a random being
            Being giver = beings[Random.Range(0, beings.Count)];
            
            // choose a big gift
            float gift = 1000f * Mathf.Sign(Random.Range(-1f,1f));
            Debug.Log("WHA : " + giver.GetName() + " " + gift);
            
            beings.Remove(giver);
            BrodcastInteraction(giver,beings,gift);
        } */

    }

    // interactions

    public void AddBeing(Being being){
        if (!love.ContainsKey(being))
        {
            love.Add(being, new Dictionary<Being, float>());
        }
    }

    public void InitInteraction(Being receiver, Being giver){
        AddBeing(receiver);
        if (!love[receiver].ContainsKey(giver))
        {
            love[receiver].Add(giver, 0);
        }

        AddBeing(giver);
        if (!love[giver].ContainsKey(receiver))
        {
            love[giver].Add(receiver, 0);
        }
    }

    public void OneWayInteraction(Being receiver, Being giver, float gift){

        // GIVER interact with RECEVER. Only RECEVER is affected by the interaction
        // ex : giver give something to receiver -> receiver received +1 love regarding giver

        // ex2: giver fait un coup batard à receiver -> receiver received -1 love regarding giver

        // add the being if they are not in the social net
        InitInteraction(receiver, giver);

        // add the relationship
        love[receiver][giver] += gift;

        // normalize the relationship
        love[receiver][giver] = NormalizeComparingMaxReputation(love[receiver][giver]);

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

    public void BrodcastInteraction(Being giver, List<Being> receivers ,float interactionForce){
            
            // GIVER interact with all RECEVERS. All RECEVERS are affected by the interaction
            // ex : giver give something to all receivers -> all receivers received +1 love regarding giver
    
            // ex2: giver fait un coup batard à all receivers -> all receivers received -1 love regarding giver
    
            // add the being if they are not in the social net
            foreach (Being receiver in receivers)
            {
                InitInteraction(receiver, giver);
            }
    
            // add the relationship
            foreach (Being receiver in receivers)
            {
                love[receiver][giver] += interactionForce;
                love[receiver][giver] = NormalizeComparingMaxReputation(love[receiver][giver]);
            }
    
            // calculate reputation
            CalculateReputation(giver);
    }

    // reputation

    public void CalculateReputation(Being being){

        if (!reputation.ContainsKey(being))
        {
            if (!love.ContainsKey(being)){
                return;
            }
            reputation.Add(being, 0);
        }

        reputation[being] = 0;
        float totalReputation = 0;
        int count = 0;

        // iterate through the all social net to find who love the being
        // todo : lourd, on devrait mieux stocker qui a déjà interagit avec qui
        foreach (KeyValuePair<Being, Dictionary<Being,float>> entry in love)
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
            if (max_reputation_being == null || reputation[being] > reputation[max_reputation_being])
            {
                max_reputation_being = being;
            }
        }
    }

    // usefull functions

    private float NormalizeComparingMaxReputation(float love){
        // Normalize the love between -100 and 100
        // based on the max reputation being

        if (max_reputation_being == null)
        {
            return love;
        }

        float max_reputation = GetReputation(max_reputation_being);

        bool negative = (love < 0);
        love = (Mathf.Abs(love)/max_reputation) * 100;

        return love*(negative ? -1 : 1);
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