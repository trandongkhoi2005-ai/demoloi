using System;
using UnityEngine;
using COW;
using COW.GamePlay;
using IFix;

namespace FFEX
{
    // Entry point - IFix injects this into GameFacade.Awake()
    // Creates persistent GameObject hosting Menu, ESP, Aimbot components
}

namespace COW
{
    partial class GameFacade
    {
        private static bool _ffexLoaded = false;

        [IFix.Patch]
        void Awake()
        {
            // Call original first
            IFix.ILFixBridge.Call<GameFacade>("Awake", this);

            if (_ffexLoaded) return;
            _ffexLoaded = true;

            // Bootstrap FFEX
            var go = new GameObject("__FFEX__");
            UnityEngine.Object.DontDestroyOnLoad(go);

            FFEX.Config.Init();

            go.AddComponent<FFEX.Esp>();
            go.AddComponent<FFEX.Menu>();

            UnityEngine.Debug.Log("[FFEX] Loaded. Tap 3x anywhere to open menu.");
        }
    }
}
