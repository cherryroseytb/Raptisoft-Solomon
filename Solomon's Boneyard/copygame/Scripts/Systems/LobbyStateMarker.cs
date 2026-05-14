using UnityEngine;

namespace SolomonCopy.Systems
{
    // 로비 씬에만 두는 마커. 활성화된 동안 InLobby=true.
    public class LobbyStateMarker : MonoBehaviour
    {
        public static bool InLobby { get; private set; }

        private void OnEnable() { InLobby = true; }
        private void OnDisable() { InLobby = false; }
        private void OnDestroy() { InLobby = false; }
    }
}
