using Modding;

namespace FountainPreview
{
    public class FountainPreview : Mod
    {
        internal static FountainPreview instance;
        
        public FountainPreview() : base(null)
        {
            instance = this;
        }
        
        public override string GetVersion()
        {
            return GetType().Assembly.GetName().Version.ToString();
        }
        
        public override void Initialize()
        {
            Log("Initializing Mod...");

            BasinVesselTag.Hook();
        }

    }
}