using System.Reflection;
using MultiNoa.Logging;

namespace MultiNoa.PlukinKit;

public sealed class PluginManager: IPluginManager
{
    private readonly string _path;
    private readonly Dictionary<string, INoaPlugin> _plugins = new Dictionary<string, INoaPlugin>();

    private PluginManager(string path)
    {
        _path = path;
    }

    private void LoadAssemblies()
    {
        if(_plugins.Count > 0)
            return;
        
        DirectoryInfo dir = new DirectoryInfo(_path);

        foreach (FileInfo file in dir.GetFiles("*.dll"))
        {
            Assembly ass = Assembly.LoadFrom(file.FullName);
            LoadPluginsFromAssembly(ass);
        }
        
    }
    
    private void LoadPluginsFromAssembly(Assembly ass)
    {
        foreach(Type t in ass.GetTypes())
        {
            if ((!t.IsSubclassOf(typeof(INoaPlugin)) && !t.GetInterfaces().Contains(typeof(INoaPlugin))) ||
                t.IsAbstract) continue;
            
            var b = (INoaPlugin?) Activator.CreateInstance(t);

            if (b == null)
            {
                MultiNoaLoggingManager.Logger.Warning($"Unable to load plugin of type {t.FullName}: No accessible and empty constructor");
                continue;
            }

            var space = b.Namespace;
                
            MultiNoaLoggingManager.Logger.Debug($"Load Plugin {t.Namespace} ({t.FullName}");
            _plugins.Add(space, b);
        }
    }

    public static PluginManager Create(string path)
    {
        var manager = new PluginManager(path);
        manager.LoadAssemblies();
        return manager;
    }

    public T0? GetAsset<T0>(string assetId) where T0 : class, INoaAsset
    {
        var key = NamespacedKey.FromString(assetId);
        return key == null ? null : GetAsset<T0>((NamespacedKey) key);
    }

    public T0? GetAsset<T0>(NamespacedKey key) where T0 : class, INoaAsset
    {
        return !_plugins.ContainsKey(key.Namespace) ? null : _plugins[key.Namespace].GetAsset<T0>(key);
    }
}