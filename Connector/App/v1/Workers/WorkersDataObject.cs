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
public class WorkersDataObject
{
    [Description("Unique identifier for the associate")]
    [Required]
    public string AssociateOID { get; init; } = string.Empty;

    [Nullable(true)]
    public WorkerID WorkerID { get; set; } = new();

    [Nullable(true)]
    public Person Person { get; set; } = new();

    [Nullable(true)]
    public BusinessCommunication BusinessCommunication { get; set; } = new();

    public List<WorkAssignment> WorkAssignments { get; set; } = new();
}


/// <summary>
/// Worker identification details
/// </summary>
public class WorkerID
{
    public string IdValue { get; set; } = string.Empty;

    [Nullable(true)]
    public CodeValue SchemeCode { get; set; } = new();
}

/// <summary>
/// Personal information of the worker
/// </summary>
public class Person
{
    [Nullable(true)]
    public LegalName LegalName { get; set; } = new();

    [Nullable(true)]
    public PreferredName PreferredName { get; set; } = new();

    [Nullable(true)]
    public DateTime BirthDate { get; set; } = DateTime.MinValue;

    [Nullable(true)]
    public CodeValue GenderCode { get; set; } = new();

    [Nullable(true)]
    public CodeValue MaritalStatusCode { get; set; } = new();

    [Nullable(true)]
    public CodeValue EthnicityCode { get; set; } = new();

    [Nullable(true)]
    public CodeValue RaceCode { get; set; } = new();

    public List<SocialInsuranceProgram> SocialInsurancePrograms { get; set; } = new();

    [Nullable(true)]
    public Address LegalAddress { get; set; } = new();

    [Nullable(true)]
    public Communication Communication { get; set; } = new();
}

/// <summary>
/// Legal name of the person
/// </summary>
public class LegalName
{
    public string GivenName { get; set; } = string.Empty;

    [Nullable(true)]
    public string MiddleName { get; set; } = string.Empty;

    public string FamilyName1 { get; set; } = string.Empty;

    [Nullable(true)]
    public string FamilyName2 { get; set; } = string.Empty;

    [Nullable(true)]
    public string FormattedName { get; set; } = string.Empty;

    [Nullable(true)]
    public CodeValue NameCode { get; set; } = new();

    public List<PreferredSalutation> PreferredSalutations { get; set; } = new();
}

/// <summary>
/// Preferred name of the person
/// </summary>
public class PreferredName
{
    [Nullable(true)]
    public string GivenName { get; set; } = string.Empty;

    [Nullable(true)]
    public string MiddleName { get; set; } = string.Empty;

    [Nullable(true)]
    public string FamilyName1 { get; set; } = string.Empty;

    [Nullable(true)]
    public string FormattedName { get; set; } = string.Empty;

    [Nullable(true)]
    public CodeValue NameCode { get; set; } = new();
}

/// <summary>
/// Preferred salutation information
/// </summary>
public class PreferredSalutation
{
    [Nullable(true)]
    public CodeValue SalutationCode { get; set; } = new();

    [Nullable(true)]
    public CodeValue TypeCode { get; set; } = new();

    [Nullable(true)]
    public int SequenceNumber { get; set; } = 0;
}

/// <summary>
/// Social insurance program information
/// </summary>
public class SocialInsuranceProgram
{
    [Nullable(true)]
    public CodeValue NameCode { get; set; } = new();

    [Nullable(true)]
    public bool CoveredIndicator { get; set; } = false;

    [Nullable(true)]
    public string IdValue { get; set; } = string.Empty;
}

/// <summary>
/// Address information
/// </summary>
public class Address
{
    [Nullable(true)]
    public string LineOne { get; set; } = string.Empty;

    [Nullable(true)]
    public string LineTwo { get; set; } = string.Empty;

    [Nullable(true)]
    public string LineThree { get; set; } = string.Empty;

    [Nullable(true)]
    public string CityName { get; set; } = string.Empty;

    [Nullable(true)]
    public CodeValue CountrySubdivisionLevel1 { get; set; } = new();

    [Nullable(true)]
    public string CountryCode { get; set; } = string.Empty;

    [Nullable(true)]
    public string PostalCode { get; set; } = string.Empty;
}

/// <summary>
/// Personal communication information
/// </summary>
public class Communication
{
    public List<PhoneNumber> Landlines { get; set; } = new();

