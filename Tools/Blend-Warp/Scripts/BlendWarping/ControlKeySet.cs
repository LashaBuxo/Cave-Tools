using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Blend_Warp.Script.BlendWarping
{
    public enum ControllingModes
    {
        BlendingDegree,

        BlendingLeft,
        BlendingRight,
        BlendingUp,
        BlendingDown,

        Brightness
    }

    public enum Controls
    {
        Amplify,
        //Blending Controls
        BlendingIncrease,
        BlendingDecrease,

        //Brightness Controls
        BrightnessIncrease,
        BrightnessDecrease,

        //Warping Controls
        WarpingIncreaseX,
        WarpingDecreaseX,
        WarpingIncreaseY,
        WarpingDecreaseY,
    }

    static class ControlKeySet
    {

        private static Dictionary<ControllingModes, KeyCode> ControllingModeHotkeys = new Dictionary<ControllingModes, KeyCode>()
            {
                {ControllingModes.BlendingDegree, KeyCode.Y},
                {ControllingModes.BlendingLeft, KeyCode.L},
                {ControllingModes.BlendingRight, KeyCode.R},
                 {ControllingModes.BlendingUp, KeyCode.U},
                  {ControllingModes.BlendingDown, KeyCode.D},

                {ControllingModes.Brightness, KeyCode.B}
            };

        private static Dictionary<Controls, KeyCode> ControllingHotkeys = new Dictionary<Controls, KeyCode>()
            {
                {Controls.Amplify, KeyCode.LeftShift},

                {Controls.BlendingIncrease, KeyCode.UpArrow},
                {Controls.BlendingDecrease, KeyCode.DownArrow},

                {Controls.BrightnessIncrease, KeyCode.UpArrow},
                {Controls.BrightnessDecrease, KeyCode.DownArrow},

                {Controls.WarpingIncreaseX, KeyCode.RightArrow},
                {Controls.WarpingDecreaseX, KeyCode.LeftArrow},
                {Controls.WarpingIncreaseY, KeyCode.UpArrow},
                {Controls.WarpingDecreaseY, KeyCode.DownArrow},
            };

        public static bool WarpPermitted()
        {
            bool RtnVal = true;
            foreach (ControllingModes mode in Enum.GetValues(typeof(ControllingModes)))
            {
                RtnVal &= (!Input.GetKey(ControllingModeHotkeys[mode]));
            }
            return RtnVal;
        }

        public static bool BlendPermitted()
        {
            return Input.GetKey(ControllingModeHotkeys[ControllingModes.BlendingLeft]) 
                || Input.GetKey(ControllingModeHotkeys[ControllingModes.BlendingRight])
                || Input.GetKey(ControllingModeHotkeys[ControllingModes.BlendingUp])
                || Input.GetKey(ControllingModeHotkeys[ControllingModes.BlendingDown]
                );
        }

        public static KeyCode GetModeKey(ControllingModes mode)
        {
            return ControllingModeHotkeys[mode];
        }

        public static KeyCode GetControlKey(Controls control)
        {
            return ControllingHotkeys[control];
        }
    }
}
