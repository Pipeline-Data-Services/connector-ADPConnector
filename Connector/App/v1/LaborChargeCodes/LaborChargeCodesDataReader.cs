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

namespace Connector.App.v1.LaborChargeCodes;

public class LaborChargeCodesDataReader : TypedAsyncDataReaderBase<LaborChargeCodesDataObject>
{
    private readonly ILogger<LaborChargeCodesDataReader> _logger;
    private int _currentPage = 0;

    private readonly ApiClient _apiClient;
    
    public LaborChargeCodesDataReader(
        ILogger<LaborChargeCodesDataReader> logger,
        ApiClient apiClient)
    {
        _logger = logger;
        _apiClient = apiClient;
    }

    public override async IAsyncEnumerable<LaborChargeCodesDataObject> GetTypedDataAsync(DataObjectCacheWriteArguments ? dataObjectRunArguments, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (true)
        {
            var response = new ApiResponse<PaginatedResponse<LaborChargeCodesDataObject>>();
            // If the LaborChargeCodesDataObject does not have the same structure as the LaborChargeCodes response from the API, create a new class for it and replace LaborChargeCodesDataObject with it.
            // Example:
            // var response = new ApiResponse<IEnumerable<LaborChargeCodesResponse>>();

            // Make a call to your API/system to retrieve the objects/type for the connector's configuration.
            try
            {
                //response = await _apiClient.GetRecords<LaborChargeCodesDataObject>(
                //    relativeUrl: "laborChargeCodes",
                //    page: _currentPage,
                //    cancellationToken: cancellationToken)
                //    .ConfigureAwait(false);
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(exception, "Exception while making a read request to data object 'LaborChargeCodesDataObject'");
                throw;
            }

            if (!response.IsSuccessful)
            {
                throw new Exception($"Failed to retrieve records for 'LaborChargeCodesDataObject'. API StatusCode: {response.StatusCode}");
            }

            if (response.Data == null || !response.Data.Items.Any()) break;

            // Return the data objects to Cache.
            foreach (var item in response.Data.Items)
            {
                // If new class was created to match the API response, create a new LaborChargeCodesDataObject object, map the properties and return a LaborChargeCodesDataObject.

                // Example:
                //var resource = new LaborChargeCodesDataObject
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