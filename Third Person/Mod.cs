using PulsarModLoader;
using PulsarModLoader.Keybinds;

namespace Third_Person
{
    public class Mod : PulsarMod, IKeybind
    {
        public override string Version => "1.1";

        public override string Author => "Dunk, Pokegustavo";

        public override string ShortDescription => "Allows to play in third person";

        public override string Name => "ThirdPerson";

        public override string HarmonyIdentifier()
        {
            return "Dunk.ThirdPerson";
        }

        public void RegisterBinds(KeybindManager manager)
        {
            manager.NewBind("ThirdPerson", "ThirdPersonButton", "Basics", "Insert");
        }

    }
}
