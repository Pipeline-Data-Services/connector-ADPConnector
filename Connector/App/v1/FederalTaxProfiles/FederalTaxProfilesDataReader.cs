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
    private int _currentPage = 0;

    private readonly ApiClient _apiClient;
    
    public FederalTaxProfilesDataReader(
        ILogger<FederalTaxProfilesDataReader> logger,
        ApiClient apiClient)
    {
        _logger = logger;
        _apiClient = apiClient;
    }

    public override async IAsyncEnumerable<FederalTaxProfilesDataObject> GetTypedDataAsync(DataObjectCacheWriteArguments ? dataObjectRunArguments, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting to Federals profiles datareader");

        ApiResponse<IEnumerable<Worker>> response;
        List<FederalTaxProfilesDataObject> federalTaxProfiles = new List<FederalTaxProfilesDataObject>();

        try
        {
            // Fetch all workers with automatic pagination and rate limiting
            response = await _apiClient.GetAllWorkersAsync(cancellationToken)
                .ConfigureAwait(false);

            //get workers list
            var workersList = response.Data.ToList();
            _logger.LogInformation("Successfully fetched {Count} workers from ADP API", workersList.Count);

            foreach (var worker in workersList)
            {

            }

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

        foreach (var dataObject in federalTaxProfiles)
        {
            yield return dataObject;
        }

        _logger.LogInformation("completed Federals profiles datareader");
    }
}