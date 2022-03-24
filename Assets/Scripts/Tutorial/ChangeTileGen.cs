using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeTileGen : MonoBehaviour
{
    public GameManager GM;
    public TileSetGenerator newTileSetGenerator;
    public GameObject newGenerateButton;
    public EnemyAgent tutorialEnemy;
    public TutorialText textController;
    public Player player;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            GM.tileSetGenerator = newTileSetGenerator;
            GM.generateButton = newGenerateButton;
            GM.enemies = new EnemyAgent[1] {tutorialEnemy};
            GM.phase++;

            if (GM.phase == 2)
                player.EnterFightingPhase();

            if (GM.phase == 3)
                player.EnterInteractivPhase();

            if (textController.quoteIndex < 5)
                textController.quoteIndex = 5;

            if(GM.phase < 3)
                textController.showNextQuote();
            
            Destroy(gameObject);
        }
    }
}
