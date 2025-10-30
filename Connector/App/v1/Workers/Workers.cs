namespace Connector.App.v1.Workers;

using Json.Schema.Generation;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Xchange.Connector.SDK.CacheWriter;

/// <summary>
/// Root response object for ADP Workers API
/// </summary>
public class WorkersResponse
{
    [JsonPropertyName("workers")]
    public List<Worker> Workers { get; set; } = new();
}

/// <summary>
/// Represents a worker in the ADP system
/// </summary>

[Description("Worker object from ADP HR system")]
public class Worker
{
    [JsonPropertyName("associateOID")]
    [Description("Unique identifier for the associate")]
    public string AssociateOID { get; init; } = string.Empty;

    [JsonPropertyName("workerID")]
    public WorkerIDDTO? WorkerID { get; set; }

    [JsonPropertyName("person")]
    public PersonDTO? Person { get; set; }

    [JsonPropertyName("businessCommunication")]
    public BusinessCommunicationDTO? BusinessCommunication { get; set; }

    [JsonPropertyName("workAssignments")]
    public List<WorkAssignmentDTO> WorkAssignments { get; set; } = new();
}

/// <summary>
/// Worker identification details
/// </summary>
public class WorkerIDDTO
{
    [JsonPropertyName("idValue")]
    public string IdValue { get; set; } = string.Empty;

    [JsonPropertyName("schemeCode")]
    public CodeValueDTO? SchemeCode { get; set; }
}

/// <summary>
/// Personal information of the worker
/// </summary>
public class PersonDTO
{
    [JsonPropertyName("legalName")]
    public LegalNameDTO? LegalName { get; set; }

    [JsonPropertyName("preferredName")]
    public PreferredNameDTO? PreferredName { get; set; }

    [JsonPropertyName("birthDate")]
    public string? BirthDate { get; set; }

    [JsonPropertyName("genderCode")]
    public CodeValueDTO? GenderCode { get; set; }

    [JsonPropertyName("maritalStatusCode")]
    public CodeValueDTO? MaritalStatusCode { get; set; }

    [JsonPropertyName("ethnicityCode")]
    public CodeValueDTO? EthnicityCode { get; set; }

    [JsonPropertyName("raceCode")]
    public CodeValueDTO? RaceCode { get; set; }

    [JsonPropertyName("socialInsurancePrograms")]
    public List<SocialInsuranceProgramDTO> SocialInsurancePrograms { get; set; } = new();

    [JsonPropertyName("legalAddress")]
    public AddressDTO? LegalAddress { get; set; }

    [JsonPropertyName("communication")]
    public CommunicationDTO? Communication { get; set; }
}

/// <summary>
/// Legal name of the person
/// </summary>
public class LegalNameDTO
{
    [JsonPropertyName("givenName")]
    public string GivenName { get; set; } = string.Empty;

    [JsonPropertyName("middleName")]
    public string? MiddleName { get; set; }

    [JsonPropertyName("familyName1")]
    public string FamilyName1 { get; set; } = string.Empty;

    [JsonPropertyName("familyName2")]
    public string? FamilyName2 { get; set; }

    [JsonPropertyName("formattedName")]
    public string? FormattedName { get; set; }

    [JsonPropertyName("nameCode")]
    public CodeValueDTO? NameCode { get; set; }

    [JsonPropertyName("preferredSalutations")]
    public List<PreferredSalutationDTO> PreferredSalutations { get; set; } = new();
}

/// <summary>
/// Preferred name of the person
/// </summary>
public class PreferredNameDTO
{
    [JsonPropertyName("givenName")]
    public string? GivenName { get; set; }

    [JsonPropertyName("middleName")]
    public string? MiddleName { get; set; }

    [JsonPropertyName("familyName1")]
    public string? FamilyName1 { get; set; }

    [JsonPropertyName("formattedName")]
    public string? FormattedName { get; set; }

    [JsonPropertyName("nameCode")]
    public CodeValueDTO? NameCode { get; set; }
}

