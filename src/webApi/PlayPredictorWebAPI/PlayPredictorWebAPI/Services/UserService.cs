using g_map_compare_backend.Common.Results;
using g_map_compare_backend.Data;
using g_map_compare_backend.Dtos.User;
using PlayPredictorWebAPI.Models;
using Microsoft.EntityFrameworkCore;
using PlayPredictorWebAPI.Common.Results;
using PlayPredictorWebAPI.Common.Utils;
using PlayPredictorWebAPI.Dtos.User;
using PlayPredictorWebAPI.Services;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace g_map_compare_backend.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public UserService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<User>> CreateUserAsync(UserCreateDto dto)
        {
            if (!ValidationService.IsValidEmail(dto.Email))
                return Result<User>.Fail(new ErrorMessage { Type = ErrorType.REGISTER_FAIL, Details = "The email is not in a valid format." }, ErrorCode.ValidationError);

            if (!ValidationService.IsValidPassword(dto.Password) && dto.AuthProvider == AuthProvider.Password)
                return Result<User>.Fail(new ErrorMessage { Type = ErrorType.REGISTER_FAIL, Details = "The password must be at least 6 characters long." }, ErrorCode.ValidationError);

            var potentialUser = await GetUserByEmailAsync(dto.Email);
            if (potentialUser.Success)
                return Result<User>.Fail(new ErrorMessage { Type = ErrorType.REGISTER_FAIL, Details = "The given email already exists." }, ErrorCode.Conflict);

            var user = new User
            {
                UserName = dto.UserName,
                Email = dto.Email,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = dto.AuthProvider == AuthProvider.Password ?  PasswordHasher.HashPasswordForRegistration(dto.Password) : null,
                GoogleId = dto.AuthProvider == AuthProvider.Google ? dto.GoogleID : null,
                AuthProvider = dto.AuthProvider
            };
           
            _context.Users.Add(user);

            return await _context.SaveChangesAsync() > 0
                ? Result<User>.Ok(user)
                : Result<User>.Fail(new ErrorMessage { Type = ErrorType.REGISTER_FAIL, Details = "User could not be created" }, ErrorCode.UnknownError);
        }

        public async Task<Result<User>> UpdateUserAsync(User user)
        {
            _context.Users.Update(user);

            return await _context.SaveChangesAsync() > 0
                ? Result<User>.Ok(user)
                : Result<User>.Fail(new ErrorMessage { Type = ErrorType.UNKNOWN, Details = "User could not be updated" }, ErrorCode.UnknownError);
        }

        public async Task<Result> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);

            var permissionResult = await HasUserPermittionAsync(user);
            if (!permissionResult.Success)
                return Result.Fail(permissionResult.ErrorMessage, permissionResult.ErrorCode);

            if (user == null)
                return Result.Fail(new ErrorMessage { Type = ErrorType.USER_NOT_FOUND, Details = "The user could not be deleted" }, ErrorCode.NotFound);

            _context.Users.Remove(user);

            return await _context.SaveChangesAsync() > 0
                ? Result.Ok()
                : Result.Fail(new ErrorMessage { Type = ErrorType.UNKNOWN, Details = "Failed to delete the user." }, ErrorCode.UnknownError);
        }

        public async Task<Result<IEnumerable<User>>> GetUsersAsync()
        {
            var users = await _context.Users.ToListAsync();
            return Result<IEnumerable<User>>.Ok(users);
        }

        public async Task<Result<User>> GetLoggedInUserAsync()
        {
            var email = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
            if (email == null)
                return Result<User>.Fail(new ErrorMessage { Type = ErrorType.LOGIN_FAIL, Details = "The Session Token is not valid" }, ErrorCode.Unauthorized);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return Result<User>.Fail(new ErrorMessage { Type = ErrorType.USER_NOT_FOUND, Details = "User not found." }, ErrorCode.NotFound);

            return Result<User>.Ok(user);
        }

        public async Task<Result<User>> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return Result<User>.Fail(new ErrorMessage { Type = ErrorType.USER_NOT_FOUND, Details = "User not found." }, ErrorCode.NotFound);
            return Result<User>.Ok(user);
        }

        public async Task<Result<User>> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return Result<User>.Fail(new ErrorMessage { Type = ErrorType.USER_NOT_FOUND, Details = "User not found." }, ErrorCode.NotFound);
            return Result<User>.Ok(user);
        }

        public async Task<Result> HasUserPermittionAsync(User user)
        {
            var loggedInUserResult = await GetLoggedInUserAsync();
            if (!loggedInUserResult.Success)
                return Result.Fail(new ErrorMessage { Type = ErrorType.LOGIN_FAIL, Details = "You need to be logged in to recieve user data" }, ErrorCode.Unauthorized);

            var loggedInUser = loggedInUserResult.Data;

            if (user.Id != loggedInUser.Id && user.Role != UserRole.Admin)
                return Result.Fail(new ErrorMessage { Type = ErrorType.ACCESS_NOT_ALLOWED, Details = "You do not have permission to perform this action." }, ErrorCode.Forbidden);

            return Result.Ok();
        }

        public async Task<Result> HasUserPermittion(int userId)
        {
            var userResult = await GetUserByIdAsync(userId);
            if (!userResult.Success)
                return Result.Fail(new ErrorMessage { Type = ErrorType.USER_NOT_FOUND, Details = "User not found." }, ErrorCode.NotFound);
            return await HasUserPermittionAsync(userResult.Data);
        }

        public async Task<Result<User>> GetUserByRefreshToken(string refreshToken)
        {
            var token = await _context.RefreshTokens
                .Include(t => t.User)
                .SingleOrDefaultAsync(t => t.Token == refreshToken);

            if (token == null)
                return Result<User>.Fail(new ErrorMessage { Type = ErrorType.USER_NOT_FOUND, Details = "Refreshtoken not found" }, ErrorCode.Unauthorized);

            var user = token.User;

            if (user == null)
                return Result<User>.Fail(new ErrorMessage { Type = ErrorType.USER_NOT_FOUND, Details = "Refreshtoken invalid" }, ErrorCode.Unauthorized);

            return Result<User>.Ok(user);
        }

        public async Task<Result> DeleteCurrentUserAsync()
        {
            var user = await GetLoggedInUserAsync();
            if (!user.Success)
                return Result.Fail(user.ErrorMessage, user.ErrorCode);

            var res = await DeleteUserAsync(user.Data.Id);
            if (!res.Success)
                return Result.Fail(res.ErrorMessage, res.ErrorCode);

            return Result.Ok();

        }

    }
}
