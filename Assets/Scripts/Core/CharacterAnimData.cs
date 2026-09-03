using UnityEngine;

[CreateAssetMenu(fileName = "CharacterAnimData", menuName = "Roguelike/CharacterAnimData")]
public class CharacterAnimData : ScriptableObject
{
    public Sprite[] idleFrames;
    public Sprite[] runFrames;
    public float fps = 8f;
}
