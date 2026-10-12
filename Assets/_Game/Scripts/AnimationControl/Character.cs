using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChangeAnim
{
    public class Character : MonoBehaviour
    {
        [SerializeField] Animator anim;
        private string animName;

        void Start()
        {
            ChangeAnim(Constants.ANIM_IDLE);
        }

        public void ChangeAnim(string animName)
        {
            if (this.animName != animName)
            {
                if (!string.IsNullOrEmpty(this.animName)) anim.ResetTrigger(this.animName);
                this.animName = animName;
                anim.SetTrigger(this.animName);
            }
        }
    }
}
