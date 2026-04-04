using PlayPredictorWebAPI.Models;
using AutoMapper;
using g_map_compare_backend.Dtos;
using g_map_compare_backend.Dtos.User;
using PlayPredictorWebAPI.Dtos.User;
using PlayPredictorWebAPI.Dtos.google_calendar;
using PlayPredictorWebAPI.Models;
using PlayPredictorWebAPI.Dtos.GoogleCalendar;


namespace g_map_compare_backend.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Domain -> DTO
            CreateMap<User, UserDto>();
            CreateMap<UserRegisterRequestDto, UserCreateDto>();
            CreateMap<Calendar, ActiveCalendarDto>();
            CreateMap<CalendarEvent, CalendarEventDto>();

            //// DTO -> Domain (optional)
            //CreateMap<UserCreateDto, User>();
            //CreateMap<UserUpdateDto, User>();
        }
    }
}
