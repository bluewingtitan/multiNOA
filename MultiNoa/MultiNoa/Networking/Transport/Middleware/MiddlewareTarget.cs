namespace MultiNoa.Networking.Transport.Middleware
{
    /// <summary>
    /// Defines the task this middleware aims to do, also defines the position inside the middleware chain.
    ///
    /// Middleware chain: When sending: Checking -> Encrypting -> Fragmenting -> Correcting -> NonModifying; When receiving: Inverted sending chain.
    /// </summary>
    public enum MiddlewareTarget
    {
        /// <summary>
        /// Only layer with guaranteed access to unencrypted byte data, used to do allow for content aware checks.
        /// </summary>
        Checking = 0,
        
        /// <summary>
        /// Layer that encrypts/decrypts data
        /// </summary>
        Encrypting = 1,
        
        /// <summary>
        /// Layer that fragments bigger packets into small ones
        /// </summary>
        Fragmenting = 2,
        
        /// <summary>
        /// Layer that corrects data that arrives
        /// </summary>
        Correcting = 3,
        
        /// <summary>
        /// Layer that does not modify data, mainly aimed towards logging and statistics
        /// </summary>
        NonModifying = 4,
        
        /// <summary>
        /// Middlewares of this type won't be called. Ever.
        /// </summary>
        Dummy = 512,
        
    }
}