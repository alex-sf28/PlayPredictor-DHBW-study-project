using AutoMapper;
using g_map_compare_backend.Common.Results;
using g_map_compare_backend.Data;
using g_map_compare_backend.Dtos.Auth;
using g_map_compare_backend.Dtos.User;
using PlayPredictorWebAPI.Models;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;
using PlayPredictorWebAPI.Common.Results;
using PlayPredictorWebAPI.Common.Utils;
using PlayPredictorWebAPI.Dtos.Auth;
using PlayPredictorWebAPI.Dtos.User;
using PlayPredictorWebAPI.Services;
using PlayPredictorWebAPI.Services.External;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace g_map_compare_backend.Services
{
    public class AuthService
    {
        private readonly UserService _userService;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly IGoogleApiClient _googleApiClient;
        private readonly OAuthStateService _stateService;

        public AuthService(UserService userService, 
            IMapper mapper, AppDbContext context, 
            TokenService tokenService, 
            IConfiguration configuration, 
            IGoogleApiClient googleApiClient,
            OAuthStateService stateService)
        {
            _userService = userService;
            _mapper = mapper;
            _context = context;
            _tokenService = tokenService;
            _configuration = configuration;
            _googleApiClient = googleApiClient;
            _stateService = stateService;
        }

        public async Task<LoginServiceResponseDto> LoginAsync(LoginRequestDto dto)
        {
            var result = await _userService.GetUserByEmailAsync(dto.Email);
            if (!result.Success)
                return new LoginServiceResponseDto(Result<LoginResponseDto>.Fail(new ErrorMessage { Type = ErrorType.LOGIN_FAIL, Details = "The email or password is incorrect." }, ErrorCode.Unauthorized), null);

            if (result.Data.AuthProvider != AuthProvider.Password)
                return new LoginServiceResponseDto(Result<LoginResponseDto>.Fail(new ErrorMessage { Type = ErrorType.LOGIN_FAIL, Details = "The email or password is incorrect." }, ErrorCode.Unauthorized), null);

            var user = result.Data;
            if (!PasswordHasher.IsCorrectPassword(dto.Password, user))
                return new LoginServiceResponseDto(Result<LoginResponseDto>.Fail(new ErrorMessage { Type = ErrorType.LOGIN_FAIL, Details = "The email or password is incorrect." }, ErrorCode.Unauthorized), null);

            return await CreateAuthResponseAsync(user);
        }

        internal async Task<LoginServiceResponseDto> RegisterAsync(UserRegisterRequestDto dto)
        {
            var userCreateDto = _mapper.Map<UserRegisterRequestDto, UserCreateDto>(dto);
            userCreateDto.AuthProvider = AuthProvider.Password;

            var result = await _userService.CreateUserAsync(userCreateDto);
            if (!result.Success)
                return new LoginServiceResponseDto(Result<LoginResponseDto>.Fail(result.ErrorMessage, result.ErrorCode), null);

            var user = result.Data;
            return await CreateAuthResponseAsync(user);
        }



        public async Task<Result<LoginResponseDto>> RefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return Result<LoginResponseDto>.Fail(new ErrorMessage { Type = ErrorType.LOGIN_FAIL, Details = "No RefreshToken provided" }, ErrorCode.Unauthorized);

            var result = await _userService.GetUserByRefreshToken(refreshToken);

            if (!result.Success)
                return Result<LoginResponseDto>.Fail(new ErrorMessage { Type = ErrorType.LOGIN_FAIL, Details = "Token is invalid, login manually" }, ErrorCode.Unauthorized);

            var user = result.Data;

            var storedToken = user.RefreshTokens.Single(x => x.Token == refreshToken);

            if (!storedToken.IsActive) return Result<LoginResponseDto>.Fail(new ErrorMessage { Type = ErrorType.LOGIN_FAIL, Details = "RefreshToken invalid" }, ErrorCode.Unauthorized);

            var accessToken = _tokenService.GenerateJwtToken(user);

            return Result<LoginResponseDto>.Ok(new LoginResponseDto { Token = accessToken, User = _mapper.Map<User, UserDto>(user) });
        }

        public async Task<Result> LogoutAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return Result.Ok();

            var result = await _userService.GetUserByRefreshToken(refreshToken);

            if (!result.Success)
                return Result.Ok();

            var user = result.Data;

            var storedToken = user.RefreshTokens.Single(x => x.Token == refreshToken);

            storedToken.Revoked = DateTime.UtcNow;

            return await _context.SaveChangesAsync() > 0
                ? Result.Ok()
                : Result.Fail(new ErrorMessage { Type = ErrorType.UNKNOWN, Details = "A uknownn error accured while logging out." }, ErrorCode.UnknownError);
        }

        public async Task<Result> ChangePassword(UserUpdatePasswordDto dto)
        {
            var userResult = await _userService.GetLoggedInUserAsync();
            if (!userResult.Success)
                return Result.Fail(userResult.ErrorMessage, userResult.ErrorCode);
            var user = userResult.Data;

            if (!PasswordHasher.IsCorrectPassword(dto.CurrentPassword, user))
                return Result.Fail(new ErrorMessage { Type = ErrorType.PASSWORD_INCORRECT, Details = "The current password is incorrect." }, ErrorCode.Unauthorized);

            if (!ValidationService.IsValidPassword(dto.NewPassword))
                return Result.Fail(new ErrorMessage { Type = ErrorType.PASSWORD_INCORRECT, Details = "The new password must be at least 6 characters long." }, ErrorCode.ValidationError);

            user.PasswordHash = PasswordHasher.HashPasswordForRegistration(dto.NewPassword);

            var tokenRes = await _tokenService.InvalidateAllRefreshTokensAsync(user);
            if (!tokenRes.Success)
                return Result.Fail(tokenRes.ErrorMessage, tokenRes.ErrorCode);

            var res = await _userService.UpdateUserAsync(user);

            return res.Success
                ? Result.Ok()
                : Result.Fail(new ErrorMessage { Type = ErrorType.UNKNOWN, Details = "Password could not be changed." }, ErrorCode.UnknownError);
        }

        public async Task<LoginServiceResponseDto> LoginWithGoogleAsync(string tokenId)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(tokenId, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _configuration["Google:ClientId"] }
            });

            if (payload.Email == null)
                return new LoginServiceResponseDto(Result<LoginResponseDto>.Fail(new ErrorMessage { Type = ErrorType.LOGIN_FAIL, Details = "Token is not valid" }, ErrorCode.Unauthorized), null);

            var userResult = await _userService.GetUserByEmailAsync(payload.Email);



            if (userResult.ErrorCode == ErrorCode.NotFound)
            {
                var userCreateDto = new UserCreateDto
                {
                    Email = payload.Email,
                    AuthProvider = AuthProvider.Google,
                    GoogleID = payload.Subject,
                    UserName = payload.Name
                };

                var user = await _userService.CreateUserAsync(userCreateDto);

                if (user.Success)
                    return await CreateAuthResponseAsync(user.Data);

                return new LoginServiceResponseDto(Result<LoginResponseDto>.Fail(new ErrorMessage { Type = ErrorType.LOGIN_FAIL, Details = user.ErrorMessage.Details }, ErrorCode.Unauthorized), null);
            }

            if (!userResult.Success || userResult.Data == null)
                return new LoginServiceResponseDto(Result<LoginResponseDto>.Fail(new ErrorMessage { Type = ErrorType.LOGIN_FAIL, Details = "Token is not valid" }, ErrorCode.Unauthorized), null);

            if (userResult.Data.AuthProvider != AuthProvider.Google)
                return new LoginServiceResponseDto(Result<LoginResponseDto>.Fail(new ErrorMessage { Type = ErrorType.LOGIN_FAIL, Details = "This email is already linked to a password login." }, ErrorCode.Unauthorized), null);

            return await CreateAuthResponseAsync(userResult.Data);
        }

        public async Task<LoginServiceResponseDto> CreateAuthResponseAsync(User user) {

            var token = _tokenService.GenerateJwtToken(user);

            var refreshToken = _tokenService.CreateRefreshToken();

            user.RefreshTokens.Add(refreshToken);
            await _userService.UpdateUserAsync(user);

            return new LoginServiceResponseDto(Result<LoginResponseDto>.Ok(new LoginResponseDto
            {
                Token = token,
                User = _mapper.Map<User, UserDto>(user)
            }), refreshToken);
        }

        public async Task<Result<AuthProviderResponseDto>> GetAuthProviderAsync()
        {
            var user = await _userService.GetLoggedInUserAsync();

            if (!user.Success)
                return Result<AuthProviderResponseDto>.Fail(user.ErrorMessage, user.ErrorCode);

            return Result<AuthProviderResponseDto>.Ok(new AuthProviderResponseDto { AuthProvider = user.Data.AuthProvider });
        }


    }
}

  

