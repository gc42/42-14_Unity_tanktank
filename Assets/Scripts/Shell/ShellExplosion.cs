using UnityEngine;

public class ShellExplosion : MonoBehaviour
{
	public LayerMask m_TankMask;                        // Utilisé pour filtrer ce que l'explosion impacte, cela devrait être défini à « Players ».
	public ParticleSystem m_ExplosionParticles;         // Référence le système de particules joué lors de l'explosion.
	public AudioSource m_ExplosionAudio;                // Référence le son joué lors de l'explosion.
	public float m_MaxDamage = 100f;                    // Les dégâts infligés lorsque l'explosion est sur le tank.
	public float m_ExplosionForce = 1000f;              // La force de l'onde de choc à appliquer sur le tank.
	public float m_MaxLifeTime = 2f;                    // Le temps en secondes avant que le missile soit retiré.
	public float m_ExplosionRadius = 5f;                // La distance maximale d'effet de l'onde de choc.


	private void Start()
	{
		// Si l'objet n'est pas détruit entretemps, détruit le missile après sa durée de vie.
		Destroy(gameObject, m_MaxLifeTime);
	}


	private void OnTriggerEnter(Collider other)
	{
		// Récupère tous les objets ayant une collision dans la sphère entourant le missile.
		Collider[] colliders = Physics.OverlapSphere(transform.position, m_ExplosionRadius, m_TankMask);

		// Parcourt tous les objets...
		for (int i = 0; i < colliders.Length; i++)
		{
			// ... et récupère le « RigidBody ».
			Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody>();

			// S'il n'y a pas de « RigidBody », passe à l'objet suivant.
			if (!targetRigidbody)
				continue;

			// Ajoute la force de l'explosion.
			targetRigidbody.AddExplosionForce(m_ExplosionForce, transform.position, m_ExplosionRadius);

			// Trouve le script de gestion de vie du tank associé à ce « RigidBody ».
			TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth>();

			// S'il n'y a pas de script à cet objet, passe au suivant.
			if (!targetHealth)
				continue;

			// Calcule les dégâts infligés suivant la distance entre l'objet et le missile.
			float damage = CalculateDamage(targetRigidbody.position);

			// Répercute ces dégâts sur le tank.
			targetHealth.TakeDamage(damage);
		}

		// Détache le système de particules du missile.
		m_ExplosionParticles.transform.parent = null;

		// Joue le système de particules.
		m_ExplosionParticles.Play();

		// Joue le son d'explosion.
		m_ExplosionAudio.Play();

		// Une fois le système de particules terminé, détruit l'objet associé.
		Destroy(m_ExplosionParticles.gameObject, m_ExplosionParticles.duration);

		// Détruit le missile.
		Destroy(gameObject);
	}


	private float CalculateDamage(Vector3 targetPosition)
	{
		// Crée un vecteur entre le missile et la cible.
		Vector3 explosionToTarget = targetPosition - transform.position;

		// Calcule la distance entre le missile et la cible.
		float explosionDistance = explosionToTarget.magnitude;

		// Calcule la proportion par rapport à la distance maximale et la distance actuelle.
		float relativeDistance = (m_ExplosionRadius - explosionDistance) / m_ExplosionRadius;

		// Calcule les dégâts infligés suivant la proportion.
		float damage = relativeDistance * m_MaxDamage;

		// S'assure que le minimum de dégâts infligés est toujours 0.
		damage = Mathf.Max(0f, damage);

		return damage;
	}
}