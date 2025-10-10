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
            // NOOB QUESTIONS (first 7)
            new()
            {
                Type = QuestionType.MultipleChoice,
                Prompt = "Which change makes the assertion pass? \n\n```python\ndef sum_two_numbers(a, b):\n    return a - b\n\nassert sum_two_numbers(2, 3) == 5\n```",
                CorrectAnswer = "return a + b",
                ChoicesJson = JsonSerializer.Serialize(new List<string>
                {
                    "return a * b",
                    "return a + b",
                    "return abs(a - b)",
                    "return max(a, b)",
                }),
            },
            new()
            {
                Type = QuestionType.MultipleChoice,
                Prompt = "What's the main goal of UX design?",
                CorrectAnswer = "Help users achieve their goals easily",
                ChoicesJson = JsonSerializer.Serialize(new List<string>
                {
                    "Make the interface look cool",
                    "Help users achieve their goals easily",
                    "Add as many options as possible",
                    "Reduce server load",
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
                    "Encrypting code with duck-themed algorithms",
                    "Using a duck-shaped USB for secure storage",
                    "Debugging code underwater",
                }),
            },
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
                Prompt = "What's the most important thing to consider when designing an app for users with visual impairments?",
                CorrectAnswer = "Ensuring high contrast and screen reader compatibility",
                ChoicesJson = JsonSerializer.Serialize(new List<string>
                {
                    "Using bright colors and animations",
                    "Making the app look trendy and modern",
                    "Ensuring high contrast and screen reader compatibility",
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
                    "To allow multiple users to share an account",
                    "To add an extra layer of security",
                    "To reset your password",
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
                Type = QuestionType.CodeFix,
                Prompt = "Make both assertions pass:",
                CorrectAnswer = "def is_even(n):\n    return n % 2 == 0",
                ChoicesJson = JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["initialCode"] = "def is_even(n):\n    return n % 2 == 1",
                    ["testCode"] = "assert is_even(4) is True and is_even(5) is False",
                }),
            },
            new()
            {
                Type = QuestionType.MultipleChoice,
                Prompt = "Which of the following is NOT a common penetration testing tool?",
                CorrectAnswer = "NeedleWork",
                ChoicesJson = JsonSerializer.Serialize(new List<string>
                {
                    "Metasploit",
                    "NeedleWork",
                    "Nmap",
                    "Wireshark",
                }),
            },
            new()
            {
                Type = QuestionType.MultipleChoice,
                Prompt = "Based on this code:\n\n```typescript\ntype Shape =\n  | { kind: \"circle\"; r: number }\n  | { kind: \"square\"; s: number };\n\nfunction area(shape: Shape) {\n  if (shape.kind === \"circle\") {\n    return Math.PI * shape.r * shape.r;\n  } else {\n    return shape.s * shape.s;\n  }\n}\n```\n\nWhich statement is correct?",
                CorrectAnswer = "Checking shape.kind narrows the type appropriately",
                ChoicesJson = JsonSerializer.Serialize(new List<string>
                {
                    "You can access shape.r without checks",
                    "A type assertion is required to access shape.r",
                    "Checking shape.kind narrows the type appropriately",
                    "TypeScript cannot discriminate unions like this",
                }),
            },
            new()
            {
                Type = QuestionType.MultipleChoice,
                Prompt = "Which of the following is NOT an example of universal design in digital products?",
                CorrectAnswer = "Using red and green color coding to differentiate between buttons",
                ChoicesJson = JsonSerializer.Serialize(new List<string>
                {
                    "Providing text alternatives for images (alt text)",
                    "Ensuring keyboard navigation works throughout the site",
                    "Using high contrast between text and background",
                    "Using red and green color coding to differentiate between buttons",
                }),
            },
            new()
            {
                Type = QuestionType.MultipleChoice,
                Prompt = "What is printed?\n\n```java\nimport java.util.stream.*;\npublic class Main {\n  public static void main(String[] args) {\n    int s = IntStream.rangeClosed(1, 5)\n      .filter(n -> n % 2 == 0)\n      .sum();\n    System.out.println(s);\n  }\n}\n```",
                CorrectAnswer = "6",
                ChoicesJson = JsonSerializer.Serialize(new List<string>
                {
                    "4",
                    "6",
                    "8",
                    "12",
                }),
            },
            new()
            {
                Type = QuestionType.MultipleChoice,
                Prompt = "A website displays the following URL when viewing a user profile:\n\n```url\nhttp://example.com/profile?id=123\n```\n\nIf the application does not validate input, which of the following could be an example of a basic exploit attempt?",
                CorrectAnswer = "http://example.com/profile?id=123 OR 1=1",
                ChoicesJson = JsonSerializer.Serialize(new List<string>
                {
                    "http://example.com/profile?id=abc",
                    "http://example.com/profile?id=123 OR 1=1",
                    "http://example.com/profile?id=999",
                    "http://example.com/profile?id=123&theme=dark",
                }),
            },
            new()
            {
                Type = QuestionType.MultipleChoice,
                Prompt = "Which is a compile-time error for ReadonlyArray?\n\n```typescript\nconst a: ReadonlyArray<number> = [1, 2, 3];\n```",
                CorrectAnswer = "a.push(4)",
                ChoicesJson = JsonSerializer.Serialize(new List<string>
                {
                    "a.slice(1)",
                    "a.map(x => x)",
                    "a.push(4)",
                    "a[0]",
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
        var quizNoob = new Quiz { Name = "Noob Quiz", Difficulty = "Noob" };
        var quizNerd = new Quiz { Name = "Nerd Quiz", Difficulty = "Nerd" };
        db.Quizzes.AddRange(quizNoob, quizNerd);
        await db.SaveChangesAsync();

        // Link first 7 questions to Noob, remaining 8 to Nerd
        var quizQuestions = new List<QuizQuestion>();
        for (int i = 0; i < questions.Count; i++)
        {
            if (i < 7)
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
                    Sequence = (i - 7) + 1
                });
            }
        }
        db.QuizQuestions.AddRange(quizQuestions);
        await db.SaveChangesAsync();
    }
}