using System;

namespace Game.Utils
{
    public static class StateValueCalculator
    {
        /// <summary>
        /// ���x���ɉ����ď�Ԓl�i�X�e�[�^�X�l�j���X�P�[�����O���ĕԂ��܂��B
        /// </summary>
        /// <param name="baseValue">��ƂȂ�l</param>
        /// <param name="currentLevel">���݂̃��x��</param>
        /// <param name="maxLevel">�ő僌�x��</param>
        /// <param name="maxRate">�ő呝�����i��F0.5f �� �ő�+50%�A-0.3f �� �ő�-30%�j</param>
        /// <param name="exponent">�����J�[�u�i1.0f�Ő��`�A2.0f�Ŏw���J�[�u�j</param>
        /// <returns>�X�P�[�����O���ꂽ��Ԓl</returns>
        public static float CalcStateValue(
            float baseValue,
            int currentLevel,
            int maxLevel = 5,
            float maxRate = 0.5f,
            float exponent = 1f)
        {
            if (currentLevel <= 1) return baseValue;
            if (currentLevel >= maxLevel) return baseValue * (1f + maxRate);

            float progress = MathF.Pow((currentLevel - 1f) / (maxLevel - 1f), exponent);
            return baseValue * (1f + maxRate * progress);
        }
    }
}
