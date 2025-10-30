namespace Connector.App.v1.StateTaxProfiles;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Root response object for ADP US State Tax Profiles API
/// Based on: GET /payroll/v1/workers/{aoid}/us-tax-profiles/{us-tax-profile-id}/state
/// </summary>
public class USStateTaxProfilesResponse
{
    [JsonPropertyName("stateTaxWithholdings")]
    public List<StateTaxWithholdingWrapper> StateTaxWithholdings { get; set; } = new();
}

/// <summary>
/// Wrapper for state tax withholding in ADP response
/// </summary>
public class StateTaxWithholdingWrapper
{
    [JsonPropertyName("stateTaxWithholding")]
    public StateTaxWithholding? StateTaxWithholding { get; set; }
}

/// <summary>
/// State Tax Withholding for a worker
/// Contains state-specific tax withholding information
/// </summary>
public class StateTaxWithholding
{
    [JsonPropertyName("itemID")]
    public string? ItemID { get; set; }

    [JsonPropertyName("stateIncomeTaxInstruction")]
    public StateIncomeTaxInstructionDTO? StateIncomeTaxInstruction { get; set; }

    [JsonPropertyName("stateDisabilityInsuranceTaxInstruction")]
    public StateTaxInstructionDTO? StateDisabilityInsuranceTaxInstruction { get; set; }

    [JsonPropertyName("stateUnemploymentInsuranceTaxInstruction")]
    public StateTaxInstructionDTO? StateUnemploymentInsuranceTaxInstruction { get; set; }

    [JsonPropertyName("residencyStatusCode")]
    public StateTaxCodeValueDTO? ResidencyStatusCode { get; set; }
}

/// <summary>
/// State Income Tax Instruction
/// </summary>
public class StateIncomeTaxInstructionDTO
{
    [JsonPropertyName("taxWithholdingStatus")]
    public StateTaxWithholdingStatusDTO? TaxWithholdingStatus { get; set; }

    [JsonPropertyName("stateCode")]
    public StateTaxCodeValueDTO? StateCode { get; set; }

    [JsonPropertyName("taxFilingStatusCode")]
    public StateTaxCodeValueDTO? TaxFilingStatusCode { get; set; }

    [JsonPropertyName("taxWithholdingAllowanceQuantity")]
    public decimal? TaxWithholdingAllowanceQuantity { get; set; }

    [JsonPropertyName("dependentsQuantity")]
    public int? DependentsQuantity { get; set; }

    [JsonPropertyName("exemptionsQuantity")]
    public int? ExemptionsQuantity { get; set; }

    [JsonPropertyName("personalExemptionsQuantity")]
    public int? PersonalExemptionsQuantity { get; set; }

    [JsonPropertyName("dependentExemptionsQuantity")]
    public int? DependentExemptionsQuantity { get; set; }

    [JsonPropertyName("additionalTaxAmount")]
    public StateTaxAmountDTO? AdditionalTaxAmount { get; set; }

    [JsonPropertyName("additionalTaxPercentage")]
    public decimal? AdditionalTaxPercentage { get; set; }

    [JsonPropertyName("overrideTaxAmount")]
    public StateTaxAmountDTO? OverrideTaxAmount { get; set; }

    [JsonPropertyName("overrideTaxPercentage")]
    public decimal? OverrideTaxPercentage { get; set; }

    [JsonPropertyName("estimatedDeductionAmount")]
    public StateTaxAmountDTO? EstimatedDeductionAmount { get; set; }

    [JsonPropertyName("taxAllowances")]
    public List<StateTaxAllowanceDTO> TaxAllowances { get; set; } = new();

    [JsonPropertyName("reciprocityLocationCode")]
    public StateTaxCodeValueDTO? ReciprocityLocationCode { get; set; }

    [JsonPropertyName("stateTaxLiabilityCode")]
    public StateTaxCodeValueDTO? StateTaxLiabilityCode { get; set; }

    [JsonPropertyName("headOfHouseholdIndicator")]
    public bool? HeadOfHouseholdIndicator { get; set; }

    [JsonPropertyName("blindIndicator")]
    public bool? BlindIndicator { get; set; }

    [JsonPropertyName("ageIndicator")]
    public bool? AgeIndicator { get; set; }

    [JsonPropertyName("spouseEmploymentIndicator")]
    public bool? SpouseEmploymentIndicator { get; set; }
}

/// <summary>
/// State Tax Instruction (for disability insurance, unemployment, etc.)
/// </summary>
public class StateTaxInstructionDTO
{
    [JsonPropertyName("taxWithholdingStatus")]
    public StateTaxWithholdingStatusDTO? TaxWithholdingStatus { get; set; }

    [JsonPropertyName("stateCode")]
    public StateTaxCodeValueDTO? StateCode { get; set; }
}

/// <summary>
/// State Tax Withholding Status
/// </summary>
public class StateTaxWithholdingStatusDTO
{
    [JsonPropertyName("statusCode")]
    public StateTaxCodeValueDTO? StatusCode { get; set; }

    [JsonPropertyName("reasonCode")]
    public StateTaxCodeValueDTO? ReasonCode { get; set; }

    [JsonPropertyName("effectiveDate")]
    public string? EffectiveDate { get; set; }
}

/// <summary>
/// State Tax Allowance
/// </summary>
public class StateTaxAllowanceDTO
{
    [JsonPropertyName("allowanceCode")]
    public StateTaxCodeValueDTO? AllowanceCode { get; set; }

    [JsonPropertyName("allowanceQuantity")]
    public decimal? AllowanceQuantity { get; set; }
}

/// <summary>
/// State Tax Amount with currency information
/// </summary>
public class StateTaxAmountDTO
{
    [JsonPropertyName("nameCode")]
    public StateTaxCodeValueDTO? NameCode { get; set; }

    [JsonPropertyName("amountValue")]
    public decimal? AmountValue { get; set; }

    [JsonPropertyName("currencyCode")]
    public string? CurrencyCode { get; set; }
}

/// <summary>
/// Generic code value structure used in state tax profiles
/// </summary>
public class StateTaxCodeValueDTO
{
    [JsonPropertyName("codeValue")]
    public string? CodeValueString { get; set; }

    [JsonPropertyName("shortName")]
    public string? ShortName { get; set; }

    [JsonPropertyName("longName")]
    public string? LongName { get; set; }
}
