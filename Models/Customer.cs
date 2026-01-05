using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Securitas8._8.Models
{
    public class Customer : IValidatableObject
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = "";
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = "";
        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; } = "";

        public Address Address { get; set; } = new();
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            if (Address == null)
            {
                results.Add(new ValidationResult(
                    "Address is required",
                    new[] { nameof(Address) }));
            }
            else
            {
                Validator.TryValidateObject(
                    Address,
                    new ValidationContext(Address),
                    results,
                    validateAllProperties: true);
            }

            return results;
        }
    }

    public class Address
    {
        [Required(ErrorMessage = "Street address is required")]
        public string Street { get; set; } = "";
        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = "";
        [Required(ErrorMessage = "State is required")]
        public string State { get; set; } = "";
        [Required(ErrorMessage = "Postal code is required")]
        public string PostalCode { get; set; } = "";
        [Required(ErrorMessage = "Country is required")]
        public string Country { get; set; } = "";
    }
}
