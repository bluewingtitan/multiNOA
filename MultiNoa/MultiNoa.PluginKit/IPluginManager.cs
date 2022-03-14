namespace MultiNoa.PlukinKit;

public interface IPluginManager
{
    public T0? GetAsset<T0>(string assetId) where T0 : class, INoaAsset;
    public T0? GetAsset<T0>(NamespacedKey key) where T0 : class, INoaAsset;
}