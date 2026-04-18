namespace MagicOGK_OIV_Builder
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            using (SplashForm splash = new SplashForm())
            {
                splash.ShowDialog();
            }

            main app = new main();
            Application.Run(app);
        }
    }
}