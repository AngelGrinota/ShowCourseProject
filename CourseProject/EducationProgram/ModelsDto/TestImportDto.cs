using System.ComponentModel.DataAnnotations;

namespace EducationProgram.Models
{
    public class TestImportDto
    {
        [Required]
        public string QuestionText { get; set; } = string.Empty;
        [Required]
        public int Points { get; set; }
        [Required]
        public string AnswerText { get; set; } = string.Empty;
        [Required]
        public bool IsCorrect { get; set; }
    }
}