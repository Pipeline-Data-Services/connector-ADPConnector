using Connector.App.v1.Workers;
using Connector.Client;
using ESR.Hosting.CacheWriter;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Xchange.Connector.SDK.CacheWriter;
using System.Net.Http;

namespace Connector.App.v1.StateTaxProfiles;

public class StateTaxProfilesDataReader : TypedAsyncDataReaderBase<StateTaxProfilesDataObject>
{
    private readonly ILogger<StateTaxProfilesDataReader> _logger;
    private readonly ApiClient _apiClient;
    
    public StateTaxProfilesDataReader(
        ILogger<StateTaxProfilesDataReader> logger,
        ApiClient apiClient)
    {
        _logger = logger;
        _apiClient = apiClient;
    }

    public override async IAsyncEnumerable<StateTaxProfilesDataObject> GetTypedDataAsync(
        DataObjectCacheWriteArguments? dataObjectRunArguments, 
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting to fetch State Tax Profiles from ADP API");

        List<StateTaxProfilesDataObject> stateTaxProfiles = new List<StateTaxProfilesDataObject>();

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

            // Step 2: Loop through all workers to get their federal tax profiles first
            int processedWorkers = 0;
            int federalProfilesFound = 0;
            int stateTaxProfilesFound = 0;
            int failureCount = 0;

            foreach (var worker in workersList)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("State tax profile fetch cancelled after processing {Count} workers", processedWorkers);
                    break;
                }

