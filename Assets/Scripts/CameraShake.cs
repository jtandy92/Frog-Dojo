using System.Collections;
using UnityEngine;

namespace Sv6.Dojo.State
{
    public class CameraShake : MonoBehaviour
    {
      public Animator camAnim;

      public void Shake(){
        camAnim.SetTrigger("shake");
      }
    }
}
