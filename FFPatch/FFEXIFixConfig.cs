// IFix configuration - specifies which types have [IFix.Patch] methods
// IFix Editor scans this class to know what to inject

using System;
using COW;
using COW.GamePlay;

namespace FFEX
{
    public class IFixConfig
    {
        // These types contain [IFix.Patch] methods
        // IFix will hook them at runtime
        static readonly Type[] PatchedTypes = new Type[]
        {
            typeof(GameFacade),
            typeof(Player),
            typeof(PlayerAttributes),
            typeof(GameSettingData),
        };
    }
}
