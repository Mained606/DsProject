using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SequenceData", menuName = "Ds Project/SequenceData")]
public class SequenceData : ScriptableObject
{
    public int sequenceIndex;
    public int sequenceCount;
    public List<DialogData> sequencesDialog;
    public bool isComplete;

    public List<StoryEventType> events;
    public CameraPose cameraPose;
    public CharacterActionType characterAction;
    public SequenceTriggerType triggerType;
    public CharacterMoveData moveData;
    public CameraEffectData cameraEffect;
    public SoundData soundData;
    public ScreenEffectData screenEffect;
}