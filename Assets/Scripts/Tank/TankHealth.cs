using UnityEngine;
using UnityEngine.UI;

public class TankHealth : MonoBehaviour
{
	public float m_StartingHealth = 100f;               // La valeur de départ avec laquelle un tank commence.
	public Slider m_Slider;                             // La référence vers le curseur de défilement affichant la santé du tank.
	public Image m_FillImage;                           // L'image composant le curseur de défilement.
	public Color m_FullHealthColor = Color.green;       // La couleur du curseur de défilement lorsque le tank a toute sa vie.
	public Color m_ZeroHealthColor = Color.red;         // La couleur de la santé lorsque le tank n'a presque plus de vie.
	public GameObject m_ExplosionPrefab;                // Un préfabriqué qui sera instancié dans la fonction Awake(), puis utilisé lors de la mort du tank.


	private AudioSource m_ExplosionAudio;               // La source audio du son d'explosion.
	private ParticleSystem m_ExplosionParticles;        // Le système de particule de la destruction du tank.
	private float m_CurrentHealth;                      // La santé actuelle du tank.
	private bool m_Dead;                                // Est-ce que le tank n'a plus de vie ?


	private void Awake()
	{
		// Instancie le préfabriqué de l'explosion et récupère une référence sur le système de particule de celui-ci.
		m_ExplosionParticles = Instantiate(m_ExplosionPrefab).GetComponent<ParticleSystem>();

		// Récupère une référence sur la source audio du préfabriqué.
		m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource>();

		// Désactive le préfabriqué afin de ne l'activer que lorsque nécessaire.
		m_ExplosionParticles.gameObject.SetActive(false);
	}


	private void OnEnable()
	{
		// Lorsque le tank est activé, réinitialise la vie du tank et enlève l'état de mort.
		m_CurrentHealth = m_StartingHealth;
		m_Dead = false;

		// Met à jour la barre de vie et sa couleur.
		SetHealthUI();
	}


	public void TakeDamage(float amount)
	{
		// Réduit la vie du tank.
		m_CurrentHealth -= amount;

		// Met à jour la barre de vie.
		SetHealthUI();

		// Si la vie restante est à zéro ou inférieur et que le tank n'est pas encore mort, appelle OnDeath().
		if (m_CurrentHealth <= 0f && !m_Dead)
		{
			OnDeath();
		}
	}


	private void SetHealthUI()
	{
		// Définit la valeur du curseur de défilement.
		m_Slider.value = m_CurrentHealth;

		// Effectue une interpolation de la couleur de la barre de vie entre les couleurs vert et rouge suivant la vie restante.
		m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth);
	}


	private void OnDeath()
	{
		// Définit l'indicateur de mort afin que cette fonction ne soit appelée qu'une seule fois.
		m_Dead = true;

		// Déplace l'explosion instanciée à la position du tank et l'active.
		m_ExplosionParticles.transform.position = transform.position;
		m_ExplosionParticles.gameObject.SetActive(true);

		// Joue l'animation du système de particules.
		m_ExplosionParticles.Play();

		// Joue le son d'explosion.
		m_ExplosionAudio.Play();

		// Enlève le tank.
		gameObject.SetActive(false);
	}
}