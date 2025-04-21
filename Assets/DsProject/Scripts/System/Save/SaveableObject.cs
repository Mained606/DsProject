using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DsProject.Scripts.System.Save
{
    /// <summary>
    /// 게임 오브젝트의 저장/로드 가능한 상태를 관리하는 컴포넌트
    /// 이 컴포넌트를 게임 오브젝트에 추가하면 활성화 상태가 저장/로드됩니다.
    /// </summary>
    public class SaveableObject : MonoBehaviour
    {
        [Tooltip("이 오브젝트의 고유 ID (비워두면 자동 생성)")]
        public string objectId;
        
        [Tooltip("저장/로드할 속성들")]
        [SerializeField] private bool saveActivationState = true;
        [SerializeField] private bool savePosition = false;
        [SerializeField] private bool saveRotation = false;
        
        private void Awake()
        {
            // ID가 비어있으면 자동 생성
            if (string.IsNullOrEmpty(objectId))
            {
                objectId = gameObject.name + "_" + Guid.NewGuid().ToString().Substring(0, 8);
            }
            
            // SaveManager에 등록
            SaveManager.OnGameSaved += OnGameSaved;
            SaveManager.OnGameLoaded += OnGameLoaded;
        }
        
        private void OnDestroy()
        {
            // SaveManager에서 등록 해제
            SaveManager.OnGameSaved -= OnGameSaved;
            SaveManager.OnGameLoaded -= OnGameLoaded;
        }
        
        /// <summary>
        /// 게임 저장 시 호출되는 이벤트
        /// </summary>
        private void OnGameSaved(SaveData saveData)
        {
            // 오브젝트 상태 정보 추가
            if (saveData.saveableObjects == null)
            {
                saveData.saveableObjects = new SaveData.SaveableObjectsData();
            }
            
            // 이미 있는 항목인지 확인
            SaveData.SaveableObjectsData.ObjectInfo existingInfo = null;
            if (saveData.saveableObjects.objects != null)
            {
                existingInfo = saveData.saveableObjects.objects.Find(obj => obj.id == objectId);
            }
            
            // 새 항목이면 추가
            if (existingInfo == null)
            {
                var newInfo = new SaveData.SaveableObjectsData.ObjectInfo
                {
                    id = objectId,
                    isActive = gameObject.activeSelf,
                    position = transform.position,
                    rotation = transform.rotation
                };
                
                if (saveData.saveableObjects.objects == null)
                {
                    saveData.saveableObjects.objects = new List<SaveData.SaveableObjectsData.ObjectInfo>();
                }
                
                saveData.saveableObjects.objects.Add(newInfo);
            }
            else // 기존 항목이면 업데이트
            {
                existingInfo.isActive = gameObject.activeSelf;
                existingInfo.position = transform.position;
                existingInfo.rotation = transform.rotation;
            }
            
            //Debug.Log($"저장 가능 오브젝트 '{gameObject.name}' (ID: {objectId}) 상태 저장 (활성화: {gameObject.activeSelf})");
        }
        
        /// <summary>
        /// 게임 로드 시 호출되는 이벤트
        /// </summary>
        private void OnGameLoaded(SaveData saveData)
        {
            // 저장된 오브젝트 데이터가 있는지 확인
            if (saveData.saveableObjects != null && saveData.saveableObjects.objects != null)
            {
                // 이 오브젝트의 저장 데이터 찾기
                var objectInfo = saveData.saveableObjects.objects.Find(obj => obj.id == objectId);
                
                if (objectInfo != null)
                {
                    // 활성화 상태 복원
                    if (saveActivationState)
                    {
                        gameObject.SetActive(objectInfo.isActive);
                        //Debug.Log($"저장 가능 오브젝트 '{gameObject.name}' (ID: {objectId}) 활성화 상태를 {objectInfo.isActive}로 설정");
                    }
                    
                    // 위치 복원
                    if (savePosition)
                    {
                        transform.position = objectInfo.position;
                    }
                    
                    // 회전 복원
                    if (saveRotation)
                    {
                        transform.rotation = objectInfo.rotation;
                    }
                }
                else
                {
                    Debug.LogWarning($"저장 가능 오브젝트 '{gameObject.name}' (ID: {objectId})의 저장 데이터를 찾을 수 없습니다.");
                }
            }
        }
    }
} 