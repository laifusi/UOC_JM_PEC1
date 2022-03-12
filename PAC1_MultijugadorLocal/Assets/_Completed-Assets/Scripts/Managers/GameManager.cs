using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;
using System;

namespace Complete
{
    public class GameManager : MonoBehaviour
    {
        public int m_NumRoundsToWin = 5;            // The number of rounds a single player has to win to win the game.
        public float m_StartDelay = 3f;             // The delay between the start of RoundStarting and RoundPlaying phases.
        public float m_EndDelay = 3f;               // The delay between the end of RoundPlaying and RoundEnding phases.
        public CameraControl m_CameraControl;       // Reference to the CameraControl script for control during different phases.
        public Text m_MessageText;                  // Reference to the overlay Text to display winning text, etc.
        public GameObject m_TankPrefab;             // Reference to the prefab the players will control.
        //private List<TankManager> m_Tanks = new List<TankManager>();
        private Dictionary<int, TankManager> tanks = new Dictionary<int, TankManager>();
        public TankManager[] m_Tanks_Available;     // A collection of managers for enabling and disabling different aspects of the tanks.
        [SerializeField] Camera extraCam;           // Reference to the extra Camera for 3 players.
        //private List<Camera> cameras = new List<Camera>();
        [SerializeField] Camera[] cinemachineCameras;
        [SerializeField] CinemachineVirtualCamera[] virtualCams;
        [SerializeField] PlayerInputManager inputManager;
        [SerializeField] InputSystemUIInputModule[] inputModules;
        [SerializeField] Transform map;


        private int m_RoundNumber;                  // Which round the game is currently on.
        private WaitForSeconds m_StartWait;         // Used to have a delay whilst the round starts.
        private WaitForSeconds m_EndWait;           // Used to have a delay whilst the round or game ends.
        private TankManager m_RoundWinner;          // Reference to the winner of the current round.  Used to make an announcement of who won.
        private TankManager m_GameWinner;           // Reference to the winner of the game.  Used to make an announcement of who won.
        private int numberOfPlayers;
        private int numActiveTanks;


        private void Start()
        {
            // Create the delays so they only have to be made once.
            m_StartWait = new WaitForSeconds (m_StartDelay);
            m_EndWait = new WaitForSeconds (m_EndDelay);

            SpawnAllTanks();
            SetCameraTargets();

            // Once the tanks have been created and the camera is using them as targets, start the game.
            StartCoroutine (GameLoop ());
        }

        private void SpawnAllTanks()
        {
            /*for(int i = 0; i < numberOfPlayers; i++)
            {
                m_Tanks.Add(m_Tanks_Available[i]);
            }*/

            if(PlayerPrefs.GetInt("Player1") == 1)
            {
                tanks.Add(0, m_Tanks_Available[0]);
            }
            if (PlayerPrefs.GetInt("Player2") == 1)
            {
                tanks.Add(1, m_Tanks_Available[1]);
            }
            if (PlayerPrefs.GetInt("Player3") == 1)
            {
                tanks.Add(2, m_Tanks_Available[2]);
            }
            if (PlayerPrefs.GetInt("Player4") == 1)
            {
                tanks.Add(3, m_Tanks_Available[3]);
            }

            numberOfPlayers = tanks.Count;
            numActiveTanks = tanks.Count;

            // For all the tanks...
            foreach (int key in tanks.Keys)
            {
                // ... create them, set their player number and references needed for control.
                InstantiateTank(key);
            }

            SetAllCameraRects();

            Invoke(nameof(PairKeyboard), 1);
        }

        private void PairKeyboard()
        {
            foreach (int key in tanks.Keys)
            {
                InputUser.PerformPairingWithDevice(Keyboard.current, user: tanks[key].m_PlayerInput.user);
            }
        }

        private void InstantiateTank(int i, bool isGamepad = default)
        {
            string scheme;
            InputDevice device;
            if (!isGamepad)
            {
                scheme = "Keyboard" + (i + 1);
                device = Keyboard.current;
            }
            else
            {
                scheme = "Gamepad";
                device = Gamepad.current;
            }
            tanks[i].m_PlayerInput = PlayerInput.Instantiate(m_TankPrefab, controlScheme: scheme, pairWithDevice: device);
            tanks[i].m_Instance = tanks[i].m_PlayerInput.gameObject;
            tanks[i].m_Instance.transform.position = tanks[i].m_SpawnPoint.position;
            tanks[i].m_Instance.transform.rotation = tanks[i].m_SpawnPoint.rotation;
            tanks[i].m_PlayerInput.defaultControlScheme = scheme;
            tanks[i].m_PlayerInput.uiInputModule = inputModules[i];
            tanks[i].m_PlayerNumber = i + 1;
            tanks[i].Setup();
            AddCamera(i);
        }

