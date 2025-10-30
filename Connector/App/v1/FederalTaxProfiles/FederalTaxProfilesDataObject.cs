namespace Connector.App.v1.FederalTaxProfiles;

using Json.Schema.Generation;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Xchange.Connector.SDK.CacheWriter;

/// <summary>
/// Data object that represents Federal Tax Profile in the Xchange system. 
/// This will be converted to a JsonSchema for validation and integration purposes.
/// Based on ADP Payroll US Tax Profiles API
/// </summary>
[PrimaryKey("ProfileId", nameof(ProfileId))]
[Description("Federal Tax Profile object for worker tax withholding information")]
public class FederalTaxProfilesDataObject
{
    [Description("Unique identifier for the tax profile")]
    [Required]
    public string ProfileId { get; init; } = string.Empty;

    [Description("Associate OID - unique identifier for the worker")]
    [Required]
    public string AssociateOID { get; init; } = string.Empty;

    [AllowNull]
    public string PayrollFileNumber { get; set; } = string.Empty;

    [AllowNull]
    public TaxCodeValue PayrollGroupCode { get; set; } = new();

    [AllowNull]
    public USFederalTaxInstruction USFederalTaxInstruction { get; set; } = new();

    public List<USStateTaxInstruction> USStateTaxInstructions { get; set; } = new();

    public List<USLocalTaxInstruction> USLocalTaxInstructions { get; set; } = new();
}

/// <summary>
/// US Federal Tax Instruction
/// </summary>
public class USFederalTaxInstruction
{
    [AllowNull]
    public FederalIncomeTaxInstruction FederalIncomeTaxInstruction { get; set; } = new();

    [AllowNull]
    public TaxInstruction SocialSecurityTaxInstruction { get; set; } = new();

    [AllowNull]
    public TaxInstruction MedicareTaxInstruction { get; set; } = new();

    [AllowNull]
    public TaxInstruction FederalUnemploymentTaxInstruction { get; set; } = new();

    [AllowNull]
    public Form1099Instruction Form1099Instruction { get; set; } = new();

    [AllowNull]
    public bool InterimW2IssuedIndicator { get; set; } = false;

    [AllowNull]
    public bool StatutoryWorkerIndicator { get; set; } = false;

    [AllowNull]
    public bool QualifiedPensionPlanCoverageIndicator { get; set; } = false;

    [AllowNull]
    public bool MultipleJobIndicator { get; set; } = false;
}

/// <summary>
/// Federal Income Tax Instruction
/// </summary>
public class FederalIncomeTaxInstruction
{
    [AllowNull]
    public TaxWithholdingStatus TaxWithholdingStatus { get; set; } = new();

    [AllowNull]
    public TaxCodeValue TaxFilingStatusCode { get; set; } = new();

    [AllowNull]
    public decimal TaxWithholdingAllowanceQuantity { get; set; } = 0m;

    [AllowNull]
    public decimal AdditionalTaxPercentage { get; set; } = 0m;

    [AllowNull]
    public TaxAmount AdditionalTaxAmount { get; set; } = new();

    [AllowNull]
    public decimal OverrideTaxPercentage { get; set; } = 0m;

    [AllowNull]
    public TaxAmount OverrideTaxAmount { get; set; } = new();

    public List<TaxAllowance> TaxAllowances { get; set; } = new();

    [AllowNull]
    public TaxAmount AdditionalIncomeAmount { get; set; } = new();
}

/// <summary>
/// Tax Instruction (for Social Security, Medicare, etc.)
/// </summary>
public class TaxInstruction
{
    [AllowNull]
    public TaxWithholdingStatus TaxWithholdingStatus { get; set; } = new();
}

/// <summary>
/// Form 1099 Instruction
/// </summary>
public class Form1099Instruction
{
    public List<TaxCodeValue> DistributionCodes { get; set; } = new();

    [AllowNull]
    public bool TotalDistributionIndicator { get; set; } = false;

    [AllowNull]
    public bool IndividualRetirementAccountIndicator { get; set; } = false;