/// <summary>
/// Preferred salutation information
/// </summary>
public class PreferredSalutationDTO
{
    [JsonPropertyName("salutationCode")]
    public CodeValueDTO? SalutationCode { get; set; }

    [JsonPropertyName("typeCode")]
    public CodeValueDTO? TypeCode { get; set; }

    [JsonPropertyName("sequenceNumber")]
    public int? SequenceNumber { get; set; }
}

/// <summary>
/// Social insurance program information
/// </summary>
public class SocialInsuranceProgramDTO
{
    [JsonPropertyName("nameCode")]
    public CodeValueDTO? NameCode { get; set; }

    [JsonPropertyName("coveredIndicator")]
    public bool? CoveredIndicator { get; set; }

    [JsonPropertyName("idValue")]
    public string? IdValue { get; set; }
}

/// <summary>
/// Address information
/// </summary>
public class AddressDTO
{
    [JsonPropertyName("lineOne")]
    public string? LineOne { get; set; }

    [JsonPropertyName("lineTwo")]
    public string? LineTwo { get; set; }

    [JsonPropertyName("lineThree")]
    public string? LineThree { get; set; }

    [JsonPropertyName("cityName")]
    public string? CityName { get; set; }

    [JsonPropertyName("countrySubdivisionLevel1")]
    public CodeValueDTO? CountrySubdivisionLevel1 { get; set; }

    [JsonPropertyName("countryCode")]
    public string? CountryCode { get; set; }

    [JsonPropertyName("postalCode")]
    public string? PostalCode { get; set; }
}

/// <summary>
/// Personal communication information
/// </summary>
public class CommunicationDTO
{
    [JsonPropertyName("landlines")]
    public List<PhoneNumberDTO> Landlines { get; set; } = new();

    [JsonPropertyName("mobiles")]
    public List<PhoneNumberDTO> Mobiles { get; set; } = new();

    [JsonPropertyName("emails")]
    public List<EmailDTO> Emails { get; set; } = new();
}

/// <summary>
/// Phone number information
/// </summary>
public class PhoneNumberDTO
{
    [JsonPropertyName("nameCode")]
    public CodeValueDTO? NameCode { get; set; }

    [JsonPropertyName("countryDialing")]
    public string? CountryDialing { get; set; }

    [JsonPropertyName("areaDialing")]
    public string? AreaDialing { get; set; }

    [JsonPropertyName("dialNumber")]
    public string? DialNumber { get; set; }

    [JsonPropertyName("formattedNumber")]
    public string? FormattedNumber { get; set; }
}

/// <summary>
/// Email information
/// </summary>
public class EmailDTO
{
    [JsonPropertyName("nameCode")]
    public CodeValueDTO? NameCode { get; set; }

    [JsonPropertyName("emailUri")]
    public string? EmailUri { get; set; }
}

/// <summary>
/// Business communication information
/// </summary>
public class BusinessCommunicationDTO
{
    [JsonPropertyName("landlines")]
    public List<PhoneNumberDTO> Landlines { get; set; } = new();

    [JsonPropertyName("mobiles")]
    public List<PhoneNumberDTO> Mobiles { get; set; } = new();

    [JsonPropertyName("emails")]
    public List<EmailDTO> Emails { get; set; } = new();
}

/// <summary>
/// Work assignment information
/// </summary>
public class WorkAssignmentDTO
{
    [JsonPropertyName("itemID")]
    public string? ItemID { get; set; }

    [JsonPropertyName("primaryIndicator")]
    public bool? PrimaryIndicator { get; set; }

    [JsonPropertyName("hireDate")]
    public string? HireDate { get; set; }

    [JsonPropertyName("seniorityDate")]
    public string? SeniorityDate { get; set; }

    [JsonPropertyName("workerTypeCode")]
    public CodeValueDTO? WorkerTypeCode { get; set; }

    [JsonPropertyName("workerStatus")]
    public WorkerStatusDTO? WorkerStatus { get; set; }

    [JsonPropertyName("assignmentStatus")]
    public AssignmentStatusDTO? AssignmentStatus { get; set; }

