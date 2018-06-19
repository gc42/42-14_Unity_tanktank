using UnityEngine;

public class CameraControl : MonoBehaviour
{
	public float m_DampTime = 0.2f;                 // Temps approximatif pris par la caméra pour se focaliser.
	public float m_ScreenEdgeBuffer = 4f;           // Espace entre la cible en haut/bas de l'écran et la bordure de celui-ci.
	public float m_MinSize = 6.5f;                  // La taille minimale que la caméra puisse avoir.
	[HideInInspector] public Transform[] m_Targets; // Toutes les cibles que la caméra doit suivre.


	private Camera m_Camera;                        // Utilisé pour référencer la caméra.
	private float m_ZoomSpeed;                      // Vitesse de référence pour l'amortissement de la taille.
	private Vector3 m_MoveVelocity;                 // Vélocité de référence pour l'amortissement de la position.
	private Vector3 m_DesiredPosition;              // La position vers laquelle la caméra se dirige.


	private void Awake()
	{
		m_Camera = GetComponentInChildren<Camera>();
	}


	private void FixedUpdate()
	{
		// Déplace la caméra à la position voulue.
		Move();

		// Change la taille de la caméra.
		Zoom();
	}


	private void Move()
	{
		// Trouve la position moyenne des cibles.
		FindAveragePosition();

		// Se déplace doucement vers cette position.
		transform.position = Vector3.SmoothDamp(transform.position, m_DesiredPosition, ref m_MoveVelocity, m_DampTime);
	}


	private void FindAveragePosition()
	{
		Vector3 averagePos = new Vector3();
		int numTargets = 0;

		// Parcourt toutes les cibles et somme leur position.
		for (int i = 0; i < m_Targets.Length; i++)
		{
			// Si la cible n'est pas active, passe à la suivante.
			if (!m_Targets[i].gameObject.activeSelf)
				continue;

			// Ajoute à la moyenne et incrémente le nombre de cibles prises en compte.
			averagePos += m_Targets[i].position;
			numTargets++;
		}

		// S'il y a des cibles, divise la somme par leur nombre pour trouver la moyenne.
		if (numTargets > 0)
			averagePos /= numTargets;

		// Garde la même valeur pour Y.
		averagePos.y = transform.position.y;

		// La position voulue est la position moyenne.
		m_DesiredPosition = averagePos;
	}


	private void Zoom()
	{
		// Trouve la taille requise suivant la position et atteint doucement cette taille.
		float requiredSize = FindRequiredSize();
		m_Camera.orthographicSize = Mathf.SmoothDamp(m_Camera.orthographicSize, requiredSize, ref m_ZoomSpeed, m_DampTime);
	}


	private float FindRequiredSize()
	{
		// Trouve la position vers laquelle le CameraRig se déplace dans son espace local.
		Vector3 desiredLocalPos = transform.InverseTransformPoint(m_DesiredPosition);

		// Commence le calcul avec une taille à 0.
		float size = 0f;

		// Parcourt toutes les cibles...
		for (int i = 0; i < m_Targets.Length; i++)
		{
			// ... et si l'une d'entre elles n'est pas active, passe à la suivante.
			if (!m_Targets[i].gameObject.activeSelf)
				continue;

			// Sinon, trouve la position de la cible dans l'espace local de la caméra.
			Vector3 targetLocalPos = transform.InverseTransformPoint(m_Targets[i].position);

			// Trouve la position de la cible à partir de la position voulue de l'espace local de la caméra.
			Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;

			// Choisit la plus grande taille parmi toutes et la distance du tank en haut ou en bas de la caméra.
			size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.y));

			// Choisit la plus grande taille parmi toutes suivant la position à gauche ou droite du tank.
			size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.x) / m_Camera.aspect);
		}

		// Ajoute le tampon de bordure à la taille.
		size += m_ScreenEdgeBuffer;

		// S'assure que la taille de la caméra n'est pas inférieure à la taille minimale.
		size = Mathf.Max(size, m_MinSize);

		return size;
	}


	public void SetStartPositionAndSize()
	{
		// Trouve la position voulue.
		FindAveragePosition();

		// Définit la position de la caméra à la position voulue, sans amortissement.
		transform.position = m_DesiredPosition;

		// Trouve et définit la taille de la caméra.
		m_Camera.orthographicSize = FindRequiredSize();
	}
}