    [AllowNull]
    public bool SimplifiedEmployeePensionAccountIndicator { get; set; } = false;
}

/// <summary>
/// Tax Allowance
/// </summary>
public class TaxAllowance
{
    [AllowNull]
    public TaxCodeValue AllowanceCode { get; set; } = new();

    [AllowNull]
    public decimal AllowanceQuantity { get; set; } = 0m;
}

/// <summary>
/// US State Tax Instruction
/// </summary>
public class USStateTaxInstruction
{
    [AllowNull]
    public string ItemID { get; set; } = string.Empty;

    [AllowNull]
    public TaxWithholdingStatus TaxWithholdingStatus { get; set; } = new();

    [AllowNull]
    public StateIncomeTaxInstruction StateIncomeTaxInstruction { get; set; } = new();

    [AllowNull]
    public TaxInstruction StateDisabilityInsuranceTaxInstruction { get; set; } = new();

    [AllowNull]
    public TaxInstruction StateUnemploymentTaxInstruction { get; set; } = new();
}

/// <summary>
/// State Income Tax Instruction
/// </summary>
public class StateIncomeTaxInstruction
{
    [AllowNull]
    public TaxWithholdingStatus TaxWithholdingStatus { get; set; } = new();

    [AllowNull]
    public TaxCodeValue StateCode { get; set; } = new();

    [AllowNull]
    public TaxCodeValue TaxFilingStatusCode { get; set; } = new();

    [AllowNull]
    public decimal TaxWithholdingAllowanceQuantity { get; set; } = 0m;

    [AllowNull]
    public TaxAmount AdditionalTaxAmount { get; set; } = new();

    [AllowNull]
    public decimal AdditionalTaxPercentage { get; set; } = 0m;

    [AllowNull]
    public TaxCodeValue ResidencyStatusCode { get; set; } = new();
}

/// <summary>
/// US Local Tax Instruction
/// </summary>
public class USLocalTaxInstruction
{
    [AllowNull]
    public string ItemID { get; set; } = string.Empty;

    [AllowNull]
    public TaxWithholdingStatus TaxWithholdingStatus { get; set; } = new();

    [AllowNull]
    public LocalIncomeTaxInstruction LocalIncomeTaxInstruction { get; set; } = new();
}

/// <summary>
/// Local Income Tax Instruction
/// </summary>
public class LocalIncomeTaxInstruction
{
    [AllowNull]
    public TaxWithholdingStatus TaxWithholdingStatus { get; set; } = new();

    [AllowNull]
    public TaxCodeValue LocalityCode { get; set; } = new();

    [AllowNull]
    public TaxCodeValue TaxFilingStatusCode { get; set; } = new();

    [AllowNull]
    public decimal TaxWithholdingAllowanceQuantity { get; set; } = 0m;

    [AllowNull]
    public TaxAmount AdditionalTaxAmount { get; set; } = new();

    [AllowNull]
    public TaxCodeValue ResidencyStatusCode { get; set; } = new();
}

/// <summary>
/// Tax withholding status information
/// </summary>
public class TaxWithholdingStatus
{
    [AllowNull]
    public TaxCodeValue StatusCode { get; set; } = new();

    [AllowNull]
    public TaxCodeValue ReasonCode { get; set; } = new();

    [AllowNull]
    public DateTime EffectiveDate { get; set; } = DateTime.MinValue;
}

/// <summary>
/// Tax amount with currency information
/// </summary>
public class TaxAmount
{
    [AllowNull]
    public TaxCodeValue NameCode { get; set; } = new();

    [AllowNull]
    public decimal AmountValue { get; set; } = 0m;

    [AllowNull]
    public string CurrencyCode { get; set; } = string.Empty;
}

/// <summary>
/// Generic code value structure used in tax profiles
/// </summary>
public class TaxCodeValue
{
    [AllowNull]
    public string CodeValueString { get; set; } = string.Empty;

    [AllowNull]
    public string ShortName { get; set; } = string.Empty;

    [AllowNull]
    public string LongName { get; set; } = string.Empty;
}