using g_map_compare_backend.Common.Results;
using g_map_compare_backend.Dtos.Auth;
using g_map_compare_backend.Dtos.User;
using g_map_compare_backend.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayPredictorWebAPI.Common.Results;
using PlayPredictorWebAPI.Dtos.Auth;
using PlayPredictorWebAPI.Dtos.User;
using PlayPredictorWebAPI.Models;
using PlayPredictorWebAPI.Services;

namespace g_map_compare_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly AuthService _authService;
        private readonly AccountApplicationService _accountApplicationService;
        private readonly OAuthService _oAuthService;
        private readonly IConfiguration _configuration;
        private readonly OAuthStateService _stateService;
        public AuthController(AuthService authService, OAuthService oAuthService, IConfiguration configuration, OAuthStateService stateService, AccountApplicationService accountApplicationService)
        {
            _authService = authService;
            _oAuthService = oAuthService;
            _configuration = configuration;
            _stateService = stateService;
            _accountApplicationService = accountApplicationService;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            var refreshToken = result.RefreshToken;
            if (refreshToken != null)
            {
                Response.Cookies.Append("refreshToken", refreshToken.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = refreshToken.Expires
                });
            }

            return result.LoginResponse.ToActionResult<LoginResponseDto>(this);

        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LoginResponseDto>> Register(UserRegisterRequestDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            var refreshToken = result.RefreshToken;

            if (refreshToken != null)
            {
                Response.Cookies.Append("refreshToken", refreshToken.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = refreshToken.Expires
                });
            }

            return result.LoginResponse.ToActionResult<LoginResponseDto>(this);
        }

        [HttpPost("refresh")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LoginResponseDto>> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (refreshToken == null)
                return BadRequest("No refresh token provided. Return to login");

            var result = await _authService.RefreshTokenAsync(refreshToken);

            return result.ToActionResult(this);
        }

        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (refreshToken == null)
                return BadRequest("No refresh token provided. Return to login");

            var result = await _authService.LogoutAsync(refreshToken);

            Response.Cookies.Delete("refreshToken");

            return result.ToActionResult(this);
        }

        [Authorize]
        [HttpPost("password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> ChangePassword(UserUpdatePasswordDto dto)
        {
            var result = await _authService.ChangePassword(dto);

            if (result.Success)
                Response.Cookies.Delete("refreshToken");

            return result.ToActionResult(this);
        }

        [HttpPost("google")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LoginResponseDto>> EnterWithGoogle(LoginGoogleRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.IdToken))
                return BadRequest(new ProblemDetails { Title = "Login Error", Detail = "Ungültiges token gesendet" });

            var result = await _authService.LoginWithGoogleAsync(dto.IdToken);

            var refreshToken = result.RefreshToken;

            if (refreshToken != null)
            {
                Response.Cookies.Append("refreshToken", refreshToken.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = refreshToken.Expires
                });
            }

            return result.LoginResponse.ToActionResult(this);
        }


        [Authorize]
        [HttpGet("AuthProvider")]
        [ProducesResponseType(typeof(AuthProviderResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthProviderResponseDto>> GetAuthProvider()
        {
            var result = await _authService.GetAuthProviderAsync();
            return result.ToActionResult(this);

        }

        [HttpGet("google-calendar")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async  Task<ActionResult<string>> InitGoogleCalendarOAuth(string? redirectUri)
        {
            var result = await _oAuthService.StartGoogleOAuth(redirectUri);

            return result.ToActionResult(this);
        }

        [HttpGet("google-calendar/callback")]
        [ProducesResponseType(typeof(AuthProviderResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ConnectGoogleCalendar(string code, string state)
        {
            var result = await _accountApplicationService.ConnectGoogleAccountAsync(code, state);

            var payload = _stateService.Validate(state);

            if (result.Success)
                return Redirect(payload.RedirectUri ?? _configuration["Frontend:ConnectCalenderRedirectUri"] ?? "http://localhost:3000/setup");

            return result.ToActionResult(this);
        }

    }
}

