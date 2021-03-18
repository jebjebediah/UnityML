using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public int numCoins;
    public CollectorAgentTrain Player1;
    public CollectorAgentTrain Player2;

    void Start()
    {
        numCoins = 8;
    }

    public void checkIfOver() {
        numCoins--;

        // If all coins are collected, end episodes for both agents
        if (numCoins <= 0) {
            numCoins = 8;
            if (Player1 != null) {
                Player1.EndEpisode();
            }
            if (Player2 != null) {
                Player2.EndEpisode();
            }
        }
    }
}
