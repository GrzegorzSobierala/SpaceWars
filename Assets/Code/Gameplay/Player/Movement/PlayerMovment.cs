using UnityEngine;
using Game.Utility;

namespace Game.Player
{
    public class PlayerMovment : MonoBehaviour
    {
        [SerializeField] Transform cursorPointer;
        [SerializeField] Transform centerOfMass;
        [SerializeField] Transform rotationChecker;
        [SerializeField] PlayerInput /*input*/;
        [SerializeField] new Rigidbody rigidbody;
        [SerializeField, Range(0.0f, 60000.0f)] float speed = 0;
        [SerializeField, Range(0.0f, 200.0f)] float percentOfMaxSpeedForHorzontalMove = 50;
        [SerializeField, Range(0.0f, 200.0f)] float percentOfMaxSpeedForBackMove = 30;
        [SerializeField, Range(0.0f, 200.0f)] float percentOfMaxSpeedForForwardMove = 100;
        [SerializeField] float rotationSpeed = 50;

        private Option lastHorizontalSide = Option.Defult;
        private Option lastVerdicalSide = Option.Defult;
        private Vector3 rotationSpeedVector => new Vector3(0, rotationSpeed, 0);

        private void Start()
        {
            cursorPointer.localPosition = rigidbody.centerOfMass;
            centerOfMass.localPosition = rigidbody.centerOfMass;
            rotationChecker.localPosition = rigidbody.centerOfMass;
        }

        private void FixedUpdate()
        {
            HorizontalMove();
            VerdicalMove();
            /zcursorPointer.LookAt(new Vector3(input.CursorPosition.x, transform.position.y, input.CursorPosition.z));
            RotateToCursor();
        }

        private void VerdicalMove()
        {
            Option side = ChooseLastUsedOption(input.Is_S_Press, input.Is_W_Press, ref lastVerdicalSide);
            if (side == Option.Option1)
            {
                MovePlayer(Vector3.back, percentOfMaxSpeedForBackMove);
                return;
            }
            if (side == Option.Option2)
            {
                MovePlayer(Vector3.forward, percentOfMaxSpeedForForwardMove);
                return;
            }
        }

        private void HorizontalMove()
        {
            Option side = ChooseLastUsedOption(input.Is_A_Press, input.Is_D_Press, ref lastHorizontalSide);
            if (side == Option.Option1)
            {
                MovePlayer(Vector3.left, percentOfMaxSpeedForHorzontalMove);
                return;
            }
            if (side == Option.Option2)
            {
                MovePlayer(Vector3.right, percentOfMaxSpeedForHorzontalMove);
                return;
            }
        }

        private Option ChooseLastUsedOption(bool option1, bool option2, ref Option lastUsedOption)
        {
            if (option1 && option2)
            {
                if (lastUsedOption == Option.Option1)
                {
                    return Option.Option2;
                }
                if (lastUsedOption == Option.Option2)
                {
                    return Option.Option1;
                }
            }
            if (option1)
            {
                lastUsedOption = Option.Option1;
                return Option.Option1;
            }
            if (option2)
            {
                lastUsedOption = Option.Option2;
                return Option.Option2;
            }
            return Option.Defult;
        }

        private void MovePlayer(Vector3 vector3, float procentOfMaxSpeed)
        {
            rigidbody.AddRelativeForce(vector3 * speed * (procentOfMaxSpeed / 100));
        }

        private void RotateToCursor()
        {
            var normalDeltaForward = Vector3.Dot(cursorPointer.forward, centerOfMass.forward);
            var normalDeltaRight = Vector3.Dot(cursorPointer.forward, centerOfMass.right);

            Vector2 dotVector = new Vector2(normalDeltaForward, normalDeltaRight);

            if (dotVector.y >= 0f)
            {
                Rotate(dotVector, false);
            }
            else
            {
                Rotate(dotVector, true);
            }
        }

        private void Rotate(Vector2 dotVector, bool isLeft)
        {
            float rotationMultiplier;
            if (isLeft)
                rotationMultiplier = -1;
            else
                rotationMultiplier = 1;

            Quaternion targetRotation = Quaternion.Euler(rotationSpeedVector * Time.fixedDeltaTime * rotationMultiplier) * rigidbody.rotation;
            rotationChecker.rotation = targetRotation;

            var normalDeltaForward = Vector3.Dot(cursorPointer.forward, rotationChecker.forward);
            var normalDeltaRight = Vector3.Dot(cursorPointer.forward, rotationChecker.right);
            Vector2 chceckerDotVector = new Vector2(normalDeltaForward, normalDeltaRight);

            bool isNormalMove;
            if (isLeft)
                isNormalMove = chceckerDotVector.y <= 0;
            else
                isNormalMove = chceckerDotVector.y >= 0;

            if (isNormalMove)
            {
                rigidbody.MoveRotation(targetRotation);
            }
            else
            {
                rigidbody.MoveRotation(cursorPointer.rotation);
            }
        }
    }
}
