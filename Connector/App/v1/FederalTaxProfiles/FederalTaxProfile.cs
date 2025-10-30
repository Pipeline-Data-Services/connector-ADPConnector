namespace Connector.App.v1.FederalTaxProfiles;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Root response object for ADP US Tax Profiles API
/// Based on: GET /payroll/v1/workers/{aoid}/us-tax-profiles
/// ADP returns a single tax profile object, not an array
/// </summary>
public class USTaxProfilesResponse
{
    [JsonPropertyName("usTaxProfiles")]
    public USTaxProfile? UsTaxProfiles { get; set; }
}

/// <summary>
/// US Tax Profile for a worker
/// Contains federal, state, and local tax withholding information
/// </summary>
public class USTaxProfile
{
    [JsonPropertyName("itemID")]
    public string? ItemID { get; set; }

    [JsonPropertyName("payrollFileNumber")]
    public string? PayrollFileNumber { get; set; }

    [JsonPropertyName("payrollGroupCode")]
    public TaxCodeValueDTO? PayrollGroupCode { get; set; }

    [JsonPropertyName("usFederalTaxInstruction")]
    public USFederalTaxInstructionDTO? USFederalTaxInstruction { get; set; }

    [JsonPropertyName("usStateTaxInstructions")]
    public List<USStateTaxInstructionDTO> USStateTaxInstructions { get; set; } = new();

    [JsonPropertyName("usLocalTaxInstructions")]
    public List<USLocalTaxInstructionDTO> USLocalTaxInstructions { get; set; } = new();
}

/// <summary>
/// US Federal Tax Instruction
/// </summary>
public class USFederalTaxInstructionDTO
{
    [JsonPropertyName("federalIncomeTaxInstruction")]
    public FederalIncomeTaxInstructionDTO? FederalIncomeTaxInstruction { get; set; }

    [JsonPropertyName("socialSecurityTaxInstruction")]
    public TaxInstructionDTO? SocialSecurityTaxInstruction { get; set; }

    [JsonPropertyName("medicareTaxInstruction")]
    public TaxInstructionDTO? MedicareTaxInstruction { get; set; }

    [JsonPropertyName("federalUnemploymentTaxInstruction")]
    public TaxInstructionDTO? FederalUnemploymentTaxInstruction { get; set; }

    [JsonPropertyName("form1099Instruction")]
    public Form1099InstructionDTO? Form1099Instruction { get; set; }

    [JsonPropertyName("interimW2IssuedIndicator")]
    public bool? InterimW2IssuedIndicator { get; set; }

    [JsonPropertyName("statutoryWorkerIndicator")]
    public bool? StatutoryWorkerIndicator { get; set; }

    [JsonPropertyName("qualifiedPensionPlanCoverageIndicator")]
    public bool? QualifiedPensionPlanCoverageIndicator { get; set; }

    [JsonPropertyName("multipleJobIndicator")]
    public bool? MultipleJobIndicator { get; set; }
}

/// <summary>
/// Federal Income Tax Instruction
/// </summary>
public class FederalIncomeTaxInstructionDTO
{
    [JsonPropertyName("taxWithholdingStatus")]
    public TaxWithholdingStatusDTO? TaxWithholdingStatus { get; set; }

    [JsonPropertyName("taxFilingStatusCode")]
    public TaxCodeValueDTO? TaxFilingStatusCode { get; set; }

    [JsonPropertyName("taxWithholdingAllowanceQuantity")]
    public decimal? TaxWithholdingAllowanceQuantity { get; set; }

    [JsonPropertyName("additionalTaxPercentage")]
    public decimal? AdditionalTaxPercentage { get; set; }

    [JsonPropertyName("additionalTaxAmount")]
    public TaxAmountDTO? AdditionalTaxAmount { get; set; }

    [JsonPropertyName("overrideTaxPercentage")]
    public decimal? OverrideTaxPercentage { get; set; }

    [JsonPropertyName("overrideTaxAmount")]
    public TaxAmountDTO? OverrideTaxAmount { get; set; }

    [JsonPropertyName("taxAllowances")]
    public List<TaxAllowanceDTO> TaxAllowances { get; set; } = new();

    [JsonPropertyName("additionalIncomeAmount")]
    public TaxAmountDTO? AdditionalIncomeAmount { get; set; }
}

/// <summary>
/// Tax Instruction (for Social Security, Medicare, etc.)
/// </summary>
public class TaxInstructionDTO
{
    [JsonPropertyName("taxWithholdingStatus")]
    public TaxWithholdingStatusDTO? TaxWithholdingStatus { get; set; }
}

/// <summary>
/// Form 1099 Instruction
/// </summary>
public class Form1099InstructionDTO
{
    [JsonPropertyName("distributionCodes")]
    public List<TaxCodeValueDTO> DistributionCodes { get; set; } = new();

    [JsonPropertyName("totalDistributionIndicator")]
    public bool? TotalDistributionIndicator { get; set; }

    [JsonPropertyName("individualRetirementAccountIndicator")]
    public bool? IndividualRetirementAccountIndicator { get; set; }

