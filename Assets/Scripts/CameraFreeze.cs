using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Sv6.Dojo.State
{
    public class CameraFreeze : MonoBehaviour
    {
        [Range(0f, 1.5f)]
        public float duration = 0.1f;

        private bool _isFrozen = false;
        private float _pendingFreezeDuration = 0f;

        private void Update()
        {
            // If we have a freeze duration pending and we're not already frozen,
            // start the freeze coroutine
            if (_pendingFreezeDuration > 0 && !_isFrozen)
            {
                StartCoroutine(DoFreeze());
            }
        }

        // Call this method to request a camera freeze
        public void Freeze()
        {
            _pendingFreezeDuration = duration;
        }

        private IEnumerator DoFreeze()
        {
            _isFrozen = true;
            float originalTimeScale = Time.timeScale;

            // Freeze the game by setting time scale to zero
            Time.timeScale = 0f;

            // Wait in real time so the freeze is unaffected by time scale changes
            yield return new WaitForSecondsRealtime(duration);

            // Restore time scale
            Time.timeScale = originalTimeScale;
            _pendingFreezeDuration = 0;
            _isFrozen = false;
        }
    }
}
