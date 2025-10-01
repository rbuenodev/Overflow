using QuestionService.Validators;
using System.ComponentModel.DataAnnotations;

namespace QuestionService.DTOs
{
    public record UpdateQuestionDto(
        [Required] string Title,
        [Required] string Content,
        [Required][TagListValidator(1, 5)] List<string> Tags
        );
}
