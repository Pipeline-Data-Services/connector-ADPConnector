namespace Connector.App.v1.StateTaxProfiles;

using Json.Schema.Generation;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Xchange.Connector.SDK.CacheWriter;

/// <summary>
/// Data object that represents State Tax Profile in the Xchange system. 
/// This will be converted to a JsonSchema for validation and integration purposes.
/// Based on ADP Payroll US State Tax Profiles API
/// </summary>
[PrimaryKey("ProfileId", nameof(ProfileId))]
[Description("State Tax Profile object for worker state tax withholding information")]
public class StateTaxProfilesDataObject
{
    [Description("Unique identifier for the state tax profile")]
    [Required]
    public string ProfileId { get; init; } = string.Empty;

    [Description("Associate OID - unique identifier for the worker")]
    [Required]
    public string AssociateOID { get; init; } = string.Empty;

    [Description("Federal Tax Profile ID that this state profile belongs to")]
    [Required]
    public string FederalTaxProfileId { get; init; } = string.Empty;

    [AllowNull]
    public StateIncomeTaxInstruction StateIncomeTaxInstruction { get; set; } = new();

    [AllowNull]
    public StateTaxInstruction StateDisabilityInsuranceTaxInstruction { get; set; } = new();

    [AllowNull]
    public StateTaxInstruction StateUnemploymentInsuranceTaxInstruction { get; set; } = new();

    [AllowNull]
    public StateTaxCodeValue ResidencyStatusCode { get; set; } = new();
}

/// <summary>
/// State Income Tax Instruction
/// </summary>
public class StateIncomeTaxInstruction
{
    [AllowNull]
    public StateTaxWithholdingStatus TaxWithholdingStatus { get; set; } = new();

    [AllowNull]
    public StateTaxCodeValue StateCode { get; set; } = new();

    [AllowNull]
    public StateTaxCodeValue TaxFilingStatusCode { get; set; } = new();

    [AllowNull]
    public decimal TaxWithholdingAllowanceQuantity { get; set; } = 0m;

    [AllowNull]
    public int DependentsQuantity { get; set; } = 0;

    [AllowNull]
    public int ExemptionsQuantity { get; set; } = 0;

    [AllowNull]
    public int PersonalExemptionsQuantity { get; set; } = 0;

    [AllowNull]
    public int DependentExemptionsQuantity { get; set; } = 0;

    [AllowNull]
    public StateTaxAmount AdditionalTaxAmount { get; set; } = new();

    [AllowNull]
    public decimal AdditionalTaxPercentage { get; set; } = 0m;

    [AllowNull]
    public StateTaxAmount OverrideTaxAmount { get; set; } = new();

    [AllowNull]
    public decimal OverrideTaxPercentage { get; set; } = 0m;

    [AllowNull]
    public StateTaxAmount EstimatedDeductionAmount { get; set; } = new();

    public List<StateTaxAllowance> TaxAllowances { get; set; } = new();

    [AllowNull]
    public StateTaxCodeValue ReciprocityLocationCode { get; set; } = new();

    [AllowNull]
    public StateTaxCodeValue StateTaxLiabilityCode { get; set; } = new();

    [AllowNull]
    public bool HeadOfHouseholdIndicator { get; set; } = false;

    [AllowNull]
    public bool BlindIndicator { get; set; } = false;

    [AllowNull]
    public bool AgeIndicator { get; set; } = false;

    [AllowNull]
    public bool SpouseEmploymentIndicator { get; set; } = false;
}

/// <summary>
/// State Tax Instruction (for disability insurance, unemployment, etc.)
/// </summary>
public class StateTaxInstruction
{
    [AllowNull]
    public StateTaxWithholdingStatus TaxWithholdingStatus { get; set; } = new();

    [AllowNull]
    public StateTaxCodeValue StateCode { get; set; } = new();
}

/// <summary>
/// State Tax Withholding Status
/// </summary>
public class StateTaxWithholdingStatus
{
    [AllowNull]
    public StateTaxCodeValue StatusCode { get; set; } = new();

    [AllowNull]
    public StateTaxCodeValue ReasonCode { get; set; } = new();

    [AllowNull]
    public DateTime EffectiveDate { get; set; } = DateTime.MinValue;
}

/// <summary>
/// State Tax Allowance
/// </summary>
public class StateTaxAllowance
{
    [AllowNull]
    public StateTaxCodeValue AllowanceCode { get; set; } = new();

    [AllowNull]
    public decimal AllowanceQuantity { get; set; } = 0m;
}

/// <summary>
/// State Tax Amount with currency information
/// </summary>
public class StateTaxAmount
{
    [AllowNull]
    public StateTaxCodeValue NameCode { get; set; } = new();

    [AllowNull]
    public decimal AmountValue { get; set; } = 0m;

    [AllowNull]
    public string CurrencyCode { get; set; } = string.Empty;
}

/// <summary>
/// Generic code value structure used in state tax profiles
/// </summary>
public class StateTaxCodeValue
{
    [AllowNull]
    public string CodeValueString { get; set; } = string.Empty;

    [AllowNull]
    public string ShortName { get; set; } = string.Empty;

    [AllowNull]
    public string LongName { get; set; } = string.Empty;
}