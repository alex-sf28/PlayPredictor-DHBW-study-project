using System.ComponentModel;

namespace PlayPredictorWebAPI.Common.Results
{
    public class ErrorMessage
    {
        public required ErrorType Type { get; set; }

        public string? Details { get; set; }

        public static string GetDescription(ErrorType value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attr = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attr?.Description ?? value.ToString();
        }
    }
}
