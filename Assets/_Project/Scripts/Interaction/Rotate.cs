using UnityEngine;

namespace MarioTest.Interaction
{
    [DisallowMultipleComponent]
    public sealed class Rotate : MonoBehaviour
    {
        [SerializeField] private Vector3 _axis = Vector3.up;
        [SerializeField] private float _degreesPerSecond = 180f;

        private void Update()
        {
            transform.Rotate(_axis, _degreesPerSecond * Time.deltaTime, Space.Self);
        }
    }
}
