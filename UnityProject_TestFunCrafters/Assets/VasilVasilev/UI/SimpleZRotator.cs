using UnityEngine;

namespace VasilVasilev.UI
{
    public class SimpleZRotator : MonoBehaviour
    {
        [SerializeField] private float _speed;
        void Update()
        {
            transform.Rotate(Vector3.forward, _speed * Time.deltaTime);
        }
    }
}
