using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SequenceData", menuName = "Ds Project/SequenceData")]
public class SequenceData : ScriptableObject
{
    public int sequenceIndex;  // 현재 시퀀스 번호
    public int sequenceCount;  // 전체 시퀀스 개수
    public List<DialogData> sequencesDialog; // 대사 목록
    public bool isComplete;  // 완료 여부

    public CameraPose cameraPose;
    public CharacterActionType characterAction;
    public SequenceTriggerType triggerType;
    public CharacterMoveData moveData;
    public CameraEffectData cameraEffect;
    public SoundData soundData;
    public ScreenEffectData screenEffect;
}