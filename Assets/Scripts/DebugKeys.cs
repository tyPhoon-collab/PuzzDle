using ItemsEnum;
using AccsEnum;
using UnityEngine;

public class DebugKeys : MonoBehaviour
{
    [SerializeField] ty_Hero tyHero;
    [SerializeField] Puzzle puzzle;
    [SerializeField] ty_Item item;
    [SerializeField] GameDirector game;

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            puzzle.AdditionalMoveableTimes += 100;
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            game.MaxFloor = 10000;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            game.Save();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            tyHero.Money += 10000;
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            tyHero.Crystal += 10000;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            tyHero.Exp += 5000;
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            for (int n = 0; n < (int)Accs.MAX; n++)
            {
                tyHero.SetAccAcquire((Accs)n);
            }
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            for (int i = 0; i < 100; i++)
            {
                Items _item = (Items)Random.Range(0, (int)Items.MAX);
                item.AddItem(_item);
            }
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            game.PlayTimeSpan += new System.TimeSpan(0, 5, 0);
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log(PlayerPrefs.GetString("HeroData"));
            Debug.Log(PlayerPrefs.GetString("FloorData"));
            Debug.Log(PlayerPrefs.GetString("PuzzleData"));
            Debug.Log(PlayerPrefs.GetString("PlayTimeData"));
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            PlayerPrefs.DeleteAll();
        }
    }
}