                try
                {
                    // Step 2a: Get federal tax profile for this worker
                    var federalTaxProfileResponse = await _apiClient.GetWorkerTaxProfileAsync(
                        worker.AssociateOID, 
                        cancellationToken)
                        .ConfigureAwait(false);

                    processedWorkers++;

                    if (federalTaxProfileResponse.IsSuccessful && 
                        federalTaxProfileResponse.Data?.UsTaxProfiles != null &&
                        !string.IsNullOrEmpty(federalTaxProfileResponse.Data.UsTaxProfiles.ItemID))
                    {
                        federalProfilesFound++;
                        var federalTaxProfileId = federalTaxProfileResponse.Data.UsTaxProfiles.ItemID;

                        _logger.LogDebug("Found federal tax profile {FederalTaxProfileId} for worker {AssociateOID}", 
                            federalTaxProfileId, 
                            worker.AssociateOID);

                        // Step 2b: Get state tax profiles for this worker using the federal tax profile ID
                        var stateTaxProfileResponse = await _apiClient.GetWorkerStateTaxProfileAsync(
                            worker.AssociateOID,
                            federalTaxProfileId,
                            cancellationToken)
                            .ConfigureAwait(false);

                        if (stateTaxProfileResponse.IsSuccessful && 
                            stateTaxProfileResponse.Data?.StateTaxWithholdings != null)
                        {
                            // Map each state tax withholding to StateTaxProfilesDataObject
                            foreach (var stateTaxWrapper in stateTaxProfileResponse.Data.StateTaxWithholdings)
                            {
                                if (stateTaxWrapper?.StateTaxWithholding != null)
                                {
                                    var mappedProfile = MapToStateTaxProfilesDataObject(
                                        stateTaxWrapper.StateTaxWithholding,
                                        worker.AssociateOID,
                                        federalTaxProfileId);
                                    stateTaxProfiles.Add(mappedProfile);
                                    stateTaxProfilesFound++;
                                }
                            }

                            if (stateTaxProfileResponse.Data.StateTaxWithholdings.Count > 0)
                            {
                                _logger.LogDebug("Retrieved {Count} state tax profile(s) for worker {AssociateOID}", 
                                    stateTaxProfileResponse.Data.StateTaxWithholdings.Count, 
                                    worker.AssociateOID);
                            }
                        }
                        else
                        {
                            _logger.LogDebug("No state tax profiles found for worker {AssociateOID}, federal profile {FederalTaxProfileId}. StatusCode: {StatusCode}", 
                                worker.AssociateOID,
                                federalTaxProfileId,
                                stateTaxProfileResponse.StatusCode);
                        }
                    }
                    else
                    {
                        _logger.LogDebug("No federal tax profile found for worker {AssociateOID}. StatusCode: {StatusCode}", 
                            worker.AssociateOID, 
                            federalTaxProfileResponse.StatusCode);
                        failureCount++;
                    }
                }
                catch (ApiException apiEx)
                {
                    _logger.LogError(apiEx, 
                        "API exception while fetching tax profiles for worker {AssociateOID}. StatusCode: {StatusCode}", 
                        worker.AssociateOID, 
                        apiEx.StatusCode);
                    failureCount++;
                    // Continue processing other workers
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, 
                        "Unexpected exception while fetching tax profiles for worker {AssociateOID}", 
                        worker.AssociateOID);
                    failureCount++;
                    // Continue processing other workers
                }

                // Log progress every 100 workers
                if (processedWorkers % 100 == 0)
                {
                    _logger.LogInformation("Progress: Processed {Processed}/{Total} workers. Federal Profiles: {Federal}, State Profiles: {State}, Failures: {Failures}", 
                        processedWorkers, 
                        workersList.Count, 
                        federalProfilesFound,
                        stateTaxProfilesFound,
                        failureCount);
                }
            }

            _logger.LogInformation("Completed fetching state tax profiles. Workers: {Workers}, Federal Profiles: {Federal}, State Profiles: {State}, Failures: {Failures}", 
                processedWorkers, 
                federalProfilesFound,
                stateTaxProfilesFound,
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
            _logger.LogError(exception, "Unexpected exception while fetching state tax profiles from ADP API");
            throw;
        }

        // Step 3: Yield return all collected state tax profiles
        foreach (var dataObject in stateTaxProfiles)
        {
            yield return dataObject;
        }

        _logger.LogInformation("Completed State Tax Profiles data reader. Total profiles returned: {Count}", stateTaxProfiles.Count);
    }

    /// <summary>
    /// Maps StateTaxWithholding (API response model) to StateTaxProfilesDataObject (internal data model)
    /// </summary>
    private StateTaxProfilesDataObject MapToStateTaxProfilesDataObject(
        StateTaxWithholding stateTaxWithholding, 
        string associateOID,
        string federalTaxProfileId)
    {
        return new StateTaxProfilesDataObject
        {
            ProfileId = stateTaxWithholding.ItemID ?? Guid.NewGuid().ToString(),
            AssociateOID = associateOID,
            FederalTaxProfileId = federalTaxProfileId,
            StateIncomeTaxInstruction = MapStateIncomeTaxInstruction(stateTaxWithholding.StateIncomeTaxInstruction),
            StateDisabilityInsuranceTaxInstruction = MapStateTaxInstruction(stateTaxWithholding.StateDisabilityInsuranceTaxInstruction),
            StateUnemploymentInsuranceTaxInstruction = MapStateTaxInstruction(stateTaxWithholding.StateUnemploymentInsuranceTaxInstruction),
            ResidencyStatusCode = MapStateTaxCodeValue(stateTaxWithholding.ResidencyStatusCode)
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

    private StateIncomeTaxInstruction MapStateIncomeTaxInstruction(StateIncomeTaxInstructionDTO? dto)
    {
        if (dto == null) return new StateIncomeTaxInstruction();

        return new StateIncomeTaxInstruction
        {
            TaxWithholdingStatus = MapStateTaxWithholdingStatus(dto.TaxWithholdingStatus),
            StateCode = MapStateTaxCodeValue(dto.StateCode),
            TaxFilingStatusCode = MapStateTaxCodeValue(dto.TaxFilingStatusCode),
            TaxWithholdingAllowanceQuantity = dto.TaxWithholdingAllowanceQuantity ?? 0m,
            DependentsQuantity = dto.DependentsQuantity ?? 0,
            ExemptionsQuantity = dto.ExemptionsQuantity ?? 0,
            PersonalExemptionsQuantity = dto.PersonalExemptionsQuantity ?? 0,
            DependentExemptionsQuantity = dto.DependentExemptionsQuantity ?? 0,
            AdditionalTaxAmount = MapStateTaxAmount(dto.AdditionalTaxAmount),
            AdditionalTaxPercentage = dto.AdditionalTaxPercentage ?? 0m,
            OverrideTaxAmount = MapStateTaxAmount(dto.OverrideTaxAmount),
            OverrideTaxPercentage = dto.OverrideTaxPercentage ?? 0m,
            EstimatedDeductionAmount = MapStateTaxAmount(dto.EstimatedDeductionAmount),
            TaxAllowances = dto.TaxAllowances?.Select(MapStateTaxAllowance).ToList() ?? new List<StateTaxAllowance>(),
            ReciprocityLocationCode = MapStateTaxCodeValue(dto.ReciprocityLocationCode),
            StateTaxLiabilityCode = MapStateTaxCodeValue(dto.StateTaxLiabilityCode),
            HeadOfHouseholdIndicator = dto.HeadOfHouseholdIndicator ?? false,
            BlindIndicator = dto.BlindIndicator ?? false,
            AgeIndicator = dto.AgeIndicator ?? false,
            SpouseEmploymentIndicator = dto.SpouseEmploymentIndicator ?? false
        };
    }

    private StateTaxInstruction MapStateTaxInstruction(StateTaxInstructionDTO? dto)
    {
        if (dto == null) return new StateTaxInstruction();

        return new StateTaxInstruction
        {
            TaxWithholdingStatus = MapStateTaxWithholdingStatus(dto.TaxWithholdingStatus),
            StateCode = MapStateTaxCodeValue(dto.StateCode)
        };
    }

    private StateTaxWithholdingStatus MapStateTaxWithholdingStatus(StateTaxWithholdingStatusDTO? dto)
    {
        if (dto == null) return new StateTaxWithholdingStatus();

        return new StateTaxWithholdingStatus
        {
            StatusCode = MapStateTaxCodeValue(dto.StatusCode),
            ReasonCode = MapStateTaxCodeValue(dto.ReasonCode),
            EffectiveDate = ParseDate(dto.EffectiveDate)
        };
    }

    private StateTaxAllowance MapStateTaxAllowance(StateTaxAllowanceDTO dto)
    {
        return new StateTaxAllowance
        {
            AllowanceCode = MapStateTaxCodeValue(dto.AllowanceCode),
            AllowanceQuantity = dto.AllowanceQuantity ?? 0m
        };
    }

    private StateTaxAmount MapStateTaxAmount(StateTaxAmountDTO? dto)
    {
        if (dto == null) return new StateTaxAmount();

        return new StateTaxAmount
        {
            NameCode = MapStateTaxCodeValue(dto.NameCode),
            AmountValue = dto.AmountValue ?? 0m,
            CurrencyCode = dto.CurrencyCode ?? string.Empty
        };
    }

    private StateTaxCodeValue MapStateTaxCodeValue(StateTaxCodeValueDTO? dto)
    {
        if (dto == null) return new StateTaxCodeValue();

        return new StateTaxCodeValue
        {
            CodeValueString = dto.CodeValueString ?? string.Empty,
            ShortName = dto.ShortName ?? string.Empty,
            LongName = dto.LongName ?? string.Empty
        };
    }
}