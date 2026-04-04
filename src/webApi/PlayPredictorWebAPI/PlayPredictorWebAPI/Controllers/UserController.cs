using AutoMapper;
using g_map_compare_backend.Data;
using g_map_compare_backend.Dtos.Auth;
using g_map_compare_backend.Dtos.User;
using PlayPredictorWebAPI.Models;
using g_map_compare_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayPredictorWebAPI.Dtos.User;

namespace g_map_compare_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IMapper _mapper;

        public UsersController(UserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var result = await _userService.GetLoggedInUserAsync();
            return result.ToActionResult<User, UserDto>(this, _mapper);
        }

        [Authorize]
        [HttpDelete("me")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> DeleteMe()
        {
            var result = await _userService.DeleteCurrentUserAsync();

            Response.Cookies.Delete("refreshToken");
            return result.ToActionResult(this);
        }

    }
}
