namespace MultiNoa.Networking.Client
{
    /// <summary>
    /// Authority Groups for serverside authority management.
    /// </summary>
    public enum AuthorityGroup
    {
        /// <summary>
        /// The base level everyone shares.
        /// Can't be revoked.
        /// </summary>
        Default = AuthorityGroupConst.Default,
        
        /// <summary>
        /// Ment for handling methods only game clients should be able execute
        /// </summary>
        Client = AuthorityGroupConst.Client,
        
        /// <summary>
        /// Ment for handling methods only other servers in the network should be able to execute
        /// </summary>
        Server = AuthorityGroupConst.Server,
        
        /// <summary>
        /// Ment for use in a scenario, where a single server/app controls others
        /// </summary>
        ControlPlane = AuthorityGroupConst.ControlPlane,
        
        Custom0 = AuthorityGroupConst.Custom0,
        
        Custom1 = AuthorityGroupConst.Custom1,
        
        Custom2 = AuthorityGroupConst.Custom2,
        
        Custom3 = AuthorityGroupConst.Custom3,
        
        
        /// <summary>
        /// Special level, that allows for invocation of ALL methods. THIS SHOULD NEVER BE GIVEN TO NORMAL PLAYERS!
        /// </summary>
        Admin = AuthorityGroupConst.Admin,
    }

    public static class AuthorityGroupConst
    {
        public const int Default = 0;
        public const int Client = 1;
        public const int Server = 2;
        public const int ControlPlane = 3;
        public const int Custom0 = 4;
        public const int Custom1 = 5;
        public const int Custom2 = 6;
        public const int Custom3 = 7;
        public const int Admin = 8;
    }
    
}