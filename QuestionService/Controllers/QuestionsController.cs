using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionService.Data;
using QuestionService.DTOs;
using QuestionService.Models;
using System.Security.Claims;

namespace QuestionService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QuestionsController(QuestionDbContext dbContext) : ControllerBase
    {

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Question>> CreateQuestion(CreateQuestionDto dto)
        {
            var validTags = await dbContext.Tags
                .Where(t => dto.Tags.Contains(t.Slug))
                .Select(t => t.Slug)
                .ToListAsync();

            var missing = dto.Tags.Except(validTags).ToList();
            if (missing.Count != 0)
                return BadRequest($"These tags are invalid: {string.Join(", ", missing)}");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue("name");

            if (userId is null || userName is null)
                return BadRequest("Cannot get user details");

            var question = new Question
            {
                AskerId = userId,
                AskerDisplayName = userName,
                Title = dto.Title,
                Content = dto.Content,
                TagSlugs = dto.Tags
            };

            dbContext.Questions.Add(question);
            await dbContext.SaveChangesAsync();

            return Created($"/questions/{question.Id}", question);
        }

        [HttpGet]
        public async Task<ActionResult<List<Question>>> GetQuestions(string? tag)
        {

            var query = dbContext.Questions.AsQueryable();
            if (!string.IsNullOrEmpty(tag))
            {
                query = query.Where(x => x.TagSlugs.Contains(tag));
            }

            return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Question>> GetQuestion(string id)
        {
            var question = await dbContext.Questions.FindAsync(id);
            if (question is null) return NotFound();

            await dbContext.Questions.Where(x => x.Id == id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ViewCount, x => x.ViewCount + 1));

            return question;
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateQuestion(string id, UpdateQuestionDto dto)
        {

            var question = await dbContext.Questions.FindAsync(id);
            if (question is null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != question.AskerId) return Forbid();

            var validTags = await dbContext.Tags
            .Where(t => dto.Tags.Contains(t.Slug))
            .Select(t => t.Slug)
            .ToListAsync();

            var missing = dto.Tags.Except(validTags).ToList();
            if (missing.Count != 0)
                return BadRequest($"These tags are invalid: {string.Join(", ", missing)}");

            question.Title = dto.Title;
            question.Content = dto.Content;
            question.UpdatedAt = DateTime.UtcNow;
            question.TagSlugs = dto.Tags;
            await dbContext.SaveChangesAsync();
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteQuestion(string id)
        {
            var question = await dbContext.Questions.FindAsync(id);
            if (question is null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != question.AskerId) return Forbid();
            dbContext.Questions.Remove(question);
            await dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
