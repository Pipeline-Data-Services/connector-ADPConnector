using Connector.Client;
using System;
using ESR.Hosting.CacheWriter;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Xchange.Connector.SDK.CacheWriter;
using System.Net.Http;

namespace Connector.App.v1.Workers;

public class WorkersDataReader : TypedAsyncDataReaderBase<WorkersDataObject>
{
    private readonly ILogger<WorkersDataReader> _logger;
    private readonly ApiClient _apiClient;

    public WorkersDataReader(
        ILogger<WorkersDataReader> logger,
        ApiClient apiClient)
    {
        _logger = logger;
        _apiClient = apiClient;
    }

    public override async IAsyncEnumerable<WorkersDataObject> GetTypedDataAsync(
        DataObjectCacheWriteArguments? dataObjectRunArguments,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting to fetch workers from ADP API");

        ApiResponse<IEnumerable<Worker>> response;
        List<WorkersDataObject> workersDataObject = new List<WorkersDataObject>();

        try
        {
            // Fetch all workers with automatic pagination and rate limiting
            response = await _apiClient.GetAllWorkersAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "HTTP exception while fetching workers from ADP API");
            throw;
        }
        catch (ApiException exception)
        {
            _logger.LogError(exception, "API exception while fetching workers from ADP API. StatusCode: {StatusCode}", exception.StatusCode);
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected exception while fetching workers from ADP API");
            throw;
        }

        if (!response.IsSuccessful)
        {
            var errorMessage = $"Failed to retrieve workers from ADP API. StatusCode: {response.StatusCode}";
            _logger.LogError(errorMessage);
            throw new Exception(errorMessage);
        }

        if (response.Data == null)
        {
            _logger.LogWarning("No workers data returned from ADP API");
            yield break;
        }

        var workersList = response.Data.ToList();
        _logger.LogInformation("Successfully fetched {Count} workers from ADP API", workersList.Count);