    public List<PhoneNumber> Mobiles { get; set; } = new();

    public List<Email> Emails { get; set; } = new();
}

/// <summary>
/// Phone number information
/// </summary>
public class PhoneNumber
{
    [Nullable(true)]
    public CodeValue NameCode { get; set; } = new();

    [Nullable(true)]
    public string CountryDialing { get; set; } = string.Empty;

    [Nullable(true)]
    public string AreaDialing { get; set; } = string.Empty;

    [Nullable(true)]
    public string DialNumber { get; set; } = string.Empty;

    [Nullable(true)]
    public string FormattedNumber { get; set; } = string.Empty;
}

/// <summary>
/// Email information
/// </summary>
public class Email
{
    [Nullable(true)]
    public CodeValue NameCode { get; set; } = new();

    [Nullable(true)]
    public string EmailUri { get; set; } = string.Empty;
}

/// <summary>
/// Business communication information
/// </summary>
public class BusinessCommunication
{
    public List<PhoneNumber> Landlines { get; set; } = new();

    public List<PhoneNumber> Mobiles { get; set; } = new();

    public List<Email> Emails { get; set; } = new();
}

/// <summary>
/// Work assignment information
/// </summary>
public class WorkAssignment
{
    [Nullable(true)]
    public string ItemID { get; set; } = string.Empty;

    [Nullable(true)]
    public bool PrimaryIndicator { get; set; } = false;

    [Nullable(true)]
    public DateTime HireDate { get; set; } = DateTime.MinValue;

    [Nullable(true)]
    public DateTime SeniorityDate { get; set; } = DateTime.MinValue;

    [Nullable(true)]
    public CodeValue WorkerTypeCode { get; set; } = new();

    [Nullable(true)]
    public WorkerStatus WorkerStatus { get; set; } = new();

    [Nullable(true)]
    public AssignmentStatus AssignmentStatus { get; set; } = new();

    public List<OrganizationalUnit> HomeOrganizationalUnits { get; set; } = new();

    public List<OrganizationalUnit> AssignedOrganizationalUnits { get; set; } = new();

    [Nullable(true)]
    public string JobTitle { get; set; } = string.Empty;

    [Nullable(true)]
    public CodeValue JobCode { get; set; } = new();

    public List<OccupationalClassification> OccupationalClassifications { get; set; } = new();

    [Nullable(true)]
    public WageLawCoverage WageLawCoverage { get; set; } = new();

    [Nullable(true)]
    public CodeValue WorkLevelCode { get; set; } = new();

    [Nullable(true)]
    public BaseRemuneration BaseRemuneration { get; set; } = new();

    public List<AdditionalRemuneration> AdditionalRemunerations { get; set; } = new();

    [Nullable(true)]
    public StandardHours StandardHours { get; set; } = new();

    [Nullable(true)]
    public decimal FullTimeEquivalenceRatio { get; set; } = 0m;

    [Nullable(true)]
    public WorkLocation HomeWorkLocation { get; set; } = new();

    public List<WorkLocation> AssignedWorkLocations { get; set; } = new();

    public List<ReportsTo> ReportsTo { get; set; } = new();

    [Nullable(true)]
    public bool ManagementPositionIndicator { get; set; } = false;

    [Nullable(true)]
    public CodeValue PayCycleCode { get; set; } = new();

    [Nullable(true)]
    public CodeValue PayGradeCode { get; set; } = new();

    [Nullable(true)]
    public DateTime TerminationDate { get; set; } = DateTime.MinValue;

    [Nullable(true)]
    public bool RehireEligibleIndicator { get; set; } = false;
}

/// <summary>
/// Worker status information
/// </summary>
public class WorkerStatus
{
    [Nullable(true)]
    public CodeValue StatusCode { get; set; } = new();

    [Nullable(true)]
    public DateTime EffectiveDate { get; set; } = DateTime.MinValue;
}

/// <summary>
/// Assignment status information
/// </summary>
public class AssignmentStatus
{
    [Nullable(true)]
    public CodeValue StatusCode { get; set; } = new();

    [Nullable(true)]
    public DateTime EffectiveDate { get; set; } = DateTime.MinValue;

    [Nullable(true)]
    public CodeValue ReasonCode { get; set; } = new();
}

