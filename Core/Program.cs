namespace ImmersiveEnvironment.Core
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            RenderEngine.InitializeRender();
			RenderEngine.InitializeContent();
			RenderEngine.RenderLoop();
        }
    }
}