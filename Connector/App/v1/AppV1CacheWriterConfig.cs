namespace Connector.App.v1;
using Connector.App.v1.FederalTaxProfiles;
using Connector.App.v1.LaborChargeCodes;
using Connector.App.v1.LocalTaxProfiles;
using Connector.App.v1.StateTaxProfiles;
using Connector.App.v1.TimeCards;
using Connector.App.v1.Workers;
using ESR.Hosting.CacheWriter;
using Json.Schema.Generation;

/// <summary>
/// Configuration for the Cache writer for this module. This configuration will be converted to a JsonSchema, 
/// so add attributes to the properties to provide any descriptions, titles, ranges, max, min, etc... 
/// The schema will be used for validation at runtime to make sure the configurations are properly formed. 
/// The schema also helps provide integrators more information for what the values are intended to be.
/// </summary>
[Title("App V1 Cache Writer Configuration")]
[Description("Configuration of the data object caches for the module.")]
public class AppV1CacheWriterConfig
{
    // Data Reader configuration
    public CacheWriterObjectConfig WorkersConfig { get; set; } = new();
    public CacheWriterObjectConfig FederalTaxProfilesConfig { get; set; } = new();
    public CacheWriterObjectConfig StateTaxProfilesConfig { get; set; } = new();
    public CacheWriterObjectConfig LocalTaxProfilesConfig { get; set; } = new();
    public CacheWriterObjectConfig TimeCardsConfig { get; set; } = new();
    public CacheWriterObjectConfig LaborChargeCodesConfig { get; set; } = new();
}