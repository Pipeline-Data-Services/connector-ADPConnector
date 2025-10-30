using Connector.App.v1.Workers;
using Connector.Client;
using ESR.Hosting.CacheWriter;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using Xchange.Connector.SDK.CacheWriter;

namespace Connector.App.v1.FederalTaxProfiles;

public class FederalTaxProfilesDataReader : TypedAsyncDataReaderBase<FederalTaxProfilesDataObject>
{
    private readonly ILogger<FederalTaxProfilesDataReader> _logger;
    private readonly ApiClient _apiClient;
    
    public FederalTaxProfilesDataReader(
        ILogger<FederalTaxProfilesDataReader> logger,
        ApiClient apiClient)
    {
        _logger = logger;
        _apiClient = apiClient;
    }

    public override async IAsyncEnumerable<FederalTaxProfilesDataObject> GetTypedDataAsync(
        DataObjectCacheWriteArguments? dataObjectRunArguments, 
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting to fetch Federal Tax Profiles from ADP API");

        List<FederalTaxProfilesDataObject> federalTaxProfiles = new List<FederalTaxProfilesDataObject>();

        try
        {
            // Step 1: Fetch all workers
            var workersResponse = await _apiClient.GetAllWorkersAsync(cancellationToken)
                .ConfigureAwait(false);

            if (!workersResponse.IsSuccessful)
            {
                var errorMessage = $"Failed to retrieve workers from ADP API. StatusCode: {workersResponse.StatusCode}";
                _logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            if (workersResponse.Data == null)
            {
                _logger.LogWarning("No workers data returned from ADP API");
                yield break;
            }

            var workersList = workersResponse.Data.ToList();
            _logger.LogInformation("Successfully fetched {Count} workers from ADP API", workersList.Count);

            // Step 2: Loop through all workers and fetch their tax profiles
            int processedCount = 0;
            int successCount = 0;
            int failureCount = 0;

            foreach (var worker in workersList)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Tax profile fetch cancelled after processing {Count} workers", processedCount);
                    break;
                }

                try
                {
                    // Get tax profile for this worker
                    var taxProfileResponse = await _apiClient.GetWorkerTaxProfileAsync(
                        worker.AssociateOID, 
                        cancellationToken)
                        .ConfigureAwait(false);

                    processedCount++;

                    if (taxProfileResponse.IsSuccessful && taxProfileResponse.Data?.UsTaxProfiles != null)
                    {
                        // Map the single tax profile to FederalTaxProfilesDataObject
                        var mappedProfile = MapToFederalTaxProfilesDataObject(
                            taxProfileResponse.Data.UsTaxProfiles, 
                            worker.AssociateOID);
                        federalTaxProfiles.Add(mappedProfile);
                        successCount++;

                        _logger.LogDebug("Retrieved tax profile for worker {AssociateOID}", worker.AssociateOID);
                    }
                    else
                    {
                        // Log warning but continue processing other workers
                        _logger.LogWarning("Failed to retrieve tax profile for worker {AssociateOID}. StatusCode: {StatusCode}", 
                            worker.AssociateOID, 
                            taxProfileResponse.StatusCode);
                        failureCount++;
                    }
                }
                catch (ApiException apiEx)
                {
                    _logger.LogError(apiEx, 
                        "API exception while fetching tax profile for worker {AssociateOID}. StatusCode: {StatusCode}", 
                        worker.AssociateOID, 
                        apiEx.StatusCode);
                    failureCount++;
                    // Continue processing other workers
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, 
                        "Unexpected exception while fetching tax profile for worker {AssociateOID}", 
                        worker.AssociateOID);
                    failureCount++;
                    // Continue processing other workers
                }

                // Log progress every 100 workers
                if (processedCount % 100 == 0)
                {
                    _logger.LogInformation("Progress: Processed {Processed}/{Total} workers. Success: {Success}, Failures: {Failures}", 
                        processedCount, 
                        workersList.Count, 
                        successCount, 
                        failureCount);
                }
            }

