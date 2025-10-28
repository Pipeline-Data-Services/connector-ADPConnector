using System;
using Xchange.Connector.SDK.Client.AuthTypes;
using Xchange.Connector.SDK.Client.ConnectionDefinitions.Attributes;

namespace Connector.Connections;

[ConnectionDefinition(title: "CustomAuth", description: "")]
public class CustomAuth : ICustomAuth
{
    //Create your own properties here like this (keep in mind only string types are supported currently):
    //[ConnectionProperty(title: "Custom Header", description: "", isRequired: true, isSensitive: false)]
    //public string CustomHeader { get; init; } = string.Empty;

    [ConnectionProperty(title: "Client ID", description: "Client ID", isRequired: true, isSensitive: false)]
    public string  ClientID { get; set; } = "7b179c69-1f23-4b7e-9e93-7be81e377dcb";

    [ConnectionProperty(title: "Client Secret", description: "Client Secret", isRequired: true, isSensitive: false)]
    public string ClientSecret { get; set; } = "ba2b69c6-0116-4122-b6b8-0e281f820672";

    [ConnectionProperty(title: "Client Certificate (Base64)", description: "Client Certificate in Base64 format", isRequired: true, isSensitive: false)]
    public string ClientCertBase64 { get; set; } = "LS0tLS1CRUdJTiBDRVJUSUZJQ0FURS0tLS0tCk1JSUZXakNDQkVLZ0F3SUJBZ0lRYW5rSllxZVlrOXhSR1hLRmRqNjRLekFOQmdrcWhraUc5dzBCQVFzRkFEQ0IKeURFTE1Ba0dBMVVFQmhNQ1ZWTXhLREFtQmdOVkJBb1RIMEYxZEc5dFlYUnBZeUJFWVhSaElGQnliMk5sYzNOcApibWNzSUVsdVl5NHhGekFWQmdOVkJBc1REbGRsWWlCVWFXVnlJRWR5YjNWd01UVXdNd1lEVlFRTEV5d29ReWtnCk1qQXhOaUJCUkZBc0lFbHVZeTRnTFNCR2IzSWdZWFYwYUc5eWFYcGxaQ0IxYzJVZ2IyNXNlVEUvTUQwR0ExVUUKQXhNMlFYVjBiMjFoZEdsaklFUmhkR0VnVUhKdlkyVnpjMmx1WnlCQmNIQnNhV05oZEdsdmJpQlRaWEoyYVdObApjeUJEUVNBdElFY3pNQjRYRFRJMU1EVXhOVEl3TWpVek0xb1hEVEkzTURVeE5USXdNalV6TTFvd2daOHhDekFKCkJnTlZCQVlUQWxWVE1Rc3dDUVlEVlFRSUV3Sk5UakVRTUE0R0ExVUVCeE1IUjI5dlpHaDFaVEVWTUJNR0ExVUUKQ2hNTVMyNXZZbVZzYzJSdmNtWm1NUXN3Q1FZRFZRUUxFd0pKVkRFdk1DMEdDU3FHU0liM0RRRUpBUXdnYlhSbwpiMjFoYzBCd2FYQmxiR2x1WldSaGRHRnpaWEoyYVdObGN5NWpiMjB4SERBYUJnTlZCQU1URTBGd2NGaGphR0Z1CloyVWdVM0JsWTNSeWRXMHdnZ0VpTUEwR0NTcUdTSWIzRFFFQkFRVUFBNElCRHdBd2dnRUtBb0lCQVFEbTVaTlgKQ3loOEJkbzlMZVpqakJneHovbkpERGhSaG10clRXTW42dDhuYUJRU0xUaVI1cHFSeEJ4alhIL1ZjVVU4Z1BwQQpsRDZvbkl0ZnlMSWlGY0hqdithdjFkaC9WYU1JSEN3NC9hVmY4ajB0UjZuczcwK3hZSEV4dUVWQkZHdytreGF4CitMcGZHTXN2TTEwTHFHN0hCSGlmalhpWHFObk1UK2l4NndSWVBFZmFHbmVreVh5RmJlYkVQQTByNG5ad09PMzUKUWNNOEdjb3VKSm15UlhUeG1JNDAyVmhoSVlXQ2FnWnlNK1drY2FxTmdoTk1MN2hNanJRTXpXaEd1RC9MU2hHTQp4NHpoamhtN1YxYWR1UGQ3QkZiVW5XeFF3dTl4a3VteTZENWV3OFpIM2xKWDJiSzNlY3dqcDI3dHdRT1Vvd1lkClRGRWwzREl6Z1ZmUUY5V1pBZ01CQUFHamdnRmxNSUlCWVRBZkJnTlZIU01FR0RBV2dCU3VYcXcxNEkwUUdVdEoKSkNMRHlRUllXeWUyd2pBSkJnTlZIUk1FQWpBQU1Ga0dBMVVkSHdSU01GQXdUcUJNb0VxR1NHaDBkSEE2THk5agpjbXhYVXk1aFpIQXVZMjl0TDBGMWRHOXRZWFJwWTBSaGRHRlFjbTlqWlhOemFXNW5RWEJ3YkdsallYUnBiMjVUClpYSjJhV05sYzBOQkxVY3pMbU55YkRDQmh3WUlLd1lCQlFVSEFRRUVlekI1TUZRR0NDc0dBUVVGQnpBQ2hraG8KZEhSd09pOHZZM0owVjFNdVlXUndMbU52YlM5QmRYUnZiV0YwYVdORVlYUmhVSEp2WTJWemMybHVaMEZ3Y0d4cApZMkYwYVc5dVUyVnlkbWxqWlhORFFTMUhNeTVqY25Rd0lRWUlLd1lCQlFVSE1BR0dGV2gwZEhBNkx5OXZZM053ClYxTXVZV1J3TG1OdmJUQU9CZ05WSFE4QkFmOEVCQU1DQmFBd0h3WURWUjBsQkJnd0ZnWUtLd1lCQkFHQ054UUMKQWdZSUt3WUJCUVVIQXdJd0hRWURWUjBPQkJZRUZMUExJRU8yVzkrNEptcTJMd0laZHQ2MUgwNGdNQTBHQ1NxRwpTSWIzRFFFQkN3VUFBNElCQVFDSGFrS2hRRkdsZ3pjZDU2c0NJWnZYT2x2VmlrRWRvWUlRZGVCaklzZ0ZLR0F4Cm5vN3Y1MXk3aklmcXJwUkVYYzNuTW00T2ZKV1FSdzl1RDFPVlJFRWhaRVFrekV0Y1lpM2hqbldqbE1WVFQ2OHAKbGNhUGdGNmFiSEprQ0RHaldsNDRHSzFnQ2xXdUQrV3N5NEhJdUJDU0pLNm1Fa0NqTW45RDZUSGUyamp6VTlXRQowOE4rVUVZWks0bENnMmUvc2NjbnVqV3pZMTdUcE5ySVJ2SkZpckRyTHBsM0xUSHVTaXNrSldGbk13Lzk1RkxUCjBEM1ZTUFFnd2tNNytmZVBiRXJWTy9EU1lNcEtOeFZJM2dNdnVZcmp3cVhvaGVTR1R2cGZjZ0RzdHVMaEFMMCsKa0JXNHFuWVFHdXZ3UHNQaHdvRkdIUlRzVEIwZ3k5MEpaN3JNa3N0ZgotLS0tLUVORCBDRVJUSUZJQ0FURS0tLS0tCg==";

    [ConnectionProperty(title: "Connection Environment", description: "Connection Environment", isRequired: true, isSensitive: false)]
    public ConnectionEnvironmentCustomAuth ConnectionEnvironment { get; set; } = ConnectionEnvironmentCustomAuth.Unknown;

    public string BaseUrl
    {
        get
        {
          return "https://api.adp.com/";
        }
    }

    public string TokenUrl
    {
        get
        {
            return "https://accounts.adp.com/auth/oauth/v2/token";
        }
    }
}

public enum ConnectionEnvironmentCustomAuth
{
    Unknown = 0,
    Production = 1,
    Test = 2
}