namespace MultiNoa.PlukinKit;

public interface INoaPlugin
{
    string Namespace { get; }
    public T0? GetAsset<T0>(NamespacedKey key) where T0: class, INoaAsset;
}