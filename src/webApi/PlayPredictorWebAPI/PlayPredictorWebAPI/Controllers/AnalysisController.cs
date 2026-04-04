using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayPredictorWebAPI.Dtos.Analysis;
using PlayPredictorWebAPI.Dtos.Auth;
using PlayPredictorWebAPI.Services;

namespace PlayPredictorWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : Controller
    {
        private readonly CalendarService _calendarService;
        private readonly FaceitService _faceitService;
        private readonly AnalysisService _analysisService;

        public AnalysisController(CalendarService calendarService, FaceitService faceitService, AnalysisService analysisService)
        {
            _calendarService = calendarService;
            _faceitService = faceitService;
            _analysisService = analysisService;
        }

        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> StartAnalysis(
            [FromQuery] DateTimeOffset? startDate = null,
            [FromQuery] DateTimeOffset? endDate = null)
        {
            var start = startDate ?? DateTimeOffset.UnixEpoch;
            var end = endDate ?? DateTimeOffset.UtcNow;

            var matches = await _faceitService.GetMatchesAsync(start, end);
            var events = await _calendarService.GetAllUserCallendarEventsAsync(start, end);

            return Ok(new
            {
                matches = matches.ErrorMessage,
                events = events.ErrorMessage
            });
        }

        [Authorize]
        [ProducesResponseType(typeof(MatchAnalysis), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [HttpGet("match")]
        public async Task<ActionResult<MatchAnalysis>> GetMatchAnalysisAsync(
            [FromQuery] DateTimeOffset? startDate = null,
            [FromQuery] DateTimeOffset? endDate = null)
        {
            var start = startDate ?? DateTimeOffset.UnixEpoch;
            var end = endDate ?? DateTimeOffset.UtcNow;

            var matchAnalysis = await _analysisService.AnalyzeMatchesAsync(start, end);

            return matchAnalysis.ToActionResult(this);
        }


        [Authorize]
        [ProducesResponseType(typeof(SessionAnalysis), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [HttpGet("session")]
        public async Task<ActionResult<SessionAnalysis>> GetSessionAnalysisAsync(
            [FromQuery] DateTimeOffset? startDate = null,
            [FromQuery] DateTimeOffset? endDate = null)
        {
            var start = startDate ?? DateTimeOffset.UnixEpoch;
            var end = endDate ?? DateTimeOffset.UtcNow;
            var sessionAnalysis = await _analysisService.AnalyzeSessionsAsync(start, end);
            return sessionAnalysis.ToActionResult(this);
        }

        [Authorize]
        [ProducesResponseType(typeof(PerformanceAnalysis), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [HttpGet("performance")]
        public async Task<ActionResult<PerformanceAnalysis>> GetPerformanceAnalysisAsync(
            [FromQuery] DateTimeOffset? startDate = null,
            [FromQuery] DateTimeOffset? endDate = null)
        {
            var start = startDate ?? DateTimeOffset.UnixEpoch;
            var end = endDate ?? DateTimeOffset.UtcNow;
            var performanceAnalysis = await _analysisService.AnalyzePerformanceAsync(start, end);
            return performanceAnalysis.ToActionResult(this);

        }

        [Authorize]
        [ProducesResponseType(typeof(CalendarAnalysis), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [HttpGet("calendar")]
        public async Task<ActionResult<CalendarAnalysis>> GetCalendarAnalysisAsync(
            [FromQuery] DateTimeOffset? startDate = null,
            [FromQuery] DateTimeOffset? endDate = null)
        {
            var start = startDate ?? DateTimeOffset.UnixEpoch;
            var end = endDate ?? DateTimeOffset.UtcNow;
            var calendarAnalysis = await _analysisService.AnalyzeCalendarAsync(start, end);
            return calendarAnalysis.ToActionResult(this);
        }
    }
}
