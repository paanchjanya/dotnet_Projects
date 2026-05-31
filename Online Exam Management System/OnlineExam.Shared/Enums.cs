namespace OnlineExam.Shared;

public enum UserRole : byte
{
    Student = 0,
    Admin = 1
}

public enum QuestionType : byte
{
    MultipleChoice = 0,
    TrueFalse = 1
}

public enum AttemptStatus : byte
{
    InProgress = 0,
    Completed = 1,
    TimedOut = 2
}
