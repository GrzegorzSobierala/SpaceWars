using Zenject;

namespace Game.Dialogues
{
    public class BlockMissionDialogueDisplayInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<BlockMissionDialogueDisplay>().FromComponentOn(gameObject).AsSingle();
        }
    }
}
