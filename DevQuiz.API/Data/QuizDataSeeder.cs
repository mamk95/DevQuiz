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

        // Create two quizzes
        var quizNoob = new Quiz { Name = "Noob Quiz", Difficulty = "Noob" };
        var quizNerd = new Quiz { Name = "Nerd Quiz", Difficulty = "Nerd" };
        db.Quizzes.AddRange(quizNoob, quizNerd);
        await db.SaveChangesAsync();

        // Link first 3 questions to Noob, remaining 4 to Nerd
        var quizQuestions = new List<QuizQuestion>();
        for (int i = 0; i < questions.Count; i++)
        {
            if (i < 3)
            {
                quizQuestions.Add(new QuizQuestion
                {
                    QuizId = quizNoob.Id,
                    QuestionId = questions[i].Id,
                    Sequence = i + 1
                });
            }
            else
            {
                quizQuestions.Add(new QuizQuestion
                {
                    QuizId = quizNerd.Id,
                    QuestionId = questions[i].Id,
                    Sequence = (i - 3) + 1
                });
            }
        }
        db.QuizQuestions.AddRange(quizQuestions);
        await db.SaveChangesAsync();
    }
}