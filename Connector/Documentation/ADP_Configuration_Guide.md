# ADP API Configuration Examples

## Quick Reference

### Rate Limiting Configuration
```csharp
// In ApiClient.cs
private const int MinMillisecondsBetweenRequests = 100; // 10 requests per second

// To adjust rate limit, modify this constant:
// - 50ms  = 20 requests/second
// - 100ms = 10 requests/second (ADP recommended)
// - 200ms = 5 requests/second
```

### Pagination Configuration
```csharp
// In ApiClient.cs
private const int DefaultPageSize = 100; // ADP recommended
private const int MaxPageSize = 100;     // ADP maximum

// ADP supports $top values from 1 to 100
// Larger pages = fewer requests but more memory
// Smaller pages = more requests but less memory per request
```

## Example API Calls

### Basic Worker Retrieval
```http
GET /hr/v2/workers?$skip=0&$top=100
Host: api.adp.com
Authorization: Bearer {access_token}
```

### With Filtering (Future Enhancement)
```http
GET /hr/v2/workers?$skip=0&$top=100&$filter=workerStatus/statusCode/codeValue eq 'Active'
Host: api.adp.com
Authorization: Bearer {access_token}
```

### With Field Selection (Future Enhancement)
```http
GET /hr/v2/workers?$skip=0&$top=100&$select=associateOID,person/legalName
Host: api.adp.com
Authorization: Bearer {access_token}
```

## Sample Response Handling

### Successful Response (200 OK)
```json
{
  "workers": [
    {
      "associateOID": "G3349K45KM6D9WH7",
      "workerID": {
        "idValue": "12345",
        "schemeCode": {
          "codeValue": "EID",
          "shortName": "Employee ID"
        }
      },
      "person": {
        "legalName": {
          "givenName": "John",
          "familyName1": "Doe",
          "formattedName": "John Doe"
        },
        "birthDate": "1990-01-15"
      },
      "workAssignments": [
        {
          "itemID": "1",
          "primaryIndicator": true,
          "hireDate": "2020-01-01",
          "jobTitle": "Software Engineer"
        }
      ]
    }
  ]
}
```

### Error Response (401 Unauthorized)
```json
{
  "error": {
    "code": "401",
    "message": "Unauthorized",
    "details": "Invalid or expired access token"
  }
}
```

### Error Response (429 Too Many Requests)
```json
{
  "error": {
    "code": "429",
    "message": "Too Many Requests",
    "details": "Rate limit exceeded"
  }
}
```

## Logging Examples

### Info Level Logs
```
[INFO] Starting to fetch workers from ADP API
[INFO] Successfully fetched 1,234 workers from ADP API
[INFO] Completed fetching workers from ADP API
```

### Warning Level Logs
```
[WARN] No workers data returned from ADP API
[WARN] Found 3 duplicate AssociateOIDs: G123, G456, G789
```

### Error Level Logs
```
[ERROR] HTTP exception while fetching workers from ADP API
        Exception: System.Net.Http.HttpRequestException: The SSL connection could not be established

[ERROR] API exception while fetching workers from ADP API. StatusCode: 401
        Exception: Connector.Client.ApiException: Unauthorized access to ADP API

[ERROR] Failed to retrieve workers from ADP API. StatusCode: 500
```

## Performance Metrics

### Expected Performance
```
Workers Count | Requests Needed | Minimum Time | Expected Time
------------- | --------------- | ------------ | -------------
100           | 1               | 0.1s         | 0.5s
1,000         | 10              | 1.0s         | 2.0s
5,000         | 50              | 5.0s         | 8.0s
10,000        | 100             | 10.0s        | 15.0s
50,000        | 500             | 50.0s        | 75.0s
```

*Note: Expected time includes network latency and API processing*

### Memory Usage
```
Workers Count | Approximate Memory
------------- | ------------------
1,000         | ~2 MB
10,000        | ~20 MB
50,000        | ~100 MB
100,000       | ~200 MB
```

*Note: Actual memory depends on worker data complexity*

## Troubleshooting

### Issue: Rate Limit Exceeded
**Symptom**: HTTP 429 responses
**Solution**: The rate limiter should prevent this, but if it occurs:
1. Increase `MinMillisecondsBetweenRequests` to 150ms or 200ms
2. Check for multiple ApiClient instances competing

### Issue: Timeout Errors
**Symptom**: HttpRequestException with timeout
**Solution**: 
1. Reduce `DefaultPageSize` to 50 or 25
2. Increase HttpClient timeout in configuration
3. Check network connectivity to ADP servers

### Issue: Incomplete Data
**Symptom**: Missing workers compared to ADP portal
**Solution**:
1. Check for workers with status filters
2. Verify authentication scope includes all workers
3. Check ADP API permissions for your application

### Issue: Memory Issues
**Symptom**: OutOfMemoryException with large datasets
**Solution**:
1. The streaming implementation should prevent this
2. Ensure data is processed and released in the yield loop
3. Consider batching downstream processing

## Configuration Checklist

Before deploying:
- [ ] Verify ADP API credentials are correct
- [ ] Confirm base URL points to correct environment (sandbox/production)
- [ ] Test rate limiting with concurrent requests
- [ ] Validate pagination works with your data size
- [ ] Ensure logging level is appropriate (INFO for production)
- [ ] Configure HttpClient timeout appropriately
- [ ] Test error handling with invalid credentials
- [ ] Verify cancellation token support
- [ ] Monitor initial production run for issues

## Environment-Specific Settings

### Development/Sandbox
```csharp
baseUrl = "https://api-sandbox.adp.com"
// More verbose logging
// Smaller page sizes for testing
DefaultPageSize = 10
```

### Production
```csharp
baseUrl = "https://api.adp.com"
// Standard logging
DefaultPageSize = 100
// Consider enabling retry logic
```

## Security Considerations

1. **Never log access tokens**
2. **Store credentials securely** (use Azure Key Vault, etc.)
3. **Use HTTPS only**
4. **Rotate credentials regularly**
5. **Monitor for unusual API usage patterns**
6. **Implement proper error handling** to avoid leaking sensitive info

## Support Resources

- **ADP Developer Portal**: https://developers.adp.com/
- **API Support**: Contact ADP support with your client_id
- **Status Page**: Check ADP API status for outages
- **Rate Limits**: Default is 10 req/sec, contact ADP for increases
