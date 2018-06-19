using System;
using UnityEngine;

[Serializable]
public class TankManager
{
	// Cette classe permet la gestion de plusieurs paramètres d'un tank.
	// Elle fonctionne en collaboration avec le gestionnaire de jeu pour contrôler le comportement des tanks
	// et si les joueurs ont ou n'ont pas le contrôle de leur tank
	// au cours des différentes phases du jeu.

	public Color m_PlayerColor;                             // La couleur que prendra le tank.
	public Transform m_SpawnPoint;                          // La position et la direction du tank lors de son apparition.
	[HideInInspector] public int m_PlayerNumber;            // Spécifie pour à quel joueur appartient le tank.
	[HideInInspector] public string m_ColoredPlayerText;    // Une chaîne de caractères pour représenter le joueur avec le nombre coloré correspondant au tank.
	[HideInInspector] public GameObject m_Instance;         // Une référence à l'instance du tank lorsqu'il est créé.
	[HideInInspector] public int m_Wins;                    // Le nombre de victoires pour ce joueur.


	private TankMovement m_Movement;                        // Référence du script de mouvement du tank, utilisé pour activer et désactiver le contrôle.
	private TankShooting m_Shooting;                        // Référence le script de tir du tank, utilisé pour activer et désactiver le contrôle.
	private GameObject m_CanvasGameObject;                  // Utilisé pour désactiver l'interface utilisateur en espace monde lors des phases de début et de fin de chaque match.


	public void Setup()
	{
		// Récupère la référence des composants.
		m_Movement = m_Instance.GetComponent<TankMovement>();
		m_Shooting = m_Instance.GetComponent<TankShooting>();
		m_CanvasGameObject = m_Instance.GetComponentInChildren<Canvas>().gameObject;

		// Définit le numéro du joueur pour chaque script.
		m_Movement.m_PlayerNumber = m_PlayerNumber;
		m_Shooting.m_PlayerNumber = m_PlayerNumber;

		// Crée une chaîne de caractères en utilisant la couleur adéquate et qui indique 'PLAYER 1', etc. suivant la couleur du tank et le numéro du joueur.
		m_ColoredPlayerText = "<color=#" + ColorUtility.ToHtmlStringRGB(m_PlayerColor) + ">PLAYER " + m_PlayerNumber + "</color>";

		// Récupère tous les composants de rendu du tank.
		MeshRenderer[] renderers = m_Instance.GetComponentsInChildren<MeshRenderer>();

		// Parcours les composants...
		for (int i = 0; i < renderers.Length; i++)
		{
			// ... défini leur couleur à la couleur de ce tank.
			renderers[i].material.color = m_PlayerColor;
		}
	}


	// Utilisé durant les phases du jeu où le joueur ne doit pas être capable de contrôler son tank.
	public void DisableControl()
	{
		m_Movement.enabled = false;
		m_Shooting.enabled = false;

		m_CanvasGameObject.SetActive(false);
	}


	// Utilisé durant les phases du jeu où le joueur doit être capable de contrôler son tank.
	public void EnableControl()
	{
		m_Movement.enabled = true;
		m_Shooting.enabled = true;

		m_CanvasGameObject.SetActive(true);
	}


	// Utilisé au début de chaque match pour remettre le tank à son état d'origine.
	public void Reset()
	{
		m_Instance.transform.position = m_SpawnPoint.position;
		m_Instance.transform.rotation = m_SpawnPoint.rotation;

		m_Instance.SetActive(false);
		m_Instance.SetActive(true);
	}
}