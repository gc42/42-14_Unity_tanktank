using UnityEngine;

public class TankMovement : MonoBehaviour
{
	public int m_PlayerNumber = 1;              // Identifie à quel joueur le tank appartient. Cela est défini par le gestionnaire de tanks.
	public float m_Speed = 12f;                 // Vitesse de déplacement avant/arrière du tank.
	public float m_TurnSpeed = 180f;            // Vitesse de rotation du tank en degrés par seconde.
	public AudioSource m_MovementAudio;         // Référence vers la source audio pour jouer le son du moteur. Note : source différente de celle pour jouer le tir.
	public AudioClip m_EngineIdling;            // Son à jouer lorsque le tank ne se déplace pas.
	public AudioClip m_EngineDriving;           // Son à jouer lorsque le tank se déplace.
	public float m_PitchRange = 0.2f;           // Variation du pitch des bruits du moteur.


	private string m_MovementAxisName;          // Nom des axes utilisés pour le mouvement avant/arrière.
	private string m_TurnAxisName;              // Nom des axes utilisés pour la rotation.
	private Rigidbody m_Rigidbody;              // Référence utilisée pour déplacer le tank.
	private float m_MovementInputValue;         // Valeur actuelle du déplacement.
	private float m_TurnInputValue;             // Valeur actuelle de rotation.
	private float m_OriginalPitch;              // Le pitch de la source audio au début de la scène.


	private void Awake()
	{
		m_Rigidbody = GetComponent<Rigidbody>();
	}


	private void OnEnable()
	{
		// Lorsque le tank est activé, assurons-nous qu'il n'est pas cinématique.
		m_Rigidbody.isKinematic = false;

		// Réinitialise les valeurs d'entrées.
		m_MovementInputValue = 0f;
		m_TurnInputValue = 0f;
	}


	private void OnDisable()
	{
		// Lorsque le tank est désactivé, réactivons le mouvement cinématique pour stopper son mouvement.
		m_Rigidbody.isKinematic = true;
	}


	private void Start()
	{
		// Les noms des axes dépendent du numéro du joueur.
		m_MovementAxisName = "Vertical" + m_PlayerNumber;
		m_TurnAxisName = "Horizontal" + m_PlayerNumber;

		// Stocke le pitch d'origine de la source audio.
		m_OriginalPitch = m_MovementAudio.pitch;
	}


	private void Update()
	{
		// Stocke la valeur des deux axes d'entrées.
		m_MovementInputValue = Input.GetAxis(m_MovementAxisName);
		m_TurnInputValue = Input.GetAxis(m_TurnAxisName);

		EngineAudio();
	}


	private void EngineAudio()
	{
		// S'il n'y a pas d'entrée (le tank est immobile)...
		if (Mathf.Abs(m_MovementInputValue) < 0.1f && Mathf.Abs(m_TurnInputValue) < 0.1f)
		{
			// ... et si la source audio joue actuellement le son lié au mouvement...
			if (m_MovementAudio.clip == m_EngineDriving)
			{
				// ... change le son pour jouer le son d'attente.
				m_MovementAudio.clip = m_EngineIdling;
				m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
				m_MovementAudio.Play();
			}
		}
		else
		{
			// Sinon, si le tank se déplace et que la source audio joue le son d'attente...
			if (m_MovementAudio.clip == m_EngineIdling)
			{
				// ... change le son pour jouer le son lié au mouvement.
				m_MovementAudio.clip = m_EngineDriving;
				m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
				m_MovementAudio.Play();
			}
		}
	}


	private void FixedUpdate()
	{
		// Ajuste la position et rotation du RigidBody.
		Move();
		Turn();
	}


	private void Move()
	{
		// Crée un vecteur dans la direction du tank. Le tank se déplace avec une vitesse basée sur l'entrée, la vitesse et le temps entre chaque frame.
		Vector3 movement = transform.forward * m_MovementInputValue * m_Speed * Time.deltaTime;

		// Applique ce mouvement à la position du RigidBody.
		m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
	}


	private void Turn()
	{
		// Détermine le nombre de degrés de rotation suivant l'entrée, la vitesse et le temps entre chaque frame.
		float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;

		// Transforme l'angle en une rotation sur l'axe Y.
		Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);

		// Applique la rotation au RigidBody.
		m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);
	}
}