using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectionManager : MonoBehaviour
{
    private List<int> playersChosen = new List<int>();
    private bool enoughPlayersChosen;
    [SerializeField] private GameObject playButton;

    public void ChoosePlayer(int playerNumber)
    {
        Image clickedImage = EventSystem.current.currentSelectedGameObject.GetComponent<Image>();
        if(playersChosen.Contains(playerNumber))
        {
            playersChosen.Remove(playerNumber);
            clickedImage.color = new Color(clickedImage.color.r, clickedImage.color.g, clickedImage.color.b, 0.5f);
        }
        else
        {
            playersChosen.Add(playerNumber);
            clickedImage.color = new Color(clickedImage.color.r, clickedImage.color.g, clickedImage.color.b, 0f);
        }

        CheckNumberOfPlayers();
    }

    private void CheckNumberOfPlayers()
    {
        enoughPlayersChosen = playersChosen.Count >= 2;
        playButton.SetActive(enoughPlayersChosen);
    }

    public void Play()
    {
        PlayerPrefs.SetInt("Player1", playersChosen.Contains(1) ? 1 : 0);
        PlayerPrefs.SetInt("Player2", playersChosen.Contains(2) ? 1 : 0);
        PlayerPrefs.SetInt("Player3", playersChosen.Contains(3) ? 1 : 0);
        PlayerPrefs.SetInt("Player4", playersChosen.Contains(4) ? 1 : 0);
        SceneManager.LoadScene(1);
    }
}
