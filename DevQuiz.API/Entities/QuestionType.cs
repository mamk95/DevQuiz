namespace DevQuiz.API.Entities;

/// <summary>
/// Defines the types of questions available in the quiz system.
/// </summary>
public enum QuestionType : byte
{
    /// <summary>
    /// A multiple choice question with four options, one correct answer.
    /// </summary>
    MultipleChoice = 0,

    /// <summary>
    /// A code-fix challenge where participants must fix code to pass a test.
    /// </summary>
    CodeFix = 1,
}