/// <summary>
/// Organizational unit information
/// </summary>
public class OrganizationalUnit
{
    [Nullable(true)]
    public CodeValue NameCode { get; set; } = new();

    [Nullable(true)]
    public CodeValue TypeCode { get; set; } = new();

    [Nullable(true)]
    public string ItemID { get; set; } = string.Empty;
}

/// <summary>
/// Occupational classification information
/// </summary>
public class OccupationalClassification
{
    [Nullable(true)]
    public CodeValue NameCode { get; set; } = new();

    [Nullable(true)]
    public CodeValue ClassificationCode { get; set; } = new();
}

/// <summary>
/// Wage law coverage information
/// </summary>
public class WageLawCoverage
{
    [Nullable(true)]
    public CodeValue WageLawNameCode { get; set; } = new();

    [Nullable(true)]
    public CodeValue CoverageCode { get; set; } = new();
}

/// <summary>
/// Base remuneration information
/// </summary>
public class BaseRemuneration
{
    [Nullable(true)]
    public DateTime EffectiveDate { get; set; } = DateTime.MinValue;

    [Nullable(true)]
    public Amount PayPeriodRateAmount { get; set; } = new();

    [Nullable(true)]
    public Amount AnnualRateAmount { get; set; } = new();

    [Nullable(true)]
    public Amount HourlyRateAmount { get; set; } = new();

    [Nullable(true)]
    public Amount DailyRateAmount { get; set; } = new();

    [Nullable(true)]
    public Amount WeeklyRateAmount { get; set; } = new();

    [Nullable(true)]
    public Amount MonthlyRateAmount { get; set; } = new();
}

/// <summary>
/// Additional remuneration information
/// </summary>
public class AdditionalRemuneration
{
    [Nullable(true)]
    public CodeValue NameCode { get; set; } = new();

    [Nullable(true)]
    public DateTime EffectiveDate { get; set; } = DateTime.MinValue;

    [Nullable(true)]
    public Amount Rate { get; set; } = new();
}

/// <summary>
/// Amount with currency information
/// </summary>
public class Amount
{
    [Nullable(true)]
    public CodeValue NameCode { get; set; } = new();

    [Nullable(true)]
    public decimal AmountValue { get; set; } = 0m;

    [Nullable(true)]
    public string CurrencyCode { get; set; } = string.Empty;
}

/// <summary>
/// Standard hours information
/// </summary>
public class StandardHours
{
    [Nullable(true)]
    public decimal HoursQuantity { get; set; } = 0m;

    [Nullable(true)]
    public CodeValue UnitCode { get; set; } = new();
}

/// <summary>
/// Work location information
/// </summary>
public class WorkLocation
{
    [Nullable(true)]
    public CodeValue NameCode { get; set; } = new();

    [Nullable(true)]
    public Address Address { get; set; } = new();
}

/// <summary>
/// Reports to relationship information
/// </summary>
public class ReportsTo
{
    [Nullable(true)]
    public string AssociateOID { get; set; } = string.Empty;

    [Nullable(true)]
    public WorkerID WorkerID { get; set; } = new();

    [Nullable(true)]
    public string PositionID { get; set; } = string.Empty;

    [Nullable(true)]
    public string PositionTitle { get; set; } = string.Empty;

    [Nullable(true)]
    public CodeValue ReportsToRelationshipCode { get; set; } = new();

    [Nullable(true)]
    public ReportsToWorkerName ReportsToWorkerName { get; set; } = new();
}

/// <summary>
/// Reports to worker name information
/// </summary>
public class ReportsToWorkerName
{
    [Nullable(true)]
    public string FormattedName { get; set; } = string.Empty;

    [Nullable(true)]
    public string GivenName { get; set; } = string.Empty;

    [Nullable(true)]
    public string MiddleName { get; set; } = string.Empty;

    [Nullable(true)]
    public string FamilyName1 { get; set; } = string.Empty;
}

/// <summary>
/// Generic code value structure used throughout ADP API
/// </summary>
public class CodeValue
{
    [Nullable(true)]
    public string CodeValueString { get; set; } = string.Empty;

    [Nullable(true)]
    public string ShortName { get; set; } = string.Empty;

    [Nullable(true)]
    public string LongName { get; set; } = string.Empty;
}

