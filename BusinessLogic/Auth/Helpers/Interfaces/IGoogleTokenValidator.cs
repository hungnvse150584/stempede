using Google.Apis.Auth;

namespace BusinessLogic.Auth.Helpers.Interfaces
{
    public interface IGoogleTokenValidator
    {
        Task<GoogleJsonWebSignature.Payload> ValidateAsync(string idToken);
    }

}
