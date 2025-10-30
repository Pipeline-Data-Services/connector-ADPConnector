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

namespace Connector.App.v1.StateTaxProfiles;

public class StateTaxProfilesDataReader : TypedAsyncDataReaderBase<StateTaxProfilesDataObject>
{
    private readonly ILogger<StateTaxProfilesDataReader> _logger;
    private int _currentPage = 0;

    private readonly ApiClient _apiClient;
    
    public StateTaxProfilesDataReader(
        ILogger<StateTaxProfilesDataReader> logger,
        ApiClient apiClient)
    {
        _logger = logger;
        _apiClient = apiClient;
    }

    public override async IAsyncEnumerable<StateTaxProfilesDataObject> GetTypedDataAsync(DataObjectCacheWriteArguments ? dataObjectRunArguments, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (true)
        {
            var response = new ApiResponse<PaginatedResponse<StateTaxProfilesDataObject>>();
            // If the StateTaxProfilesDataObject does not have the same structure as the StateTaxProfiles response from the API, create a new class for it and replace StateTaxProfilesDataObject with it.
            // Example:
            // var response = new ApiResponse<IEnumerable<StateTaxProfilesResponse>>();

            // Make a call to your API/system to retrieve the objects/type for the connector's configuration.
            try
            {
                //response = await _apiClient.GetRecords<StateTaxProfilesDataObject>(
                //    relativeUrl: "stateTaxProfiles",
                //    page: _currentPage,
                //    cancellationToken: cancellationToken)
                //    .ConfigureAwait(false);
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(exception, "Exception while making a read request to data object 'StateTaxProfilesDataObject'");
                throw;
            }

            if (!response.IsSuccessful)
            {
                throw new Exception($"Failed to retrieve records for 'StateTaxProfilesDataObject'. API StatusCode: {response.StatusCode}");
            }

            if (response.Data == null || !response.Data.Items.Any()) break;

            // Return the data objects to Cache.
            foreach (var item in response.Data.Items)
            {
                // If new class was created to match the API response, create a new StateTaxProfilesDataObject object, map the properties and return a StateTaxProfilesDataObject.

                // Example:
                //var resource = new StateTaxProfilesDataObject
                //{
                //// TODO: Map properties.      
                //};
                //yield return resource;
                yield return item;
            }

            // Handle pagination per API client design
            _currentPage++;
            if (_currentPage >= response.Data.TotalPages)
            {
                break;
            }
        }
    }
}