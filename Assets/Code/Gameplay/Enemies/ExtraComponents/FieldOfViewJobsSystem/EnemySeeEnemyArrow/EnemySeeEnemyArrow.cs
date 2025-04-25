using Game.Utility;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Room.Enemy
{
    public class EnemySeeEnemyArrow : MonoBehaviour
    {
        private const float _Z_POS_MOVE = -0.05f;

        public void TransformArrow(EnemyBase startEnemy, EnemyBase endEnemy)
        {
            ArrowParameters startPara = startEnemy.ArrowParameters;
            Transform startTrans = startEnemy.transform;
            Vector2 startOffset = startTrans.TransformVector(startPara.offset);
            Vector2 startPos = (Vector2)startTrans.position + startOffset;

            ArrowParameters endPara = endEnemy.ArrowParameters;
            Transform endTrans = endEnemy.transform;
            Vector2 endOffset = endTrans.TransformVector(endPara.offset);
            Vector2 endPos = (Vector2)endTrans.position + endOffset;

            if (transform.localScale.x != endPara.scale)
            {
                transform.localScale = new Vector3(endPara.scale, endPara.scale, endPara.scale);
            }

            float angleToEnemy = Utils.RotateTowards(transform, endPos);
            angleToEnemy = Utils.GetAngleIn180Format(angleToEnemy);
            float enemyAngle = Utils.GetAngleIn180Format(endTrans.eulerAngles.z);
            
            float deltaAngle = math.abs(Utils.GetAngleIn180Format(enemyAngle - angleToEnemy));

            float targetDistanceFromEnemy;
            if(deltaAngle < 90)
            {
                targetDistanceFromEnemy = math.remap(0, 90, endPara.verdicalDistanceFromTarget,
                    endPara.horizontalDistanceFromTarget, deltaAngle);
            }
            else
            {
                targetDistanceFromEnemy = math.remap(90, 180, endPara.horizontalDistanceFromTarget,
                    endPara.verdicalDistanceFromTarget, deltaAngle);
            }

            Vector2 backVector = (startPos - endPos).normalized * targetDistanceFromEnemy;
            transform.position = (Vector3)(endPos + backVector) + new Vector3(0, 0, _Z_POS_MOVE);
        }
    }
}
