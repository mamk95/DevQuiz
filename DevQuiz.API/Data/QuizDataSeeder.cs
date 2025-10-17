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
            // NOOB QUESTIONS (first 6)
            new()
            {
                Type = QuestionType.MultipleChoice,
                Prompt = "What's the main goal of UX design?",
                CorrectAnswer = "Better user experience",
                ChoicesJson = JsonSerializer.Serialize(new List<string>
                {
                    "More complex interface",
                    "Better user experience",
                    "Confusing navigation",
                    "Increased server load",
                }),
            },
            new()
            {
                Type = QuestionType.MultipleChoice,
                Prompt = "What is \"rubber duck debugging\"?",
                CorrectAnswer = "Talking to a duck to find bugs in your code",
                ChoicesJson = JsonSerializer.Serialize(new List<string>
                {
                    "Talking to a duck to find bugs in your code",
                    "Encrypting with duck-themed algorithms",
                    "Using a duck-shaped USB for secure storage",
                    "Debugging code underwater",
                }),
            },
            new()
            {
                Type = QuestionType.MultipleChoice,
                Prompt = "How can you design an app for users with visual impairments?",
                CorrectAnswer = "Using high contrast and colour-blind friendly colours",
                ChoicesJson = JsonSerializer.Serialize(new List<string>
                {
                    "Using bright colors and animations",
                    "Using complex navigation",
                    "Using high contrast and colour-blind friendly colours",
                    "Adding lots of small text and icons",
                }),
            },
            new()
            {
                Type = QuestionType.MultipleChoice,
                Prompt = "What is the purpose of two-factor authentication?",
                CorrectAnswer = "To add an extra layer of security",
                ChoicesJson = JsonSerializer.Serialize(new List<string>
                {
                    "To speed up login",
                    "To reset your password",
                    "To add an extra layer of security",
                    "Nothing, it's just a buzzword",
                }),
            },
            new()
            {
                Type = QuestionType.MultipleChoice,
                Prompt = "Who is known for being the first computer programmer?",
                CorrectAnswer = "Ada Lovelace",
                ChoicesJson = JsonSerializer.Serialize(new List<string>
                {
                    "Ada Lovelace",
                    "Mark Zuckerberg",
                    "Alan Turing",
                    "Marie Curie",
                }),
            },
            new()
            {
                Type = QuestionType.MultipleChoice,
                Prompt = "Which of these is NOT a Capgemini value?",
                CorrectAnswer = "Baldness",
                ChoicesJson = JsonSerializer.Serialize(new List<string>
                {
                    "Team-spirit",
                    "Fun",
                    "Boldness",
                    "Baldness",
                }),
            },
            // NERD QUESTIONS (remaining 8)
            new()
            {
                Type = QuestionType.MultipleChoice,
                Prompt = "What is the output of unknown_func(2,3)?\n\n```python\ndef unknown_func(a, b):\n    return a + b\n```",
                CorrectAnswer = "5",
                ChoicesJson = JsonSerializer.Serialize(new List<string>
                {
                    "2",
                    "5",
                    "-1",
                    "6",
                }),
            },
            new()
            {
                Type = QuestionType.MultipleChoice,
                Prompt = "Which of the following is a common penetration testing tool?",
                CorrectAnswer = "Wireshark",
                ChoicesJson = JsonSerializer.Serialize(new List<string>
                {
                    "Pyjamashark",
                    "GreatWhiteShark",
                    "Babyshark",
                    "Wireshark",
                }),
            },
            new()
            {
                Type = QuestionType.CodeFix,
                Prompt = "Fix the string so the check passes.",
                CorrectAnswer = "const text = \"hello world\";",
                ChoicesJson = JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["initialCode"] = "const text = \"hello\";",
                    ["testCode"] = "assert(text === \"hello world\");",
                }),
            },
            new()
            {
                Type = QuestionType.MultipleChoice,
                Prompt = "What is cloud storage?",
                CorrectAnswer = "A method to save files on the internet",
                ChoicesJson = JsonSerializer.Serialize(new List<string>
                {
                    "A type of weather forecast",
                    "A storage option on a satellite",
                    "A method to save files on the internet",
                    "A program for saving files locally on a computer",
                }),
            },
            new()
            {
                Type = QuestionType.MultipleChoice,
                Prompt = "What is the value of a?\n\n```python\na = 6 % 2\n```",
                CorrectAnswer = "0",
                ChoicesJson = JsonSerializer.Serialize(new List<string>
                {
                    "0",
                    "1",
                    "2",
                    "-100",
                }),
            },
            new()
            {
                Type = QuestionType.MultipleChoice,
                Prompt = "Which of these is an example of SQL injection?",
                CorrectAnswer = "http://x.com/?id=123'OR 1=1",
                ChoicesJson = JsonSerializer.Serialize(new List<string>
                {
                    "http://x.com/?id=abc",
                    "http://x.com/?id=123'OR 1=1",
                    "http://x.com/?id=999",
                    "http://x.com/?id=123&theme=dark",
                }),
            },
            new()
            {
                Type = QuestionType.MultipleChoice,
                Prompt = "Which of the following operations will result in an error?\n\n```typescript\nconst a: ReadonlyArray<number> = [1];\n```",
                CorrectAnswer = "a[0] = 2",
                ChoicesJson = JsonSerializer.Serialize(new List<string>
                {
                    "var b = a",
                    "a.map(x => x)",
                    "a[0] = 2",
                    "var c = a[0]",
                }),
            },
            new()
            {
                Type = QuestionType.MultipleChoice,
                Prompt = "Which of these is NOT a Capgemini value?",
                CorrectAnswer = "Baldness",
                ChoicesJson = JsonSerializer.Serialize(new List<string>
                {
                    "Team-spirit",
                    "Fun",
                    "Boldness",
                    "Baldness",
                }),
            },

        };

        db.Questions.AddRange(questions);
        await db.SaveChangesAsync();

        // Create two quizzes
        var quizNoob = new Quiz { Name = "Noob Quiz", Difficulty = "noob" };
        var quizNerd = new Quiz { Name = "Nerd Quiz", Difficulty = "nerd" };
        db.Quizzes.AddRange(quizNoob, quizNerd);
        await db.SaveChangesAsync();

        // Link first 6 questions to Noob, remaining 8 to Nerd
        var quizQuestions = new List<QuizQuestion>();
        for (int i = 0; i < questions.Count; i++)
        {
            if (i < 6)
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
                    Sequence = (i - 6) + 1
                });
            }
        }
        db.QuizQuestions.AddRange(quizQuestions);
        await db.SaveChangesAsync();
    }
}