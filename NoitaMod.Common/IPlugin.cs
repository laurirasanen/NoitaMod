namespace NoitaMod.Common
{
    public struct PluginInfo
    {
        public string Name;
    }

    public interface IPlugin
    {
        PluginInfo PluginInfo { get; set; }

        void OnLoad( Host host );

        void OnUnload();
    }
}