    [JsonPropertyName("homeOrganizationalUnits")]
    public List<OrganizationalUnitDTO> HomeOrganizationalUnits { get; set; } = new();

    [JsonPropertyName("assignedOrganizationalUnits")]
    public List<OrganizationalUnitDTO> AssignedOrganizationalUnits { get; set; } = new();

    [JsonPropertyName("jobTitle")]
    public string? JobTitle { get; set; }

    [JsonPropertyName("jobCode")]
    public CodeValueDTO? JobCode { get; set; }

    [JsonPropertyName("occupationalClassifications")]
    public List<OccupationalClassificationDTO> OccupationalClassifications { get; set; } = new();

    [JsonPropertyName("wageLawCoverage")]
    public WageLawCoverageDTO? WageLawCoverage { get; set; }

    [JsonPropertyName("workLevelCode")]
    public CodeValueDTO? WorkLevelCode { get; set; }

    [JsonPropertyName("baseRemuneration")]
    public BaseRemunerationDTO? BaseRemuneration { get; set; }

    [JsonPropertyName("additionalRemunerations")]
    public List<AdditionalRemunerationDTO> AdditionalRemunerations { get; set; } = new();

    [JsonPropertyName("standardHours")]
    public StandardHoursDTO? StandardHours { get; set; }

    [JsonPropertyName("fullTimeEquivalenceRatio")]
    public decimal? FullTimeEquivalenceRatio { get; set; }

    [JsonPropertyName("homeWorkLocation")]
    public WorkLocationDTO? HomeWorkLocation { get; set; }

    [JsonPropertyName("assignedWorkLocations")]
    public List<WorkLocationDTO> AssignedWorkLocations { get; set; } = new();

    [JsonPropertyName("reportsTo")]
    public List<ReportsToDTO> ReportsTo { get; set; } = new();

    [JsonPropertyName("managementPositionIndicator")]
    public bool? ManagementPositionIndicator { get; set; }

    [JsonPropertyName("payCycleCode")]
    public CodeValueDTO? PayCycleCode { get; set; }

    [JsonPropertyName("payGradeCode")]
    public CodeValueDTO? PayGradeCode { get; set; }

    [JsonPropertyName("terminationDate")]
    public string? TerminationDate { get; set; }

    [JsonPropertyName("rehireEligibleIndicator")]
    public bool? RehireEligibleIndicator { get; set; }
}

/// <summary>
/// Worker status information
/// </summary>
public class WorkerStatusDTO
{
    [JsonPropertyName("statusCode")]
    public CodeValueDTO? StatusCode { get; set; }

    [JsonPropertyName("effectiveDate")]
    public string? EffectiveDate { get; set; }
}

/// <summary>
/// Assignment status information
/// </summary>
public class AssignmentStatusDTO
{
    [JsonPropertyName("statusCode")]
    public CodeValueDTO? StatusCode { get; set; }

    [JsonPropertyName("effectiveDate")]
    public string? EffectiveDate { get; set; }

    [JsonPropertyName("reasonCode")]
    public CodeValueDTO? ReasonCode { get; set; }
}

/// <summary>
/// Organizational unit information
/// </summary>
public class OrganizationalUnitDTO
{
    [JsonPropertyName("nameCode")]
    public CodeValueDTO? NameCode { get; set; }

    [JsonPropertyName("typeCode")]
    public CodeValueDTO? TypeCode { get; set; }

    [JsonPropertyName("itemID")]
    public string? ItemID { get; set; }
}

/// <summary>
/// Occupational classification information
/// </summary>
public class OccupationalClassificationDTO
{
    [JsonPropertyName("nameCode")]
    public CodeValueDTO? NameCode { get; set; }

    [JsonPropertyName("classificationCode")]
    public CodeValueDTO? ClassificationCode { get; set; }
}

/// <summary>
/// Wage law coverage information
/// </summary>
public class WageLawCoverageDTO
{
    [JsonPropertyName("wageLawNameCode")]
    public CodeValueDTO? WageLawNameCode { get; set; }