        private void AddCamera(int playerNumber)
        {            
            cinemachineCameras[playerNumber].gameObject.SetActive(true);

            virtualCams[playerNumber].LookAt = tanks[playerNumber].m_Instance.transform;
            virtualCams[playerNumber].Follow = tanks[playerNumber].m_Instance.transform;
        }
        
        private void SetAllCameraRects()
        {
            int i = 0;
            foreach(int key in tanks.Keys)
            {
                if (numberOfPlayers > 2)
                {
                    if (i == 0)
                    {
                        cinemachineCameras[key].rect = new Rect(-0.5f, 0.5f, 1.0f, 1.0f);
                    }
                    else if(i == 1)
                    {
                        cinemachineCameras[key].rect = new Rect(0.5f, 0.5f, 1.0f, 1.0f);
                    }
                    else if(i == 2)
                    {
                        cinemachineCameras[key].rect = new Rect(-0.5f, -0.5f, 1.0f, 1.0f);
                    }
                    else
                    {
                        cinemachineCameras[key].rect = new Rect(0.5f, -0.5f, 1.0f, 1.0f);
                    }
                }
                else
                {
                    if (i == 0)
                    {
                        cinemachineCameras[key].rect = new Rect(-0.5f, 0.0f, 1.0f, 1.0f);
                    }
                    else
                    {
                        cinemachineCameras[key].rect = new Rect(0.5f, 0.0f, 1.0f, 1.0f);
                    }
                }
                i++;
            }
        }

        private void SetCameraTargets()
        {
            // Create a collection of transforms the same size as the number of tanks.
            Transform[] targets = new Transform[tanks.Count];

            int i = 0;
            // For each of these transforms...
            foreach(int key in tanks.Keys)
            {
                // ... set it to the appropriate tank transform.
                targets[i] = tanks[key].m_Instance.transform;
                i++;
            }

            // These are the targets the camera should follow.
            m_CameraControl.m_Targets = targets;
        }

        // This is called from start and will run each phase of the game one after another.
        private IEnumerator GameLoop ()
        {
            // Start off by running the 'RoundStarting' coroutine but don't return until it's finished.
            yield return StartCoroutine (RoundStarting ());

            // Once the 'RoundStarting' coroutine is finished, run the 'RoundPlaying' coroutine but don't return until it's finished.
            yield return StartCoroutine (RoundPlaying());

            // Once execution has returned here, run the 'RoundEnding' coroutine, again don't return until it's finished.
            yield return StartCoroutine (RoundEnding());

            // This code is not run until 'RoundEnding' has finished.  At which point, check if a game winner has been found.
            if (m_GameWinner != null)
            {
                // If there is a game winner, restart the level.
                SceneManager.LoadScene (0);
            }
            else
            {
                // If there isn't a winner yet, restart this coroutine so the loop continues.
                // Note that this coroutine doesn't yield.  This means that the current version of the GameLoop will end.
                StartCoroutine (GameLoop ());
            }
        }


        private IEnumerator RoundStarting ()
        {
            // As soon as the round starts reset the tanks and make sure they can't move.
            ResetAllTanks ();
            DisableTankControl ();

            // Snap the camera's zoom and position to something appropriate for the reset tanks.
            m_CameraControl.SetStartPositionAndSize ();

            // Increment the round number and display text showing the players what round it is.
            m_RoundNumber++;
            m_MessageText.text = "ROUND " + m_RoundNumber;

            // Wait for the specified length of time until yielding control back to the game loop.
            yield return m_StartWait;
        }


        private IEnumerator RoundPlaying ()
        {
            // As soon as the round begins playing let the players control the tanks.
            EnableTankControl ();

            // Clear the text from the screen.
            m_MessageText.text = string.Empty;

            // While there is not one tank left...
            while (!OneTankLeft())
            {
                // ... return on the next frame.
                yield return null;
            }
        }