            _logger.LogInformation("Completed fetching tax profiles. Total: {Total}, Success: {Success}, Failures: {Failures}", 
                processedCount, 
                successCount, 
                failureCount);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "HTTP exception while fetching data from ADP API");
            throw;
        }
        catch (ApiException exception)
        {
            _logger.LogError(exception, "API exception while fetching data from ADP API. StatusCode: {StatusCode}", exception.StatusCode);
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected exception while fetching tax profiles from ADP API");
            throw;
        }

        // Step 3: Yield return all collected tax profiles
        foreach (var dataObject in federalTaxProfiles)
        {
            yield return dataObject;
        }

        _logger.LogInformation("Completed Federal Tax Profiles data reader. Total profiles returned: {Count}", federalTaxProfiles.Count);
    }

    /// <summary>
    /// Maps USTaxProfile (API response model with JSON attributes) to FederalTaxProfilesDataObject (internal data model)
    /// </summary>
    private FederalTaxProfilesDataObject MapToFederalTaxProfilesDataObject(USTaxProfile taxProfile, string associateOID)
    {
        return new FederalTaxProfilesDataObject
        {
            ProfileId = taxProfile.ItemID ?? Guid.NewGuid().ToString(),
            AssociateOID = associateOID,
            PayrollFileNumber = taxProfile.PayrollFileNumber ?? string.Empty,
            PayrollGroupCode = MapTaxCodeValue(taxProfile.PayrollGroupCode),
            USFederalTaxInstruction = MapUSFederalTaxInstruction(taxProfile.USFederalTaxInstruction),
            USStateTaxInstructions = taxProfile.USStateTaxInstructions?.Select(MapUSStateTaxInstruction).ToList() ?? new List<USStateTaxInstruction>(),
            USLocalTaxInstructions = taxProfile.USLocalTaxInstructions?.Select(MapUSLocalTaxInstruction).ToList() ?? new List<USLocalTaxInstruction>()
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

        return DateTime.MinValue;
    }

    private USFederalTaxInstruction MapUSFederalTaxInstruction(USFederalTaxInstructionDTO? dto)
    {
        if (dto == null) return new USFederalTaxInstruction();

        return new USFederalTaxInstruction
        {
            FederalIncomeTaxInstruction = MapFederalIncomeTaxInstruction(dto.FederalIncomeTaxInstruction),
            SocialSecurityTaxInstruction = MapTaxInstruction(dto.SocialSecurityTaxInstruction),
            MedicareTaxInstruction = MapTaxInstruction(dto.MedicareTaxInstruction),
            FederalUnemploymentTaxInstruction = MapTaxInstruction(dto.FederalUnemploymentTaxInstruction),
            Form1099Instruction = MapForm1099Instruction(dto.Form1099Instruction),
            InterimW2IssuedIndicator = dto.InterimW2IssuedIndicator ?? false,
            StatutoryWorkerIndicator = dto.StatutoryWorkerIndicator ?? false,
            QualifiedPensionPlanCoverageIndicator = dto.QualifiedPensionPlanCoverageIndicator ?? false,
            MultipleJobIndicator = dto.MultipleJobIndicator ?? false
        };
    }

    private FederalIncomeTaxInstruction MapFederalIncomeTaxInstruction(FederalIncomeTaxInstructionDTO? dto)
    {
        if (dto == null) return new FederalIncomeTaxInstruction();

        return new FederalIncomeTaxInstruction
        {
            TaxWithholdingStatus = MapTaxWithholdingStatus(dto.TaxWithholdingStatus),
            TaxFilingStatusCode = MapTaxCodeValue(dto.TaxFilingStatusCode),
            TaxWithholdingAllowanceQuantity = dto.TaxWithholdingAllowanceQuantity ?? 0m,
            AdditionalTaxPercentage = dto.AdditionalTaxPercentage ?? 0m,
            AdditionalTaxAmount = MapTaxAmount(dto.AdditionalTaxAmount),
            OverrideTaxPercentage = dto.OverrideTaxPercentage ?? 0m,
            OverrideTaxAmount = MapTaxAmount(dto.OverrideTaxAmount),
            TaxAllowances = dto.TaxAllowances?.Select(MapTaxAllowance).ToList() ?? new List<TaxAllowance>(),
            AdditionalIncomeAmount = MapTaxAmount(dto.AdditionalIncomeAmount)
        };
    }

    private TaxInstruction MapTaxInstruction(TaxInstructionDTO? dto)
    {
        if (dto == null) return new TaxInstruction();

        return new TaxInstruction
        {
            TaxWithholdingStatus = MapTaxWithholdingStatus(dto.TaxWithholdingStatus)
        };
    }

    private Form1099Instruction MapForm1099Instruction(Form1099InstructionDTO? dto)
    {
        if (dto == null) return new Form1099Instruction();

        return new Form1099Instruction
        {
            DistributionCodes = dto.DistributionCodes?.Select(MapTaxCodeValue).ToList() ?? new List<TaxCodeValue>(),
            TotalDistributionIndicator = dto.TotalDistributionIndicator ?? false,
            IndividualRetirementAccountIndicator = dto.IndividualRetirementAccountIndicator ?? false,
            SimplifiedEmployeePensionAccountIndicator = dto.SimplifiedEmployeePensionAccountIndicator ?? false
        };
    }

    private TaxAllowance MapTaxAllowance(TaxAllowanceDTO dto)
    {
        return new TaxAllowance
        {
            AllowanceCode = MapTaxCodeValue(dto.AllowanceCode),
            AllowanceQuantity = dto.AllowanceQuantity ?? 0m
        };
    }

    private USStateTaxInstruction MapUSStateTaxInstruction(USStateTaxInstructionDTO dto)
    {
        return new USStateTaxInstruction
        {
            ItemID = dto.ItemID ?? string.Empty,
            TaxWithholdingStatus = MapTaxWithholdingStatus(dto.TaxWithholdingStatus),
            StateIncomeTaxInstruction = MapStateIncomeTaxInstruction(dto.StateIncomeTaxInstruction),
            StateDisabilityInsuranceTaxInstruction = MapTaxInstruction(dto.StateDisabilityInsuranceTaxInstruction),
            StateUnemploymentTaxInstruction = MapTaxInstruction(dto.StateUnemploymentTaxInstruction)
        };
    }

    private StateIncomeTaxInstruction MapStateIncomeTaxInstruction(StateIncomeTaxInstructionDTO? dto)
    {
        if (dto == null) return new StateIncomeTaxInstruction();

        return new StateIncomeTaxInstruction
        {
            TaxWithholdingStatus = MapTaxWithholdingStatus(dto.TaxWithholdingStatus),
            StateCode = MapTaxCodeValue(dto.StateCode),
            TaxFilingStatusCode = MapTaxCodeValue(dto.TaxFilingStatusCode),
            TaxWithholdingAllowanceQuantity = dto.TaxWithholdingAllowanceQuantity ?? 0m,
            AdditionalTaxAmount = MapTaxAmount(dto.AdditionalTaxAmount),
            AdditionalTaxPercentage = dto.AdditionalTaxPercentage ?? 0m,
            ResidencyStatusCode = MapTaxCodeValue(dto.ResidencyStatusCode)
        };
    }

    private USLocalTaxInstruction MapUSLocalTaxInstruction(USLocalTaxInstructionDTO dto)
    {
        return new USLocalTaxInstruction
        {
            ItemID = dto.ItemID ?? string.Empty,
            TaxWithholdingStatus = MapTaxWithholdingStatus(dto.TaxWithholdingStatus),
            LocalIncomeTaxInstruction = MapLocalIncomeTaxInstruction(dto.LocalIncomeTaxInstruction)
        };
    }

    private LocalIncomeTaxInstruction MapLocalIncomeTaxInstruction(LocalIncomeTaxInstructionDTO? dto)
    {
        if (dto == null) return new LocalIncomeTaxInstruction();

        return new LocalIncomeTaxInstruction
        {
            TaxWithholdingStatus = MapTaxWithholdingStatus(dto.TaxWithholdingStatus),
            LocalityCode = MapTaxCodeValue(dto.LocalityCode),
            TaxFilingStatusCode = MapTaxCodeValue(dto.TaxFilingStatusCode),
            TaxWithholdingAllowanceQuantity = dto.TaxWithholdingAllowanceQuantity ?? 0m,
            AdditionalTaxAmount = MapTaxAmount(dto.AdditionalTaxAmount),
            ResidencyStatusCode = MapTaxCodeValue(dto.ResidencyStatusCode)
        };
    }

    private TaxWithholdingStatus MapTaxWithholdingStatus(TaxWithholdingStatusDTO? dto)
    {
        if (dto == null) return new TaxWithholdingStatus();

        return new TaxWithholdingStatus
        {
            StatusCode = MapTaxCodeValue(dto.StatusCode),
            ReasonCode = MapTaxCodeValue(dto.ReasonCode),
            EffectiveDate = ParseDate(dto.EffectiveDate)
        };
    }

    private TaxAmount MapTaxAmount(TaxAmountDTO? dto)
    {
        if (dto == null) return new TaxAmount();

        return new TaxAmount
        {
            NameCode = MapTaxCodeValue(dto.NameCode),
            AmountValue = dto.AmountValue ?? 0m,
            CurrencyCode = dto.CurrencyCode ?? string.Empty
        };
    }

    private TaxCodeValue MapTaxCodeValue(TaxCodeValueDTO? dto)
    {
        if (dto == null) return new TaxCodeValue();

        return new TaxCodeValue
        {
            CodeValueString = dto.CodeValueString ?? string.Empty,
            ShortName = dto.ShortName ?? string.Empty,
            LongName = dto.LongName ?? string.Empty
        };
    }
}