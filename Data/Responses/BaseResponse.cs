using System.Text.Json.Serialization;

namespace TaskApi.Data.Responses
{
    public class BaseResponse
    {
        [JsonIgnore]
        public bool Success { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string ErrorCode { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Error { get; set; }
    }
}