        private IEnumerator RoundEnding ()
        {
            // Stop tanks from moving.
            DisableTankControl ();

            // Clear the winner from the previous round.
            m_RoundWinner = null;

            // See if there is a winner now the round is over.
            m_RoundWinner = GetRoundWinner ();

            // If there is a winner, increment their score.
            if (m_RoundWinner != null)
                m_RoundWinner.m_Wins++;

            // Now the winner's score has been incremented, see if someone has one the game.
            m_GameWinner = GetGameWinner ();

            // Get a message based on the scores and whether or not there is a game winner and display it.
            string message = EndMessage ();
            m_MessageText.text = message;

            // Wait for the specified length of time until yielding control back to the game loop.
            yield return m_EndWait;
        }


        // This is used to check if there is one or fewer tanks remaining and thus the round should end.
        private bool OneTankLeft()
        {
            // Start the count of tanks left at zero.
            int numTanksLeft = 0;

            // Go through all the tanks...
            foreach (int key in tanks.Keys)
            {
                // ... and if they are active, increment the counter.
                if (tanks[key].m_Instance.activeSelf)
                    numTanksLeft++;
            }

            if(numActiveTanks > numTanksLeft)
            {
                numActiveTanks = numTanksLeft;
                //SetAllCameraRects();
            }

            // If there are one or fewer tanks remaining return true, otherwise return false.
            return numTanksLeft <= 1;
        }
        
        
        // This function is to find out if there is a winner of the round.
        // This function is called with the assumption that 1 or fewer tanks are currently active.
        private TankManager GetRoundWinner()
        {
            // Go through all the tanks...
            foreach (int key in tanks.Keys)
            {
                // ... and if one of them is active, it is the winner so return it.
                if (tanks[key].m_Instance.activeSelf)
                    return tanks[key];
            }

            // If none of the tanks are active it is a draw so return null.
            return null;
        }


        // This function is to find out if there is a winner of the game.
        private TankManager GetGameWinner()
        {
            // Go through all the tanks...
            foreach (int key in tanks.Keys)
            {
                // ... and if one of them has enough rounds to win the game, return it.
                if (tanks[key].m_Wins == m_NumRoundsToWin)
                    return tanks[key];
            }

            // If no tanks have enough rounds to win, return null.
            return null;
        }


        // Returns a string message to display at the end of each round.
        private string EndMessage()
        {
            // By default when a round ends there are no winners so the default end message is a draw.
            string message = "DRAW!";

            // If there is a winner then change the message to reflect that.
            if (m_RoundWinner != null)
                message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

            // Add some line breaks after the initial message.
            message += "\n\n\n\n";

            // Go through all the tanks and add each of their scores to the message.
            foreach (int key in tanks.Keys)
            {
                message += tanks[key].m_ColoredPlayerText + ": " + tanks[key].m_Wins + " WINS\n";
            }

            // If there is a game winner, change the entire message to reflect that.
            if (m_GameWinner != null)
                message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";

            return message;
        }


        // This function is used to turn all the tanks back on and reset their positions and properties.
        private void ResetAllTanks()
        {
            foreach (int key in tanks.Keys)
            {
                tanks[key].Reset();
            }
        }


        private void EnableTankControl()
        {
            foreach (int key in tanks.Keys)
            {
                tanks[key].EnableControl();
            }
        }


        private void DisableTankControl()
        {
            foreach (int key in tanks.Keys)
            {
                tanks[key].DisableControl();
            }
        }

        public void InstantiateNewPlayer(int playerNumber)
        {
            if(tanks.ContainsKey(playerNumber))
            {
                return;
            }

            numberOfPlayers++;
            numActiveTanks++;
            tanks.Add(playerNumber, m_Tanks_Available[playerNumber]);
            InstantiateTank(playerNumber);
            SetAllCameraRects();
        }

        public void InstantiateNewUndefinedPlayer()
        {
            if (numberOfPlayers >= 4)
            {
                return;
            }
            for(int i = 0; i < 4; i++)
            {
                if(!tanks.ContainsKey(i))
                {
                    numberOfPlayers++;
                    numActiveTanks++;
                    tanks.Add(i, m_Tanks_Available[i]);
                    InstantiateTank(i, true);
                    SetAllCameraRects();
                    InputUser.PerformPairingWithDevice(Gamepad.current, user: tanks[i].m_PlayerInput.user);
                    return;
                }
            }
        }
    }
}