namespace MagicOGK_OIV_Builder
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new main());
        }

    }
}