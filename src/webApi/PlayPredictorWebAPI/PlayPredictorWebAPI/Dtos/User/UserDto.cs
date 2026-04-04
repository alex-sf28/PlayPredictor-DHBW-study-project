namespace g_map_compare_backend.Dtos.User
{
    public class UserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        public DateTime CreatedAt { get; internal set; }
    }
}
