using MarioTest.Core;
using UnityEngine;

namespace MarioTest.Enemies
{
    public static class ProjectilePrediction
    {
        public static Vector3 PredictPosition(
            Vector3 origin,
            Vector3 targetPosition,
            Vector3 targetVelocity,
            float projectileSpeed,
            int iterations = 3)
        {
            if (projectileSpeed <= GameplayEpsilon.MinSpeed)
            {
                return targetPosition;
            }

            float targetY = targetPosition.y;
            Vector3 horizontalOrigin = new Vector3(origin.x, 0f, origin.z);
            Vector3 horizontalTarget = new Vector3(targetPosition.x, 0f, targetPosition.z);
            Vector3 horizontalVelocity = new Vector3(targetVelocity.x, 0f, targetVelocity.z);

            Vector3 toTarget = horizontalTarget - horizontalOrigin;
            float timeToImpact = toTarget.magnitude / projectileSpeed;

            for (int i = 0; i < iterations; i++)
            {
                Vector3 predictedHorizontal = horizontalTarget + horizontalVelocity * timeToImpact;
                toTarget = predictedHorizontal - horizontalOrigin;
                timeToImpact = toTarget.magnitude / projectileSpeed;
            }

            Vector3 predictedPosition = horizontalTarget + horizontalVelocity * timeToImpact;
            predictedPosition.y = targetY;
            return predictedPosition;
        }
    }
}
