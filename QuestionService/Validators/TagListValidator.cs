using System.ComponentModel.DataAnnotations;

namespace QuestionService.Validators
{
    public class TagListValidator(int min, int max) : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not List<string> tags)
                return new ValidationResult("Tags must be a list of strings");
            if (tags.Count < min || tags.Count > max)
                return new ValidationResult($"Number of tags must be between {min} and {max}");
            foreach (var tag in tags)
            {
                if (string.IsNullOrWhiteSpace(tag))
                    return new ValidationResult("Tags cannot be empty or whitespace");
                if (tag.Length > 30)
                    return new ValidationResult("Each tag must be at most 30 characters long");
            }
            return ValidationResult.Success;
        }
    }
}
