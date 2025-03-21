using System;
using SimpleJwt.Abstractions.Serialization;

namespace SimpleJwt.Core.Serialization
{
    /// <summary>
    /// This class exists only to guide users to the correct package. 
    /// To use System.Text.Json, add a reference to the SimpleJwt.SystemTextJson package.
    /// </summary>
    [Obsolete("This class is not implemented. Please use SimpleJwt.SystemTextJson.Serialization.SystemTextJsonProvider from the SimpleJwt.SystemTextJson package instead.", true)]
    public class SystemTextJsonProviderPlaceholder
    {
        /// <summary>
        /// Constructor that throws an error to guide users to the correct package.
        /// </summary>
        public SystemTextJsonProviderPlaceholder()
        {
            throw new NotSupportedException(
                "This class is not implemented. To use System.Text.Json, add a reference to the SimpleJwt.SystemTextJson package " +
                "and use SimpleJwt.SystemTextJson.Serialization.SystemTextJsonProvider instead."
            );
        }
    }
} 