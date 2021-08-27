namespace Tindo.Ugit.Server.Models
{
    using System.ComponentModel.DataAnnotations;

    public class RepositoryModel
    {
        [Required(AllowEmptyStrings = false)]
        [StringLength(20, ErrorMessage = "The max length is 20")]
        [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Space character doesn't allowed")]
        public string Name { get; set; }
        public string Description { get; set; }
        public string Language { get; set; } = "Unknown";
    }
}
