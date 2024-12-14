using Microsoft.Graph;

namespace DDOT.MPS.Communication.Api.Managers
{
    public interface IGraphClientService
    {
        GraphServiceClient CreateGraphClient();
    }
}
