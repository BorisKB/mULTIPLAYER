using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Cinemachine;

public class CameraPlayer : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    public void SetCameraFollow(Transform target)
    {
        if (virtualCamera.Follow == null)
        {
            virtualCamera.Follow = target;
            virtualCamera.LookAt = target;
        }
        return;
    }

}
