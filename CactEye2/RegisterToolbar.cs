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

                    TelescopeMenu.TextureNoSignal = new Texture2D[8];
                    for (int i = 0; i < TelescopeMenu.TextureNoSignal.Length; i++)
                    {
                        TelescopeMenu.TextureNoSignal[i] = WhiteNoiseTexture(128, 128, 1f);
                    }
            }
        }

        public Texture2D WhiteNoiseTexture(int width, int height, float alpha = .16f)
        {
            var black = new Color(0, 0, 0, alpha);
            var white = new Color(1, 1, 1, alpha);
            width *= 2;
            height *= 2;
            var texture2D = new Texture2D(width, height);
            var colors = new Color[width * height];
            for (int i = 0; i < width * height; i++)
            {
                colors[i] = UnityEngine.Random.Range(0, 100) % 2 == 1 ? black : white;
            }
            texture2D.SetPixels(colors);
            texture2D.Apply();
            return texture2D;
        }

    }
}
