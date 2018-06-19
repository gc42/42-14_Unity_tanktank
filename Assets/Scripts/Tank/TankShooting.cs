using UnityEngine;
using UnityEngine.UI;

public class TankShooting : MonoBehaviour
{
	public int m_PlayerNumber = 1;              // Utilisé pour identifier les joueurs.
	public Rigidbody m_Shell;                   // Préfabriqué du missile.
	public Transform m_FireTransform;           // Un enfant du tank où les missiles sont créés.
	public Slider m_AimSlider;                  // Un enfant du tank qui affiche la force du tir.
	public AudioSource m_ShootingAudio;         // Référence vers la source audio jouée lors du tir. Notez que la source audio est différente de celle du mouvement.
	public AudioClip m_ChargingClip;            // Audio joué à chaque chargement du tir.
	public AudioClip m_FireClip;                // Audio joué lorsque le missile est tiré.
	public float m_MinLaunchForce = 15f;        // La force donnée au missile si le bouton n'est pas maintenu.
	public float m_MaxLaunchForce = 30f;        // La force donnée au missile si le bouton est maintenu jusqu'au maximum du temps de charge.
	public float m_MaxChargeTime = 0.75f;       // Combien de temps le missile peut se charger avant d'être à la force maximale.


	private string m_FireButton;                // Le bouton utilisé pour tirer les missiles.
	private float m_CurrentLaunchForce;         // La force qui sera donnée lorsque le bouton sera relâché.
	private float m_ChargeSpeed;                // La vitesse du tir augmente suivant la valeur maximale du temps de chargement.
	private bool m_Fired;                       // Indicateur si le missile a été tiré ou non.


	private void OnEnable()
	{
		// Lorsque le tank est activé, réinitialise la force de lancement et l'interface utilisateur.
		m_CurrentLaunchForce = m_MinLaunchForce;
		m_AimSlider.value = m_MinLaunchForce;
	}


	private void Start()
	{
		// Le bouton de tir dépend du nombre de joueurs.
		m_FireButton = "Fire" + m_PlayerNumber;

		// La vitesse de chargement de la force du tir est ratio du champ de force par rapport au temps de charge.
		m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;
	}


	private void Update()
	{
		// Le curseur de défilement devrait avoir une valeur par défaut équivalente à la force minimale.
		m_AimSlider.value = m_MinLaunchForce;

		// Si la force maximale a été dépassée et que le missile n'a pas encore été tiré...
		if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
		{
			// ... utilise la force maximale et tire le missile.
			m_CurrentLaunchForce = m_MaxLaunchForce;
			Fire();
		}
		// Sinon, si le bouton de feu vient juste d'être appuyé...
		else if (Input.GetButtonDown(m_FireButton))
		{
			// ... réinitialise l'indicateur de tir et réinitialise la force du tir.
			m_Fired = false;
			m_CurrentLaunchForce = m_MinLaunchForce;

			// Change le son pour utiliser le son de chargement.
			m_ShootingAudio.clip = m_ChargingClip;
			m_ShootingAudio.Play();
		}
		// Sinon, si le bouton de feu est maintenu et que le missile n'est pas encore tiré...
		else if (Input.GetButton(m_FireButton) && !m_Fired)
		{
			// Incrémente la force de lancement et met à jour le curseur de défilement.
			m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;

			m_AimSlider.value = m_CurrentLaunchForce;
		}
		// Sinon, si le bouton de feu est relâché et que le missile n'a pas encore été tiré...
		else if (Input.GetButtonUp(m_FireButton) && !m_Fired)
		{
			// ... tire le missile.
			Fire();
		}
	}


	private void Fire()
	{
		// Défini l'indicateur de tir afin de n'appeler la fonction qu'une seule fois.
		m_Fired = true;

		// Crée une instance du missile et stocke la référence dans son rigidbody.
		Rigidbody shellInstance =
			Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;

		// Défini la vélocité du missile à la force de tir dans la direction du tir.
		shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward; ;

		// Change le son pour utiliser le son du tir.
		m_ShootingAudio.clip = m_FireClip;
		m_ShootingAudio.Play();

		// Réinitialise la force de lancement. Ce n'est qu'une précaution.
		m_CurrentLaunchForce = m_MinLaunchForce;
	}
}