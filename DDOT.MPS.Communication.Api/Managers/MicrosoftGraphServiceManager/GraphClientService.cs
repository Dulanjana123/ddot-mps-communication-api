using Azure.Identity;
using DDOT.MPS.Communication.Model.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace DDOT.MPS.Communication.Api.Managers
{
    public class GraphClientService: IGraphClientService
    {
        private readonly DdotMpsMeetingConfigeration _ddotMpsMeetingConfigeration;        

        public GraphClientService(IOptions<DdotMpsMeetingConfigeration> ddotMpsGraphApiConfigerationOptions)
        {
            _ddotMpsMeetingConfigeration = ddotMpsGraphApiConfigerationOptions != null 
                ? ddotMpsGraphApiConfigerationOptions.Value
                : throw new ArgumentNullException(nameof(ddotMpsGraphApiConfigerationOptions));            
        }

        public GraphServiceClient CreateGraphClient()
        {
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var options = new ClientSecretCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,                
            };

            var clientSecretCredential = new ClientSecretCredential(
                _ddotMpsMeetingConfigeration.GraphApiAppTenantId,
                _ddotMpsMeetingConfigeration.GraphApiAppClientId,
                _ddotMpsMeetingConfigeration.GraphApiAppClientSecret,
                options);

            return new GraphServiceClient(clientSecretCredential, scopes);
        }
    }
}
