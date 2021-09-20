using PulsarModLoader;

namespace Third_Person
{
    public class Mod : PulsarMod
    {
        public override string Version => "1.0";

        public override string Author => "Dunk";

        public override string ShortDescription => "Allows to play in third person";

        public override string Name => "ThirdPerson";

        public override string HarmonyIdentifier()
        {
            return "Dunk.ThirdPerson";
        }
    }
}
