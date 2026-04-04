using g_map_compare_backend.Common.Results;
using PlayPredictorWebAPI.Models;

namespace PlayPredictorWebAPI.Services
{
    public class AccountApplicationService
    {
        private readonly OAuthService _oauthService;
        private readonly CalendarService _calendarService;
        
        public AccountApplicationService(OAuthService oauthService, CalendarService calendarService)
        {
            _oauthService = oauthService;
            _calendarService = calendarService;
        }

        public async Task<Result> RemoveGoogleAccount()
        {
            var res = await _calendarService.DeleteUserCalendars();
            if (!res.Success)
                return res;
            var res2 = await _oauthService.DisconnectGoogleAccountAsync();
            if(!res2.Success)
                return res2;

            return Result.Ok();
        }

        public async Task<Result> ConnectGoogleAccountAsync(string code, string state)
        {
            var res = await _oauthService.ConnectGoogleCalendar(code, state);
            if (!res.Success)
                return res;

            var externalAccounts = await _oauthService.GetExternalOAuthAccounts(res.Data.User);
            var existing = externalAccounts.FirstOrDefault(a => a.Provider == OAuthProvider.Google);

            if (existing != null)
            {
                await _calendarService.DeleteUserAPICalendars(res.Data.User);

                await _oauthService.UpdateGoogleAccountAsync(res.Data.ProviderUserId, res.Data.Email, res.Data.OAuthToken, existing);
            }else
            {
                await _oauthService.CreateGoogleAccount(res.Data);
            }

            return Result.Ok();
        }
    }
}
