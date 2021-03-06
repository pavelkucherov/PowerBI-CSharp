// Code generated by Microsoft (R) AutoRest Code Generator 1.2.2.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace Microsoft.PowerBI.Api.V2.Models
{
    using Microsoft.PowerBI;
    using Microsoft.PowerBI.Api;
    using Microsoft.PowerBI.Api.V2;
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// Connection String wrapper.
    /// </summary>
    public partial class ConnectionDetails
    {
        /// <summary>
        /// Initializes a new instance of the ConnectionDetails class.
        /// </summary>
        public ConnectionDetails()
        {
          CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ConnectionDetails class.
        /// </summary>
        /// <param name="connectionString">A dataset connection string.</param>
        public ConnectionDetails(string connectionString = default(string))
        {
            ConnectionString = connectionString;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets a dataset connection string.
        /// </summary>
        [JsonProperty(PropertyName = "connectionString")]
        public string ConnectionString { get; set; }

    }
}
