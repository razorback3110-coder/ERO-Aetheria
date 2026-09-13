using System;
using UnityEngine;

namespace EternalRealmsOnline.V6
{
    public enum EROV6ConnectionState
    {
        Offline, Connecting, Authenticated, InWorld, Reconnecting, Disconnected
    }

    public sealed class EROV6ConnectionStateMachine : MonoBehaviour
    {
        public EROV6ConnectionState State { get; private set; } = EROV6ConnectionState.Offline;
        public event Action<EROV6ConnectionState> Changed;

        public void Set(EROV6ConnectionState next)
        {
            if (State == next) return;
            State = next;
            Changed?.Invoke(next);
        }
    }
}