    [JsonPropertyName("coverageCode")]
    public CodeValueDTO? CoverageCode { get; set; }
}

/// <summary>
/// Base remuneration information
/// </summary>
public class BaseRemunerationDTO
{
    [JsonPropertyName("effectiveDate")]
    public string? EffectiveDate { get; set; }

    [JsonPropertyName("payPeriodRateAmount")]
    public AmountDTO? PayPeriodRateAmount { get; set; }

    [JsonPropertyName("annualRateAmount")]
    public AmountDTO? AnnualRateAmount { get; set; }

    [JsonPropertyName("hourlyRateAmount")]
    public AmountDTO? HourlyRateAmount { get; set; }

    [JsonPropertyName("dailyRateAmount")]
    public AmountDTO? DailyRateAmount { get; set; }

    [JsonPropertyName("weeklyRateAmount")]
    public AmountDTO? WeeklyRateAmount { get; set; }

    [JsonPropertyName("monthlyRateAmount")]
    public AmountDTO? MonthlyRateAmount { get; set; }
}

/// <summary>
/// Additional remuneration information
/// </summary>
public class AdditionalRemunerationDTO
{
    [JsonPropertyName("nameCode")]
    public CodeValueDTO? NameCode { get; set; }

    [JsonPropertyName("effectiveDate")]
    public string? EffectiveDate { get; set; }

    [JsonPropertyName("rate")]
    public AmountDTO? Rate { get; set; }
}

/// <summary>
/// Amount with currency information
/// </summary>
public class AmountDTO
{
    [JsonPropertyName("nameCode")]
    public CodeValueDTO? NameCode { get; set; }

    [JsonPropertyName("amountValue")]
    public decimal? AmountValue { get; set; }

    [JsonPropertyName("currencyCode")]
    public string? CurrencyCode { get; set; }
}

/// <summary>
/// Standard hours information
/// </summary>
public class StandardHoursDTO
{
    [JsonPropertyName("hoursQuantity")]
    public decimal? HoursQuantity { get; set; }

    [JsonPropertyName("unitCode")]
    public CodeValueDTO? UnitCode { get; set; }
}

/// <summary>
/// Work location information
/// </summary>
public class WorkLocationDTO
{
    [JsonPropertyName("nameCode")]
    public CodeValueDTO? NameCode { get; set; }

    [JsonPropertyName("address")]
    public AddressDTO? Address { get; set; }
}

/// <summary>
/// Reports to relationship information
/// </summary>
public class ReportsToDTO
{
    [JsonPropertyName("associateOID")]
    public string? AssociateOID { get; set; }

    [JsonPropertyName("workerID")]
    public WorkerIDDTO? WorkerID { get; set; }

    [JsonPropertyName("positionID")]
    public string? PositionID { get; set; }

    [JsonPropertyName("positionTitle")]
    public string? PositionTitle { get; set; }

    [JsonPropertyName("reportsToRelationshipCode")]
    public CodeValueDTO? ReportsToRelationshipCode { get; set; }

    [JsonPropertyName("reportsToWorkerName")]
    public ReportsToWorkerNameDTO? ReportsToWorkerName { get; set; }
}

/// <summary>
/// Reports to worker name information
/// </summary>
public class ReportsToWorkerNameDTO
{
    [JsonPropertyName("formattedName")]
    public string? FormattedName { get; set; }

    [JsonPropertyName("givenName")]
    public string? GivenName { get; set; }

    [JsonPropertyName("middleName")]
    public string? MiddleName { get; set; }

    [JsonPropertyName("familyName1")]
    public string? FamilyName1 { get; set; }
}

/// <summary>
/// Generic code value structure used throughout ADP API
/// </summary>
public class CodeValueDTO
{
    [JsonPropertyName("codeValue")]
    public string? CodeValueString { get; set; }

    [JsonPropertyName("shortName")]
    public string? ShortName { get; set; }

    [JsonPropertyName("longName")]
    public string? LongName { get; set; }
}
