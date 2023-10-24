using UnityEngine;

namespace Tools.ComponentsComposition
{
	public class ComponentOfRoot : MonoBehaviour
	{
		private IComponentsRoot _root;
		protected IComponentsRoot Root => _root ??= gameObject.GetComponentInParent<IComponentsRoot>();
	}
}