    [JsonPropertyName("simplifiedEmployeePensionAccountIndicator")]
    public bool? SimplifiedEmployeePensionAccountIndicator { get; set; }
}

/// <summary>
/// Tax Allowance
/// </summary>
public class TaxAllowanceDTO
{
    [JsonPropertyName("allowanceCode")]
    public TaxCodeValueDTO? AllowanceCode { get; set; }

    [JsonPropertyName("allowanceQuantity")]
    public decimal? AllowanceQuantity { get; set; }
}

/// <summary>
/// US State Tax Instruction
/// </summary>
public class USStateTaxInstructionDTO
{
    [JsonPropertyName("itemID")]
    public string? ItemID { get; set; }

    [JsonPropertyName("taxWithholdingStatus")]
    public TaxWithholdingStatusDTO? TaxWithholdingStatus { get; set; }

    [JsonPropertyName("stateIncomeTaxInstruction")]
    public StateIncomeTaxInstructionDTO? StateIncomeTaxInstruction { get; set; }

    [JsonPropertyName("stateDisabilityInsuranceTaxInstruction")]
    public TaxInstructionDTO? StateDisabilityInsuranceTaxInstruction { get; set; }

    [JsonPropertyName("stateUnemploymentTaxInstruction")]
    public TaxInstructionDTO? StateUnemploymentTaxInstruction { get; set; }
}

/// <summary>
/// State Income Tax Instruction
/// </summary>
public class StateIncomeTaxInstructionDTO
{
    [JsonPropertyName("taxWithholdingStatus")]
    public TaxWithholdingStatusDTO? TaxWithholdingStatus { get; set; }

    [JsonPropertyName("stateCode")]
    public TaxCodeValueDTO? StateCode { get; set; }

    [JsonPropertyName("taxFilingStatusCode")]
    public TaxCodeValueDTO? TaxFilingStatusCode { get; set; }

    [JsonPropertyName("taxWithholdingAllowanceQuantity")]
    public decimal? TaxWithholdingAllowanceQuantity { get; set; }

    [JsonPropertyName("additionalTaxAmount")]
    public TaxAmountDTO? AdditionalTaxAmount { get; set; }

    [JsonPropertyName("additionalTaxPercentage")]
    public decimal? AdditionalTaxPercentage { get; set; }

    [JsonPropertyName("residencyStatusCode")]
    public TaxCodeValueDTO? ResidencyStatusCode { get; set; }
}

/// <summary>
/// US Local Tax Instruction
/// </summary>
public class USLocalTaxInstructionDTO
{
    [JsonPropertyName("itemID")]
    public string? ItemID { get; set; }

    [JsonPropertyName("taxWithholdingStatus")]
    public TaxWithholdingStatusDTO? TaxWithholdingStatus { get; set; }

    [JsonPropertyName("localIncomeTaxInstruction")]
    public LocalIncomeTaxInstructionDTO? LocalIncomeTaxInstruction { get; set; }
}

/// <summary>
/// Local Income Tax Instruction
/// </summary>
public class LocalIncomeTaxInstructionDTO
{
    [JsonPropertyName("taxWithholdingStatus")]
    public TaxWithholdingStatusDTO? TaxWithholdingStatus { get; set; }

    [JsonPropertyName("localityCode")]
    public TaxCodeValueDTO? LocalityCode { get; set; }

    [JsonPropertyName("taxFilingStatusCode")]
    public TaxCodeValueDTO? TaxFilingStatusCode { get; set; }

    [JsonPropertyName("taxWithholdingAllowanceQuantity")]
    public decimal? TaxWithholdingAllowanceQuantity { get; set; }

    [JsonPropertyName("additionalTaxAmount")]
    public TaxAmountDTO? AdditionalTaxAmount { get; set; }

    [JsonPropertyName("residencyStatusCode")]
    public TaxCodeValueDTO? ResidencyStatusCode { get; set; }
}

/// <summary>
/// Tax withholding status information
/// </summary>
public class TaxWithholdingStatusDTO
{
    [JsonPropertyName("statusCode")]
    public TaxCodeValueDTO? StatusCode { get; set; }

    [JsonPropertyName("reasonCode")]
    public TaxCodeValueDTO? ReasonCode { get; set; }

    [JsonPropertyName("effectiveDate")]
    public string? EffectiveDate { get; set; }
}

/// <summary>
/// Tax amount with currency information
/// </summary>
public class TaxAmountDTO
{
    [JsonPropertyName("nameCode")]
    public TaxCodeValueDTO? NameCode { get; set; }

    [JsonPropertyName("amountValue")]
    public decimal? AmountValue { get; set; }

    [JsonPropertyName("currencyCode")]
    public string? CurrencyCode { get; set; }
}

/// <summary>
/// Generic code value structure used in tax profiles
/// </summary>
public class TaxCodeValueDTO
{
    [JsonPropertyName("codeValue")]
    public string? CodeValueString { get; set; }

    [JsonPropertyName("shortName")]
    public string? ShortName { get; set; }

    [JsonPropertyName("longName")]
    public string? LongName { get; set; }
}
