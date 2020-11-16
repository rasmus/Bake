namespace Bake.Books.Cooks.DotNet
{
    public class DotNetBuildCook : ICook
    {
        public void Initialize(ICookInitializer cookInitializer)
        {
            cookInitializer
                .DependOn<DotNetCleanCook>();
        }
    }
}
