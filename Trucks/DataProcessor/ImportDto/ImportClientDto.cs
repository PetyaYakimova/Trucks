using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Trucks.Data.Models;

namespace Trucks.DataProcessor.ImportDto
{
    public class ImportClientDto
    {
        [Required]
        [MaxLength(40)]
        [MinLength(3)]
        [JsonProperty("Name")]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(40)]
        [MinLength(2)]
        [JsonProperty("Nationality")]
        public string Nationality { get; set; } = null!;

        [Required]
        [JsonProperty("Type")]
        public string Type { get; set; } = null!;

        [JsonProperty("Trucks")]
        [NotMapped]
        public List<int> TruckIds { get; set; } = new List<int>();
    }
}
