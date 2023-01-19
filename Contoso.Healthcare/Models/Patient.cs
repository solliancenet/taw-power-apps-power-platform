using System.Text.Json.Serialization;

namespace Contoso.Healthcare.Models
{
    public class Patient
    {
        [JsonPropertyName("id")]
        public string id { get; set; } //This property has to be lowercase "id" because of a bug in CosmosDB
        public string type="patient";
        [JsonPropertyName("patientid")]
        public string PatientId { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
