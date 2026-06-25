//using UnityEngine;
//using Unity.Cinemachine; // Если у вас новая версия Unity (2023+), или Unity.Netcode для старых

//public class CinemachineNetBinder : MonoBehaviour
//{
//    private CinemachineCamera vcam; // Если старая Unity, напишите CinemachineVirtualCamera

//    private void Awake()
//    {
//        vcam = GetComponent<CinemachineCamera>(); // Или CinemachineVirtualCamera
//    }

//    private void LateUpdate()
//    {
//        // Если камера уже за кем-то следит, ничего не делаем
//        if (vcam.Follow != null) return;

//        // Ждем, пока в сети появится НАШ локальный игрок
//        if (NetworkPlayer.LocalInstance != null)
//        {
//            Transform playerTransform = NetworkPlayer.LocalInstance.transform;

//            // Назначаем игрока целью для следования
//            vcam.Follow = playerTransform;
//            vcam.LookAt = playerTransform;
//        }
//    }
//}
