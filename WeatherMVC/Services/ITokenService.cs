using System.Threading.Tasks;
using IdentityModel.Client;

namespace WeatherMVC.Services
{
    public interface ITokenService
    {
        Task<TokenResponse> GetToken(string scope);

        Task<TokenResponse> GetRefreshToken(string refreshToken);

    }
}