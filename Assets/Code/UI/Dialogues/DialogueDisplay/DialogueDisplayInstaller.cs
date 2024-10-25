using Game.Utility;
using Zenject;

namespace Game.Dialogues
{
    public class DialogueDisplayInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Utils.BindGetComponent<DialogueDisplayBase>(Container, gameObject);
        }
    }
}
