using UnityEngine;
using ToolbarControl_NS;

namespace CactEye2
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        bool initted = false;
        void Start()
        {
            ToolbarControl.RegisterMod(CactEyeConfigMenu.MODID, CactEyeConfigMenu.MODNAME);
        }
        void OnGUI()
        {
            if (!initted)
            {
                initted = true;
                TelescopeMenu.InitStatics();
            }

        }
    }
}
