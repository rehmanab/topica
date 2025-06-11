namespace Topica.Pulsar.Models;

public class ClusterConfiguration
{
    public string ServiceUrl { get; set; }
    public string BrokerServiceUrl { get; set; }
    public bool BrokerClientTlsEnabled { get; set; }
    public bool TlsAllowInsecureConnection { get; set; }
    public bool ServbrokerClientTlsEnabledWithKeyStoreiceUrl { get; set; }
    public string BrokerClientTlsTrustStoreType { get; set; }
    public string BrokerClientTlsKeyStoreType { get; set; }
    public string BrokerClientSslFactoryPlugin { get; set; }
}