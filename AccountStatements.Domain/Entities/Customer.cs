using System.ComponentModel.DataAnnotations;
using AccountStatements.Domain.Common;

namespace AccountStatements.Domain.Entities
{
    public class Customer : BaseEntity
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}
