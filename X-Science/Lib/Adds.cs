using UnityEngine;

namespace ScienceChecklist.Lib.Adds
{
    public static class Adds
    {
        public static float AcceleratedSlider(float value, float leftValue, float rightValue, float pow, StepRule[] stepRules, params GUILayoutOption[] options)
        {
            float realMax = rightValue - leftValue;
            float hiddenMax = RealToHidden(realMax, pow);
            float realValue = value - leftValue;
            float hiddenValue = RealToHidden(realValue, pow);

            float newHiddenValue = GUILayout.HorizontalSlider(hiddenValue, 0f, hiddenMax, options);
            
            if (hiddenValue == newHiddenValue)
            {
                return value;
            }
            else if (newHiddenValue == 0f)
            {
                return leftValue;
            }
            else if (newHiddenValue == hiddenMax)
            {
                return rightValue;
            }

            realValue = HiddenToReal(newHiddenValue, pow) + leftValue;
            float stepMultiplier = 1;

            for (int i = 0; i < stepRules.Length; i++)
            {
                if (realValue < stepRules[i].ApplyLimit)
                {
                    stepMultiplier = 1 / stepRules[i].Step;
                    break;
                }
            }
            
            realValue = Mathf.Round((realValue) * stepMultiplier) / stepMultiplier;
            realValue = Mathf.Min(rightValue, Mathf.Max(leftValue, realValue));

            return realValue;
        }

        public struct StepRule
        {
            public float Step;
            public float ApplyLimit;

            public StepRule(float step, float applyLimit)
            {
                Step = step;
                ApplyLimit = applyLimit;
            }
        }

        public static float Map(float value, float left, float right, float newLeft, float newRight)
        {
            return (value - left) / (right - left) * (newRight - newLeft) + newLeft;
        }

        private static float RealToHidden(float realValue, float pow)
        {
            return Mathf.Pow(realValue, 1 / pow);
        }

        private static float HiddenToReal(float hiddenValue, float pow)
        {
            return Mathf.Pow(hiddenValue, pow);
        }
    }
}
