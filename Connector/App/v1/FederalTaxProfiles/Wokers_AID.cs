namespace Connector.App.v1.Workers;

using Json.Schema.Generation;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Xchange.Connector.SDK.CacheWriter;

/// <summary>
/// Data object that will represent an object in the Xchange system. This will be converted to a JsonSchema, 
/// so add attributes to the properties to provide any descriptions, titles, ranges, max, min, etc... 
/// These types will be used for validation at runtime to make sure the objects being passed through the system 
/// are properly formed. The schema also helps provide integrators more information for what the values 
/// are intended to be.
/// </summary>
[PrimaryKey("AssociateOID", nameof(AssociateOID))]
[Description("Worker object for Xchange system")]
public class Workers_AID
{
    [Description("Unique identifier for the associate")]
    [Required]
    public string AssociateOID { get; init; } = string.Empty;
}
