namespace MultiNoa.PlukinKit;

/// <summary>
/// Adressed inside of a plugin as {Namespace}/{Subspace}:{Name}.
/// </summary>
public interface INoaAsset
{
    public NamespacedKey Key { get; }
    
    #region Helpers

    /// <summary>
    /// Tries to generate a new instance of this plugin from the specified plugin manager with this assets namespaced key.
    /// </summary>
    /// <param name="manager">Plugin Manager to use</param>
    /// <returns>Null or new Instance of this asset-type</returns>
    public virtual INoaAsset? GetNewInstance(PluginManager manager)
    {
        return manager.GetAsset<INoaAsset>(Key);
    }

    public string Namespace => Key.Namespace;
    public string Subspace => Key.Subspace;
    public string Name => Key.AssetName;

    #endregion

}