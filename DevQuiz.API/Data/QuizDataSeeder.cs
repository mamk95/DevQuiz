namespace DevQuiz.API.Data;
using System.Text.Json;
using DevQuiz.API.Entities;
using Microsoft.EntityFrameworkCore;

public static class QuizDataSeeder
{
    public static async Task SeedQuestionsAsync(QuizDbContext db)
    {
        if (await db.Questions.AnyAsync())
            return;

        var questions = new List<Question>
        {
            new()
            {
                Sequence = 1,
                Type = QuestionType.CodeFix,
                Prompt = "Fix the string so the check passes.",
                CorrectAnswer = "const text = \"hello world\";",
                ChoicesJson = JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["initialCode"] = "const text = \"hello wrld\";",
                    ["testCode"] = "assert(text === \"hello world\");",
                }),
            },
            new()
            {
                Sequence = 2,
                Type = QuestionType.MultipleChoice,
                Prompt = "What is the output of `for (var x in ['a','b','c']) console.log(x)`?",
                CorrectAnswer = "0, 1, 2",
                ChoicesJson = JsonSerializer.Serialize(new List<string>
                {
                    "a, b, c",
                    "0, 1, 2",
                    "compile-time error",
                    "runtime error",
                }),
            },
            new()
            {
                Sequence = 3,
                Type = QuestionType.MultipleChoice,
                Prompt = "What does `console.log('5' + 3 - 2)` print?",
                CorrectAnswer = "51",
                ChoicesJson = JsonSerializer.Serialize(new List<string>
                {
                    "53",
                    "6",
                    "51",
                    "NaN",
                }),
            },
            new()
            {
                Sequence = 4,
                Type = QuestionType.MultipleChoice,
                Prompt = "Which HTTP status code means 'Too Many Requests'?",
                CorrectAnswer = "429",
                ChoicesJson = JsonSerializer.Serialize(new List<string>
                {
                    "404",
                    "409",
                    "429",
                    "503",
                }),
            },
            new()
            {
                Sequence = 5,
                Type = QuestionType.MultipleChoice,
                Prompt = "Which command shows the commit history?",
                CorrectAnswer = "git log",
                ChoicesJson = JsonSerializer.Serialize(new List<string>
                {
                    "git log",
                    "git status",
                    "git diff",
                    "git branch",
                }),
            },
            new()
            {
                Sequence = 6,
                Type = QuestionType.MultipleChoice,
                Prompt = "Which range equals >=1.2.3 <2.0.0?",
                CorrectAnswer = "^1.2.3",
                ChoicesJson = JsonSerializer.Serialize(new List<string>
                {
                    "^1.2.3",
                    "~1.2.3",
                    "1.x",
                    ">=1.2.3 <1.3.0",
                }),
            },
            new()
            {
                Sequence = 7,
                Type = QuestionType.MultipleChoice,
                Prompt = "Which HTTP method is used for a CORS preflight?",
                CorrectAnswer = "OPTIONS",
                ChoicesJson = JsonSerializer.Serialize(new List<string>
                {
                    "GET",
                    "POST",
                    "OPTIONS",
                    "HEAD",
                }),
            },
        };

        db.Questions.AddRange(questions);
        await db.SaveChangesAsync();
    }
}