namespace Connector.App.v1;
using Connector.App.v1.FederalTaxProfiles;
using Connector.App.v1.LaborChargeCodes;
using Connector.App.v1.LocalTaxProfiles;
using Connector.App.v1.StateTaxProfiles;
using Connector.App.v1.TimeCards;
using Connector.App.v1.Workers;
using ESR.Hosting.CacheWriter;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;
using Xchange.Connector.SDK.Abstraction.Change;
using Xchange.Connector.SDK.Abstraction.Hosting;
using Xchange.Connector.SDK.CacheWriter;
using Xchange.Connector.SDK.Hosting.Configuration;

public class AppV1CacheWriterServiceDefinition : BaseCacheWriterServiceDefinition<AppV1CacheWriterConfig>
{
    public override string ModuleId => "app-1";
    public override Type ServiceType => typeof(GenericCacheWriterService<AppV1CacheWriterConfig>);

    public override void ConfigureServiceDependencies(IServiceCollection serviceCollection, string serviceConfigJson)
    {
        var serviceConfig = JsonSerializer.Deserialize<AppV1CacheWriterConfig>(serviceConfigJson);
        serviceCollection.AddSingleton<AppV1CacheWriterConfig>(serviceConfig!);
        serviceCollection.AddSingleton<GenericCacheWriterService<AppV1CacheWriterConfig>>();
        serviceCollection.AddSingleton<ICacheWriterServiceDefinition<AppV1CacheWriterConfig>>(this);
        // Register Data Readers as Singletons
        serviceCollection.AddSingleton<WorkersDataReader>();
        serviceCollection.AddSingleton<FederalTaxProfilesDataReader>();
        serviceCollection.AddSingleton<StateTaxProfilesDataReader>();
        serviceCollection.AddSingleton<LocalTaxProfilesDataReader>();
        serviceCollection.AddSingleton<TimeCardsDataReader>();
        serviceCollection.AddSingleton<LaborChargeCodesDataReader>();
    }

    public override IDataObjectChangeDetectorProvider ConfigureChangeDetectorProvider(IChangeDetectorFactory factory, ConnectorDefinition connectorDefinition)
    {
        var options = factory.CreateProviderOptionsWithNoDefaultResolver();
        // Configure Data Object Keys for Data Objects that do not use the default

        this.RegisterKeysForObject<WorkersDataObject>(options, connectorDefinition);
        this.RegisterKeysForObject<FederalTaxProfilesDataObject>(options, connectorDefinition);
        this.RegisterKeysForObject<StateTaxProfilesDataObject>(options, connectorDefinition);
        this.RegisterKeysForObject<LocalTaxProfilesDataObject>(options, connectorDefinition);
        this.RegisterKeysForObject<TimeCardsDataObject>(options, connectorDefinition);
        this.RegisterKeysForObject<LaborChargeCodesDataObject>(options, connectorDefinition);
        return factory.CreateProvider(options);
    }

    public override void ConfigureService(ICacheWriterService service, AppV1CacheWriterConfig config)
    {
        var dataReaderSettings = new DataReaderSettings
        {
            DisableDeletes = false,
            UseChangeDetection = true
        };
        // Register Data Reader configurations for the Cache Writer Service
        //todo: enable workers once their data object is ready
        // service.RegisterDataReader<WorkersDataReader, WorkersDataObject>(ModuleId, config.WorkersConfig, dataReaderSettings);
        service.RegisterDataReader<FederalTaxProfilesDataReader, FederalTaxProfilesDataObject>(ModuleId, config.FederalTaxProfilesConfig, dataReaderSettings);
        service.RegisterDataReader<StateTaxProfilesDataReader, StateTaxProfilesDataObject>(ModuleId, config.StateTaxProfilesConfig, dataReaderSettings);
        service.RegisterDataReader<LocalTaxProfilesDataReader, LocalTaxProfilesDataObject>(ModuleId, config.LocalTaxProfilesConfig, dataReaderSettings);
        service.RegisterDataReader<TimeCardsDataReader, TimeCardsDataObject>(ModuleId, config.TimeCardsConfig, dataReaderSettings);
        service.RegisterDataReader<LaborChargeCodesDataReader, LaborChargeCodesDataObject>(ModuleId, config.LaborChargeCodesConfig, dataReaderSettings);
    }
}