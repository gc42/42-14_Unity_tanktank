using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public int m_NumRoundsToWin = 5;            // Le nombre de matchs qu'un joueur doit gagner pour remporter la partie.
	public float m_StartDelay = 3f;             // Le temps entre le début des phases RoundStarting et RoundPlaying.
	public float m_EndDelay = 3f;               // Le temps entre la fin des phases RoundPlaying et RoundEnding.
	public CameraControl m_CameraControl;       // Référence le script CameraControl pour contrôler les différentes phases du jeu.
	public Text m_MessageText;                  // Référence le texte des résultats.
	public GameObject m_TankPrefab;             // Référence le préfabriqué du tank.
	public TankManager[] m_Tanks;               // Une liste de gestionnaire de tanks pour activer ou désactiver plusieurs paramètres des tanks.


	private int m_RoundNumber;                  // Numéro du match en cours.
	private WaitForSeconds m_StartWait;         // Utilisé pour avoir un délai au démarrage d'un match.
	private WaitForSeconds m_EndWait;           // Utilisé pour avoir un délai à la fin d'un match.
	private TankManager m_RoundWinner;          // Référence le gagnant du match. Utilisé pour annoncer qui a gagné.
	private TankManager m_GameWinner;           // Référence le gagnant de la partie. Utilisé pour annoncer qui a gagné.


	private void Start()
	{
		// Crée les délais d'attente afin que cela ne soit réalisé qu'une seule fois.
		m_StartWait = new WaitForSeconds(m_StartDelay);
		m_EndWait = new WaitForSeconds(m_EndDelay);

		SpawnAllTanks();
		SetCameraTargets();

		// Une fois que les tanks ont été créés et que la caméra les utilise comme cible, démarre le jeu.
		StartCoroutine(GameLoop());
	}


	private void SpawnAllTanks()
	{
		// Pour tous les tanks...
		for (int i = 0; i < m_Tanks.Length; i++)
		{
			// ... les crée, défini le numéro du joueur et les référence pour les manipuler.
			m_Tanks[i].m_Instance =
				Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
			m_Tanks[i].m_PlayerNumber = i + 1;
			m_Tanks[i].Setup();
		}
	}


	private void SetCameraTargets()
	{
		// Créer une liste de transformation de la même taille que le nombre de tanks.
		Transform[] targets = new Transform[m_Tanks.Length];

		// Pour chacune de ces transformations...
		for (int i = 0; i < targets.Length; i++)
		{
			// ... la définit à la transformation du tank.
			targets[i] = m_Tanks[i].m_Instance.transform;
		}

		// Ce sont les cibles que la caméra doit suivre.
		m_CameraControl.m_Targets = targets;
	}


	// La fonction est appelée au début et exécutera chaque phase du jeu, l'une après l'autre.
	private IEnumerator GameLoop()
	{
		// Démarre avec la coroutine 'RoundStarting' mais ne continuera que lorsqu'elle sera finie.
		yield return StartCoroutine(RoundStarting());

		// Une fois que la coroutine 'RoundStarting' est terminée, exécute  la coroutine 'RoundPlaying' mais ne continuera que lorsqu'elle sera finie.
		yield return StartCoroutine(RoundPlaying());

		// Une fois que l'exécution arrive ici, exécute la coroutine 'RoundEnding', mais ne continuera que lorsqu'elle sera finie.
		yield return StartCoroutine(RoundEnding());

		// Ce code ne s'exécutera que lorsque 'RoundEnding' se termine. À ce moment, vérifie si un joueur gagnant a été trouvé.
		if (m_GameWinner != null)
		{
			// S'il y a un joueur gagnant, redémarre le niveau.
			Application.LoadLevel(Application.loadedLevel);
		}
		else
		{
			// S'il n'y a pas encore de joueur gagnant, la boucle continue.
			// Notez que la coroutine ne fait pas de cession. Cela signifie que l'exécution actuelle de la fonction GameLoop va s'arrêter.
			StartCoroutine(GameLoop());
		}
	}


	private IEnumerator RoundStarting()
	{
		// Dès que le match démarre, réinitialise les tanks et s'assure qu'ils ne peuvent se déplacer.
		ResetAllTanks();
		DisableTankControl();

		// Force le zoom et la position de la caméra à quelque chose d'approprié pour les tanks réinitialisés.
		m_CameraControl.SetStartPositionAndSize();

		// Incrémente le nombre de matchs et affiche un texte au joueur pour indiquer quel match est en cours.
		m_RoundNumber++;
		m_MessageText.text = "ROUND " + m_RoundNumber;

		// Attend pour le temps spécifié avant de rendre l'exécution à la boucle de jeu.
		yield return m_StartWait;
	}


	private IEnumerator RoundPlaying()
	{
		// Dès que le match démarre sa phase de jeu, donne aux joueurs le contrôle des tanks.
		EnableTankControl();

		// Nettoie le texte affiché à l'écran.
		m_MessageText.text = string.Empty;

		// Tant qu'il reste plusieurs tanks...
		while (!OneTankLeft())
		{
			// ... passe à la prochaine frame.
			yield return null;
		}
	}


	private IEnumerator RoundEnding()
	{
		// Stoppe le déplacement des tanks.
		DisableTankControl();

		// Enlève le gagnant du précédent match.
		m_RoundWinner = null;

		// Regarde s'il y a un gagnant maintenant que le match est fini.
		m_RoundWinner = GetRoundWinner();

		// S'il y a un gagnant, incrémente son score.
		if (m_RoundWinner != null)
			m_RoundWinner.m_Wins++;

		// Maintenant que le score du gagnant a été incrémenté, regarde si quelqu'un à gagné la partie.
		m_GameWinner = GetGameWinner();

		// Récupère un message dépendant des scores et de l'existence d'un gagnant pour la partie et l'affiche.
		string message = EndMessage();
		m_MessageText.text = message;

		// Attend pour le temps indiqué avant de rendre l'exécution à la boucle de jeu.
		yield return m_EndWait;
	}


	// Utilisé pour vérifier s'il reste au plus un tank dans le match.
	private bool OneTankLeft()
	{
		// Démarre un compteur des tanks restants.
		int numTanksLeft = 0;

		// Parcours tous les tanks...
		for (int i = 0; i < m_Tanks.Length; i++)
		{
			// ... et s'ils sont actifs, incrémente le compteur.
			if (m_Tanks[i].m_Instance.activeSelf)
				numTanksLeft++;
		}

		// S'il y a au plus un tank, retourne true, sinon false.
		return numTanksLeft <= 1;
	}


	// Cette fonction permet de trouver s'il y a un gagnant dans le match.
	// Cette fonction est appelée avec l'hypothèse qu'il reste tout au plus un tank encore actif.
	private TankManager GetRoundWinner()
	{
		// Parcours tous les tanks...
		for (int i = 0; i < m_Tanks.Length; i++)
		{
			// ... et si l'un d'entre eux est actif, c'est le gagnant.
			if (m_Tanks[i].m_Instance.activeSelf)
				return m_Tanks[i];
		}

		// Sinon, aucun tank n'est actif, c'est un match nul.
		return null;
	}


	// Cette fonction permet de trouver s'il y a un gagnant pour la partie.
	private TankManager GetGameWinner()
	{
		// Parcours tous les tanks...
		for (int i = 0; i < m_Tanks.Length; i++)
		{
			// ... et si l'un d'entre eux a gagné assez de matchs, le retourne.
			if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
				return m_Tanks[i];
		}

		// Sinon, retourne null.
		return null;
	}


	// Retourne une chaîne de caractères pour afficher à la fin de chaque match.
	private string EndMessage()
	{
		// Par défaut, lorsqu'un match se termine, il n'y a pas de gagnant, donc le message par défaut est pour un match nul.
		string message = "DRAW!";

		// S'il y a un gagnant, on change le texte pour l'indiquer.
		if (m_RoundWinner != null)
			message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

		// Ajoute quelques sauts de ligne après le message de base.
		message += "\n\n\n\n";

		// Parcours tous les tanks et ajoute leur score au message.
		for (int i = 0; i < m_Tanks.Length; i++)
		{
			message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " WINS\n";
		}

		// S'il y a un gagnant de partie, change complètement le message.
		if (m_GameWinner != null)
			message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";

		return message;
	}


	// Cette fonction est utilisée pour réinitialiser tous les tanks..
	private void ResetAllTanks()
	{
		for (int i = 0; i < m_Tanks.Length; i++)
		{
			m_Tanks[i].Reset();
		}
	}


	private void EnableTankControl()
	{
		for (int i = 0; i < m_Tanks.Length; i++)
		{
			m_Tanks[i].EnableControl();
		}
	}


	private void DisableTankControl()
	{
		for (int i = 0; i < m_Tanks.Length; i++)
		{
			m_Tanks[i].DisableControl();
		}
	}
}