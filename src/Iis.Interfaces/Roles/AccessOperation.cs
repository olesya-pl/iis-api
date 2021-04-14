namespace Iis.Interfaces.Roles
{
    public enum AccessOperation : byte
    {
        None,
        Create,
        Read,
        Update,
        Delete,
        Search,
        Commenting,
        AccessLevelUpdate
    }
}