        // Check for duplicate AssociateOIDs
        var duplicates = workersList
            .GroupBy(x => x.AssociateOID)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Any())
        {
            _logger.LogWarning("Found {Count} duplicate AssociateOIDs: {Duplicates}",
                duplicates.Count,
                string.Join(", ", duplicates));
        }

        // Map Worker to WorkersDataObject
        foreach (var worker in workersList)
        {
            var result = MapToWorkersDataObject(worker);
            workersDataObject.Add(result);
        }

        foreach (var dataObject in workersDataObject)
        {
            yield return dataObject;
        }

        _logger.LogInformation("Completed fetching workers from ADP API");
    }

    /// <summary>
    /// Maps Worker (API response model with JSON attributes) to WorkersDataObject (internal data model)
    /// </summary>
    private WorkersDataObject MapToWorkersDataObject(Worker worker)
    {
        return new WorkersDataObject
        {
            AssociateOID = worker.AssociateOID,
            WorkerID = MapWorkerID(worker.WorkerID),
            Person = MapPerson(worker.Person),
            BusinessCommunication = MapBusinessCommunication(worker.BusinessCommunication),
            WorkAssignments = worker.WorkAssignments?.Select(MapWorkAssignment).ToList() ?? new List<WorkAssignment>()
        };
    }

    /// <summary>
    /// Parses a date string to DateTime. Returns DateTime.MinValue if the string is null or invalid.
    /// </summary>
    private DateTime ParseDate(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
            return DateTime.MinValue;

        if (DateTime.TryParse(dateString, out var result))
            return result;

        //_logger.LogWarning("Failed to parse date: {DateString}", dateString);
        return DateTime.MinValue;
    }

    private WorkerID MapWorkerID(WorkerIDDTO? dto)
    {
        if (dto == null) return null!;

        return new WorkerID
        {
            IdValue = dto.IdValue,
            SchemeCode = MapCodeValue(dto.SchemeCode)
        };
    }

    private Person MapPerson(PersonDTO? dto)
    {
        if (dto == null) return null!;

        return new Person
        {
            LegalName = MapLegalName(dto.LegalName),
            PreferredName = MapPreferredName(dto.PreferredName),
            BirthDate = ParseDate(dto.BirthDate),
            GenderCode = MapCodeValue(dto.GenderCode),
            MaritalStatusCode = MapCodeValue(dto.MaritalStatusCode),
            EthnicityCode = MapCodeValue(dto.EthnicityCode),
            RaceCode = MapCodeValue(dto.RaceCode),
            SocialInsurancePrograms = dto.SocialInsurancePrograms?.Select(MapSocialInsuranceProgram).ToList() ?? new List<SocialInsuranceProgram>(),
            LegalAddress = MapAddress(dto.LegalAddress),
            Communication = MapCommunication(dto.Communication)
        };
    }

    private LegalName MapLegalName(LegalNameDTO? dto)
    {
        if (dto == null) return null!;

        return new LegalName
        {
            GivenName = dto.GivenName,
            MiddleName = dto.MiddleName ?? string.Empty,
            FamilyName1 = dto.FamilyName1,
            FamilyName2 = dto.FamilyName2 ?? string.Empty,
            FormattedName = dto.FormattedName ?? string.Empty,
            NameCode = MapCodeValue(dto.NameCode),
            PreferredSalutations = dto.PreferredSalutations?.Select(MapPreferredSalutation).ToList() ?? new List<PreferredSalutation>()
        };
    }

    private PreferredName MapPreferredName(PreferredNameDTO? dto)
    {
        if (dto == null) return null!;

        return new PreferredName
        {
            GivenName = dto.GivenName ?? string.Empty,
            MiddleName = dto.MiddleName ?? string.Empty,
            FamilyName1 = dto.FamilyName1 ?? string.Empty,
            FormattedName = dto.FormattedName ?? string.Empty,
            NameCode = MapCodeValue(dto.NameCode)
        };
    }

    private PreferredSalutation MapPreferredSalutation(PreferredSalutationDTO dto)
    {
        return new PreferredSalutation
        {
            SalutationCode = MapCodeValue(dto.SalutationCode),
            TypeCode = MapCodeValue(dto.TypeCode),
            SequenceNumber = dto.SequenceNumber ?? 0
        };
    }

    private SocialInsuranceProgram MapSocialInsuranceProgram(SocialInsuranceProgramDTO dto)
    {
        return new SocialInsuranceProgram
        {
            NameCode = MapCodeValue(dto.NameCode),
            CoveredIndicator = dto.CoveredIndicator ?? false,
            IdValue = dto.IdValue ?? null!
        };
    }

    private Address MapAddress(AddressDTO? dto)
    {
        if (dto == null) return null!;

        return new Address
        {
            LineOne = dto.LineOne ?? null!,
            LineTwo = dto.LineTwo ?? null!,
            LineThree = dto.LineThree ?? null!,
            CityName = dto.CityName ?? null!,
            CountrySubdivisionLevel1 = MapCodeValue(dto.CountrySubdivisionLevel1),
            CountryCode = dto.CountryCode ?? null!,
            PostalCode = dto.PostalCode ?? null!
        };
    }

    private Communication MapCommunication(CommunicationDTO? dto)
    {
        if (dto == null) return null!;

        return new Communication
        {
            Landlines = dto.Landlines?.Select(MapPhoneNumber).ToList() ?? new List<PhoneNumber>(),
            Mobiles = dto.Mobiles?.Select(MapPhoneNumber).ToList() ?? new List<PhoneNumber>(),
            Emails = dto.Emails?.Select(MapEmail).ToList() ?? new List<Email>()
        };
    }

    private PhoneNumber MapPhoneNumber(PhoneNumberDTO dto)
    {
        return new PhoneNumber
        {
            NameCode = MapCodeValue(dto.NameCode),
            CountryDialing = dto.CountryDialing ?? null!,
            AreaDialing = dto.AreaDialing ?? null!,
            DialNumber = dto.DialNumber ?? null!,
            FormattedNumber = dto.FormattedNumber ?? null!
        };
    }

    private Email MapEmail(EmailDTO dto)
    {
        return new Email
        {
            NameCode = MapCodeValue(dto.NameCode),
            EmailUri = dto.EmailUri ?? null!
        };
    }

    private BusinessCommunication MapBusinessCommunication(BusinessCommunicationDTO? dto)
    {
        if (dto == null) return null!;

        return new BusinessCommunication
        {
            Landlines = dto.Landlines?.Select(MapPhoneNumber).ToList() ?? new List<PhoneNumber>(),
            Mobiles = dto.Mobiles?.Select(MapPhoneNumber).ToList() ?? new List<PhoneNumber>(),
            Emails = dto.Emails?.Select(MapEmail).ToList() ?? new List<Email>()
        };
    }

    private WorkAssignment MapWorkAssignment(WorkAssignmentDTO dto)
    {
        return new WorkAssignment
        {
            ItemID = dto.ItemID ?? null!,
            PrimaryIndicator = dto.PrimaryIndicator ?? false,
            HireDate = ParseDate(dto.HireDate),
            SeniorityDate = ParseDate(dto.SeniorityDate),
            WorkerTypeCode = MapCodeValue(dto.WorkerTypeCode),
            WorkerStatus = MapWorkerStatus(dto.WorkerStatus),
            AssignmentStatus = MapAssignmentStatus(dto.AssignmentStatus),
            HomeOrganizationalUnits = dto.HomeOrganizationalUnits?.Select(MapOrganizationalUnit).ToList() ?? new List<OrganizationalUnit>(),
            AssignedOrganizationalUnits = dto.AssignedOrganizationalUnits?.Select(MapOrganizationalUnit).ToList() ?? new List<OrganizationalUnit>(),
            JobTitle = dto.JobTitle ?? null!,
            JobCode = MapCodeValue(dto.JobCode),
            OccupationalClassifications = dto.OccupationalClassifications?.Select(MapOccupationalClassification).ToList() ?? new List<OccupationalClassification>(),
            WageLawCoverage = MapWageLawCoverage(dto.WageLawCoverage),
            WorkLevelCode = MapCodeValue(dto.WorkLevelCode),
            BaseRemuneration = MapBaseRemuneration(dto.BaseRemuneration),
            AdditionalRemunerations = dto.AdditionalRemunerations?.Select(MapAdditionalRemuneration).ToList() ?? new List<AdditionalRemuneration>(),
            StandardHours = MapStandardHours(dto.StandardHours),
            FullTimeEquivalenceRatio = dto.FullTimeEquivalenceRatio ?? 0m,
            HomeWorkLocation = MapWorkLocation(dto.HomeWorkLocation),
            AssignedWorkLocations = dto.AssignedWorkLocations?.Select(MapWorkLocation).ToList() ?? new List<WorkLocation>(),
            ReportsTo = dto.ReportsTo?.Select(MapReportsTo).ToList() ?? new List<ReportsTo>(),
            ManagementPositionIndicator = dto.ManagementPositionIndicator ?? false,
            PayCycleCode = MapCodeValue(dto.PayCycleCode),
            PayGradeCode = MapCodeValue(dto.PayGradeCode),
            TerminationDate = ParseDate(dto.TerminationDate),
            RehireEligibleIndicator = dto.RehireEligibleIndicator ?? false
        };
    }

    private WorkerStatus MapWorkerStatus(WorkerStatusDTO? dto)
    {
        if (dto == null) return null!;

        return new WorkerStatus
        {
            StatusCode = MapCodeValue(dto.StatusCode),
            EffectiveDate = ParseDate(dto.EffectiveDate)
        };
    }

    private AssignmentStatus MapAssignmentStatus(AssignmentStatusDTO? dto)
    {
        if (dto == null) return null!;

        return new AssignmentStatus
        {
            StatusCode = MapCodeValue(dto.StatusCode),
            EffectiveDate = ParseDate(dto.EffectiveDate),
            ReasonCode = MapCodeValue(dto.ReasonCode)
        };
    }

    private OrganizationalUnit MapOrganizationalUnit(OrganizationalUnitDTO dto)
    {
        return new OrganizationalUnit
        {
            NameCode = MapCodeValue(dto.NameCode),
            TypeCode = MapCodeValue(dto.TypeCode),
            ItemID = dto.ItemID ?? null!
        };
    }

    private OccupationalClassification MapOccupationalClassification(OccupationalClassificationDTO dto)
    {
        return new OccupationalClassification
        {
            NameCode = MapCodeValue(dto.NameCode),
            ClassificationCode = MapCodeValue(dto.ClassificationCode)
        };
    }

    private WageLawCoverage MapWageLawCoverage(WageLawCoverageDTO? dto)
    {
        if (dto == null) return null!;

        return new WageLawCoverage
        {
            WageLawNameCode = MapCodeValue(dto.WageLawNameCode),
            CoverageCode = MapCodeValue(dto.CoverageCode)
        };
    }

    private BaseRemuneration MapBaseRemuneration(BaseRemunerationDTO? dto)
    {
        if (dto == null) return null!;

        return new BaseRemuneration
        {
            EffectiveDate = ParseDate(dto.EffectiveDate),
            PayPeriodRateAmount = MapAmount(dto.PayPeriodRateAmount),
            AnnualRateAmount = MapAmount(dto.AnnualRateAmount),
            HourlyRateAmount = MapAmount(dto.HourlyRateAmount),
            DailyRateAmount = MapAmount(dto.DailyRateAmount),
            WeeklyRateAmount = MapAmount(dto.WeeklyRateAmount),
            MonthlyRateAmount = MapAmount(dto.MonthlyRateAmount)
        };
    }

    private AdditionalRemuneration MapAdditionalRemuneration(AdditionalRemunerationDTO dto)
    {
        return new AdditionalRemuneration
        {
            NameCode = MapCodeValue(dto.NameCode),
            EffectiveDate = ParseDate(dto.EffectiveDate),
            Rate = MapAmount(dto.Rate)
        };
    }

    private Amount MapAmount(AmountDTO? dto)
    {
        if (dto == null) return null!;

        return new Amount
        {
            NameCode = MapCodeValue(dto.NameCode),
            AmountValue = dto.AmountValue ?? 0m,
            CurrencyCode = dto.CurrencyCode ?? null!
        };
    }

    private StandardHours MapStandardHours(StandardHoursDTO? dto)
    {
        if (dto == null) return null!;

        return new StandardHours
        {
            HoursQuantity = dto.HoursQuantity ?? 0m,
            UnitCode = MapCodeValue(dto.UnitCode)
        };
    }

    private WorkLocation MapWorkLocation(WorkLocationDTO? dto)
    {
        if (dto == null) return null!;

        return new WorkLocation
        {
            NameCode = MapCodeValue(dto.NameCode),
            Address = MapAddress(dto.Address)
        };
    }

    private ReportsTo MapReportsTo(ReportsToDTO dto)
    {
        return new ReportsTo
        {
            AssociateOID = dto.AssociateOID ?? string.Empty,
            WorkerID = MapWorkerID(dto.WorkerID),
            PositionID = dto.PositionID ?? string.Empty,
            PositionTitle = dto.PositionTitle ?? string.Empty,
            ReportsToRelationshipCode = MapCodeValue(dto.ReportsToRelationshipCode),
            ReportsToWorkerName = MapReportsToWorkerName(dto.ReportsToWorkerName)
        };
    }

    private ReportsToWorkerName MapReportsToWorkerName(ReportsToWorkerNameDTO? dto)
    {
        if (dto == null) return null!;

        return new ReportsToWorkerName
        {
            FormattedName = dto.FormattedName ?? string.Empty,
            GivenName = dto.GivenName ?? string.Empty,
            MiddleName = dto.MiddleName ?? string.Empty,
            FamilyName1 = dto.FamilyName1 ?? string.Empty
        };
    }

    private CodeValue MapCodeValue(CodeValueDTO? dto)
    {
        if (dto == null) return null!;

        return new CodeValue
        {
            CodeValueString = dto.CodeValueString ?? string.Empty,
            ShortName = dto.ShortName ?? string.Empty,
            LongName = dto.LongName ?? string.Empty
        };
    }
}