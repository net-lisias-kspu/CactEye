using UnityEngine;
using ToolbarControl_NS;
using KSP_Log;

namespace CactEye2
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class InitialSetup : MonoBehaviour
    {
        bool initted = false;
        internal static Log Log = null;
        public static System.Random Random;


        void Awake()
        {
            if (Log == null)
#if DEBUG
                Log = new Log("CactEyeOptics", Log.LEVEL.INFO);
#else
          Log = new Log("CactEyeOptics", Log.LEVEL.ERROR);
#endif

        }

        void Start()
        {
            ToolbarControl.RegisterMod(OccultationScienceEventWindow.MODID, OccultationScienceEventWindow.MODNAME);

            // Initialize rnd using current day milliseconds
            System.DateTime CurrentDate = new System.DateTime();
            CurrentDate = System.DateTime.Now;
            int DaySeconds = ((CurrentDate.Hour * 3600) + (CurrentDate.Minute * 60) + (CurrentDate.Second)) * 1000 + CurrentDate.Millisecond;
            Random = new System.Random(DaySeconds);
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
                CreateWindowStyle();
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
                colors[i] = InitialSetup.Random.Range(0, 100) % 2 == 1 ? black : white;
            }
            texture2D.SetPixels(colors);
            texture2D.Apply();
            return texture2D;
        }
        
        public static GUIStyle windowStyle = null;
        void CreateWindowStyle()
        {
            if (windowStyle == null)
            {
                windowStyle = new GUIStyle(HighLogic.Skin.window);
                //window.normal.background.SetPixels( new[] { new Color(0.5f, 0.5f, 0.5f, 1f) });
                windowStyle.active.background = windowStyle.normal.background;

                Texture2D tex = windowStyle.normal.background; //.CreateReadable();

                var pixels = tex.GetPixels32();

                for (int i = 0; i < pixels.Length; ++i)
                    pixels[i].a = 255;

                tex.SetPixels32(pixels); tex.Apply();

                // one of these apparently fixes the right thing
                // window.onActive.background =
                // window.onFocused.background =
                // window.onNormal.background =
                //window.onHover.background =
                windowStyle.active.background =
                windowStyle.focused.background =
                //window.hover.background =
                windowStyle.normal.background = tex;
            }
        }

    }
}
