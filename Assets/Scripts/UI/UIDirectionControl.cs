using UnityEngine;

public class UIDirectionControl : MonoBehaviour
{
	// Cette classe est utilisée pour s'assurer que les éléments d'interface utilisateur en espace monde
	// telle que la barre de vie garde la direction appropriée.

	public bool m_UseRelativeRotation = true;       // Est-ce qu'une rotation relative doit être utilisée pour ce GameObject ?


	private Quaternion m_RelativeRotation;          // La rotation locale au début de la scène.


	private void Start()
	{
		m_RelativeRotation = transform.parent.localRotation;
	}


	private void Update()
	{
		if (m_UseRelativeRotation)
			transform.rotation = m_RelativeRotation;
	}
}