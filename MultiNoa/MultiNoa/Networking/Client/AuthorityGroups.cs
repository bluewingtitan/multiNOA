namespace MultiNoa.Networking.Client
{
    /// <summary>
    /// A collection of Authority-Group strings as used inside aton.
    /// Feel free to not use this, or to use it as a starting point to expand upon.
    /// </summary>
    public static class AuthorityGroups
    {
        /// <summary>
        /// multiNoa equivalent of linux "root" user. Don't use it in production...
        /// </summary>
        public const string Operator = "multiNoa:OPERATOR";
        
        /// <summary>
        /// The default group that every user has. Not revokable.
        /// </summary>
        public const string Default = "multiNoa:DEFAULT";
        
        /// <summary>
        /// Group for other servers in the network, used serverside.
        /// </summary>
        public const string OtherServer = "multiNoa:OTHER_SERVER";
        
        /// <summary>
        /// Group for distant servers, used clientside.
        /// </summary>
        public const string Server = "multiNoa:SERVER";
        
        /// <summary>
        /// Group for clients, used serverside.
        /// </summary>
        public const string Client = "multiNoa:CLIENT";
        
        /// <summary>
        /// Group for seperate apps used to control a server(network).
        /// </summary>
        public const string ControlPlain = "multiNoa:CONTROL_PLAIN";

    }
}