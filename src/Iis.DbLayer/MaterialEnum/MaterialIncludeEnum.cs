namespace Iis.DbLayer.MaterialEnum
{
    public enum MaterialIncludeEnum: byte
    {
        WithChildren = 0,
        WithFeatures,
        OnlyParent,
        WithNodes,
        WithFiles
    }

}