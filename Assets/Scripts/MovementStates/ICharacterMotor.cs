// ICharacterMotor.cs
using UnityEngine;

public interface ICharacterMotor
{
    void MoveCharacter(Vector2 input);  // Changed from Move to MoveCharacter to avoid confusion
    void SetSpeed(float speed);
    void SetAnimationBool(string parameter, bool value);
    void SetAnimationFloat(string parameter, float value);
}