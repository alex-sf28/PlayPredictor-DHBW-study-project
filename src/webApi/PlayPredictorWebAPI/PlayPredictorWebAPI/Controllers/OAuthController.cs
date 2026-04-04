using AutoMapper;
using Google.Apis.Calendar.v3.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using PlayPredictorWebAPI.Dtos.Faceit;
using PlayPredictorWebAPI.Dtos.google_calendar;
using PlayPredictorWebAPI.Dtos.GoogleCalendar;
using PlayPredictorWebAPI.Services;

namespace PlayPredictorWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OAuthController : Controller
    {
        private readonly OAuthService _oAuthService;
        private readonly IMapper _mapper;
        private readonly CalendarService _calendarService;
        private readonly AccountApplicationService _accountApplicationService;
        private readonly FaceitService _faceitService;

        public OAuthController(OAuthService oAuthService, IMapper mapper, CalendarService calendarService, AccountApplicationService accountApplicationService, FaceitService faceitService)
        {
            _oAuthService = oAuthService;
            _mapper = mapper;
            _calendarService = calendarService;
            _accountApplicationService = accountApplicationService;
            _faceitService = faceitService;
        }

        [Authorize]
        [HttpGet("google-calendar/calendars")]
        [ProducesResponseType(typeof(ICollection<UserCalendarResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ICollection<UserCalendarResponseDto>>> GetCalendars()
        {
            var res = await _calendarService.GetUserCalendars();

            return res.ToActionResult(this);
        }

        [Authorize]
        [HttpPut("google-calendar/calendars/active")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> SetActiveCalendar(ICollection<ActiveCalendarDto> dtos)
        {
            var res = await _calendarService.SetActiveCalendars(dtos);

            return res.ToActionResult(this);
        }

        [Authorize]
        [HttpGet("google-calendar/calendars/active")]
        [ProducesResponseType(typeof(ICollection<ActiveCalendarDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ICollection<ActiveCalendarDto>>> GetActiveCalendar()
        {
            var res = await _calendarService.GetActiveCalendar();
            return res.ToActionResult<Models.Calendar, ActiveCalendarDto>(this, _mapper);
        }

        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [HttpGet("google-calendar/state")]
        public async Task<ActionResult> GetGoogleCalendarOAuthState()
        {
            var result = await _oAuthService.HasUserGoogleCalendar();

            return result.ToActionResult(this);
        }

        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [HttpDelete("google-calendar")]
        public async Task<ActionResult> DisconnectGoogleCalendar()
        {
            var res = await _accountApplicationService.RemoveGoogleAccount();

            return res.ToActionResult(this);
        }

        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [HttpPost("google-calendar/calendars")]
        public async Task<ActionResult> AddGoogleCalendarByICalenderFile(List<IFormFile> files)
        {
            var res = await _calendarService.AddCalendarsFromFile(files);
            return res.ToActionResult(this);
        }

        [Authorize]
        [ProducesResponseType(typeof(ICollection<PlayerDto>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [HttpGet("faceit/players/search")]
        public async Task<ActionResult<ICollection<PlayerDto>>> SearchFaceitPlayers([FromQuery] string nickname, [FromQuery] int offset = 0, [FromQuery] int limit = 10)
        {
            var res = await _faceitService.SearchFaceitPlayersAsync(nickname, offset, limit);
            return res.ToActionResult(this);
        }

        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [HttpPut("faceit/players/{id}")]
        public async Task<ActionResult> ChangeFacitAccount(string id)
        {
            var res = await _faceitService.UpdateFaceitAccountAsync(id);
            return res.ToActionResult(this);
        }

        [Authorize]
        [ProducesResponseType(typeof(PlayerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [HttpGet("faceit/players")]
        public async Task<ActionResult<PlayerDto>> GetFaceitAccount()
        {
            var res = await _faceitService.GetUserPlayerAccount();
            return res.ToActionResult(this);
        }

        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [HttpDelete("faceit/players")]
        public async Task<ActionResult> DisconnectFaceitAccount()
        {
            var res = await _faceitService.DisconnectFaceitAccountAsync();
            return res.ToActionResult(this);
        }

        [Authorize]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [HttpGet("faceit/matches/state")]
        public async Task<ActionResult<bool>> HasUserSavedMatches()
        {
            var res = await _faceitService.HasUserSavedMatches();

            return res.ToActionResult(this);
        }

        
    }
}
