// Code generated by Microsoft (R) AutoRest Code Generator 1.2.2.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace Microsoft.PowerBI.Api.V2.Models
{
    using Microsoft.PowerBI;
    using Microsoft.PowerBI.Api;
    using Microsoft.PowerBI.Api.V2;
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Power BI Generate Token Request
    /// </summary>
    public partial class GenerateTokenRequest
    {
        /// <summary>
        /// Initializes a new instance of the GenerateTokenRequest class.
        /// </summary>
        public GenerateTokenRequest()
        {
          CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the GenerateTokenRequest class.
        /// </summary>
        /// <param name="accessLevel">The dataset mode or type. Possible values
        /// include: 'View', 'Edit', 'Create'</param>
        /// <param name="datasetId">The dataset Id</param>
        /// <param name="allowSaveAs">Allow SaveAs the report with generated
        /// token.</param>
        /// <param name="identities">The effective identities to use for RLS
        /// rules</param>
        public GenerateTokenRequest(string accessLevel = default(string), string datasetId = default(string), bool? allowSaveAs = default(bool?), IList<EffectiveIdentity> identities = default(IList<EffectiveIdentity>))
        {
            AccessLevel = accessLevel;
            DatasetId = datasetId;
            AllowSaveAs = allowSaveAs;
            Identities = identities;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets the dataset mode or type. Possible values include:
        /// 'View', 'Edit', 'Create'
        /// </summary>
        [JsonProperty(PropertyName = "accessLevel")]
        public string AccessLevel { get; set; }

        /// <summary>
        /// Gets or sets the dataset Id
        /// </summary>
        [JsonProperty(PropertyName = "datasetId")]
        public string DatasetId { get; set; }

        /// <summary>
        /// Gets or sets allow SaveAs the report with generated token.
        /// </summary>
        [JsonProperty(PropertyName = "allowSaveAs")]
        public bool? AllowSaveAs { get; set; }

        /// <summary>
        /// Gets or sets the effective identities to use for RLS rules
        /// </summary>
        [JsonProperty(PropertyName = "identities")]
        public IList<EffectiveIdentity> Identities { get; set; }

    }
}
