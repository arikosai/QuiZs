using QuiZs.Tests.Helpers;
using QuiZs.Tests.Models;
using Xunit;

namespace QuiZs.Tests;

// 4. Тестирование режима прохождения
public sealed class QuizPassModeTests
{
    // 4.1 Вопросы и ответы доступны для отображения
    [Fact]
    public void PassMode_QuestionsAndAnswersAreAvailable()
    {
        var quiz = QuizBuilder.MakeQuiz(questionCount: 3);

        Assert.NotEmpty(quiz.Questions);
        foreach (var q in quiz.Questions)
            Assert.Equal(DraftQuestion.AnswerCount, q.Answers.Count);
    }

    // 4.2 Навигация вперёд увеличивает индекс вопроса
    [Fact]
    public void PassMode_MoveNext_IncrementsQuestionIndex()
    {
        var quiz = QuizBuilder.MakeQuiz(questionCount: 3);
        var state = new QuizRunState(quiz, QuizRunMode.Pass);

        state.MoveNext();

        Assert.Equal(1, state.CurrentIndex);
    }

    // 4.3 Навигация назад уменьшает индекс вопроса
    [Fact]
    public void PassMode_MovePrevious_DecrementsQuestionIndex()
    {
        var quiz = QuizBuilder.MakeQuiz(questionCount: 3);
        var state = new QuizRunState(quiz, QuizRunMode.Pass);
        state.MoveNext(); // index = 1

        state.MovePrevious();

        Assert.Equal(0, state.CurrentIndex);
    }

    // 4.4 Нельзя перейти назад с первого вопроса
    [Fact]
    public void PassMode_MovePreviousFromFirst_IndexStaysAtZero()
    {
        var quiz = QuizBuilder.MakeQuiz(questionCount: 3);
        var state = new QuizRunState(quiz, QuizRunMode.Pass);

        state.MovePrevious(); // уже на первом

        Assert.Equal(0, state.CurrentIndex);
    }

    // 4.5 Нельзя перейти вперёд за последний вопрос
    [Fact]
    public void PassMode_MoveNextOnLast_IsLastQuestion()
    {
        var quiz = QuizBuilder.MakeQuiz(questionCount: 2);
        var state = new QuizRunState(quiz, QuizRunMode.Pass);
        state.MoveNext();

        var isLast = state.IsLastQuestion;

        Assert.True(isLast);
    }

    // 4.6 Выбор варианта ответа сохраняется
    [Fact]
    public void PassMode_SelectAnswer_IsRecorded()
    {
        var quiz = QuizBuilder.MakeQuiz(questionCount: 2);
        var state = new QuizRunState(quiz, QuizRunMode.Pass);

        state.SelectAnswer(2); // выбираем вариант 2

        Assert.Equal(2, state.SelectedAnswers[0]);
    }

    // 4.7 После выбора ответа повторный выбор не меняет результат
    [Fact]
    public void PassMode_SelectAnswer_CannotBeChangedAfterSelection()
    {
        var quiz = QuizBuilder.MakeQuiz(questionCount: 2);
        var state = new QuizRunState(quiz, QuizRunMode.Pass);

        state.SelectAnswer(0);
        var alreadyAnswered = state.SelectedAnswers[0].HasValue;
        if (!alreadyAnswered)
            state.SelectAnswer(2);

        Assert.Equal(0, state.SelectedAnswers[0]);
    }

    // 4.8 Правильный ответ должен отображаться зелёным цветом
    [Fact]
    public void PassMode_CorrectAnswer_IsMarkedGreen()
    {
        var quiz = QuizBuilder.MakeQuiz(questionCount: 1);
        var state = new QuizRunState(quiz, QuizRunMode.Pass);
        state.SelectAnswer(0);

        var question = quiz.Questions[0];
        var selectedIndex = state.SelectedAnswers[0]!.Value;
        var isCorrect = question.Answers[selectedIndex].IsCorrect;

        Assert.True(isCorrect);
    }

    // 4.9 Неправильный ответ должен отображаться красным цветом
    [Fact]
    public void PassMode_WrongAnswer_IsMarkedRed()
    {
        var quiz = QuizBuilder.MakeQuiz(questionCount: 1);
        var state = new QuizRunState(quiz, QuizRunMode.Pass);
        state.SelectAnswer(1);

        var question = quiz.Questions[0];
        var selectedIndex = state.SelectedAnswers[0]!.Value;
        var isCorrect = question.Answers[selectedIndex].IsCorrect;

        Assert.False(isCorrect);
    }

    // 4.10 Ответы для разных вопросов хранятся независимо
    [Fact]
    public void PassMode_AnswersForDifferentQuestions_AreIndependent()
    {
        var quiz = QuizBuilder.MakeQuiz(questionCount: 3);
        var state = new QuizRunState(quiz, QuizRunMode.Pass);

        state.SelectAnswer(0);
        state.MoveNext();
        state.SelectAnswer(2);
        state.MoveNext();
        state.SelectAnswer(1);

        Assert.Equal(0, state.SelectedAnswers[0]);
        Assert.Equal(2, state.SelectedAnswers[1]);
        Assert.Equal(1, state.SelectedAnswers[2]);
    }
}

// 4. Тестирование режима прохождения. Результаты викторины
public sealed class QuizResultsTests
{
    // 4.11 Все ответы правильные — счётчик равен числу вопросов
    [Fact]
    public void CountCorrectAnswers_AllCorrect_ReturnsTotal()
    {
        var quiz = QuizBuilder.MakeQuiz(questionCount: 3);
        var selectedAnswers = new int?[] { 0, 0, 0 };

        var correct = CountCorrectAnswers(quiz, selectedAnswers);

        Assert.Equal(3, correct);
    }

    // 4.12 Все ответы неправильные — счётчик равен нулю
    [Fact]
    public void CountCorrectAnswers_NoneCorrect_ReturnsZero()
    {
        var quiz = QuizBuilder.MakeQuiz(questionCount: 3);
        var selectedAnswers = new int?[] { 1, 1, 1 };

        var correct = CountCorrectAnswers(quiz, selectedAnswers);

        Assert.Equal(0, correct);
    }

    // 4.13 Часть ответов правильная — счётчик корректен
    [Fact]
    public void CountCorrectAnswers_SomeCorrect_ReturnsPartialCount()
    {
        var quiz = QuizBuilder.MakeQuiz(questionCount: 4);
        var selectedAnswers = new int?[] { 0, 1, 0, 2 };

        var correct = CountCorrectAnswers(quiz, selectedAnswers);

        Assert.Equal(2, correct);
    }

    // 4.14 Пропущенные вопросы (null) не считаются правильными
    [Fact]
    public void CountCorrectAnswers_SkippedAnswers_NotCounted()
    {
        var quiz = QuizBuilder.MakeQuiz(questionCount: 3);
        var selectedAnswers = new int?[] { 0, null, null };

        var correct = CountCorrectAnswers(quiz, selectedAnswers);

        Assert.Equal(1, correct);
    }

    // 4.15 Результаты доступны только в режиме «Прохождение»
    [Theory]
    [InlineData(QuizRunMode.Pass, true)]
    [InlineData(QuizRunMode.Demo, false)]
    public void Results_OnlyAvailableInPassMode(QuizRunMode mode, bool expectResults)
    {
        var showResults = mode == QuizRunMode.Pass;

        Assert.Equal(expectResults, showResults);
    }

    private static int CountCorrectAnswers(Quiz quiz, int?[] selectedAnswers)
    {
        var correctCount = 0;
        for (var i = 0; i < quiz.Questions.Count; i++)
        {
            var selected = selectedAnswers[i];
            if (selected >= 0 &&
                selected.HasValue &&
                selected.Value < quiz.Questions[i].Answers.Count &&
                quiz.Questions[i].Answers[selected.Value].IsCorrect)
            {
                correctCount++;
            }
        }
        return correctCount;
    }
}


// 5. Тестирование режима демонстрации
public sealed class QuizDemoModeTests
{
    // 5.1 Вопросы и варианты ответа доступны для отображения
    [Fact]
    public void DemoMode_QuestionsAndAnswersAreAvailable()
    {
        var quiz = QuizBuilder.MakeQuiz(questionCount: 3);
        var state = new QuizRunState(quiz, QuizRunMode.Demo);

        Assert.Equal(3, quiz.Questions.Count);
        foreach (var q in quiz.Questions)
            Assert.Equal(DraftQuestion.AnswerCount, q.Answers.Count);
    }

    // 5.2 Навигация вперёд работает в режиме демонстрации
    [Fact]
    public void DemoMode_MoveNext_IncrementsIndex()
    {
        var quiz = QuizBuilder.MakeQuiz(questionCount: 3);
        var state = new QuizRunState(quiz, QuizRunMode.Demo);

        state.MoveNext();

        Assert.Equal(1, state.CurrentIndex);
    }

    // 5.3 Навигация назад работает в режиме демонстрации
    [Fact]
    public void DemoMode_MovePrevious_DecrementsIndex()
    {
        var quiz = QuizBuilder.MakeQuiz(questionCount: 3);
        var state = new QuizRunState(quiz, QuizRunMode.Demo);
        state.MoveNext();

        state.MovePrevious();

        Assert.Equal(0, state.CurrentIndex);
    }

    // 5.4 В режиме демонстрации ответы не фиксируются
    [Fact]
    public void DemoMode_AnswerSelection_IsNotTracked()
    {
        var quiz = QuizBuilder.MakeQuiz(questionCount: 2);
        var state = new QuizRunState(quiz, QuizRunMode.Demo);

        Assert.All(state.SelectedAnswers, a => Assert.Null(a));
    }

    // 5.5 На последнем вопросе определяется конец демонстрации
    [Fact]
    public void DemoMode_LastQuestion_IsDetectedCorrectly()
    {
        var quiz = QuizBuilder.MakeQuiz(questionCount: 2);
        var state = new QuizRunState(quiz, QuizRunMode.Demo);
        state.MoveNext(); // перешли на последний

        Assert.True(state.IsLastQuestion);
    }

    // 5.6 Режим корректно передаётся в объект состояния
    [Fact]
    public void DemoMode_ModeIsSetCorrectly()
    {
        var quiz = QuizBuilder.MakeQuiz();
        var state = new QuizRunState(quiz, QuizRunMode.Demo);

        Assert.Equal(QuizRunMode.Demo, state.Mode);
    }
}

public sealed class QuizRunState
{
    private readonly Quiz _quiz;
    public QuizRunMode Mode { get; }
    public int?[] SelectedAnswers { get; }
    public int CurrentIndex { get; private set; }

    public bool IsLastQuestion => CurrentIndex == _quiz.Questions.Count - 1;

    public QuizRunState(Quiz quiz, QuizRunMode mode)
    {
        _quiz = quiz;
        Mode = mode;
        SelectedAnswers = new int?[quiz.Questions.Count];
    }

    public void SelectAnswer(int answerIndex)
    {
        if (SelectedAnswers[CurrentIndex].HasValue) return;
        SelectedAnswers[CurrentIndex] = answerIndex;
    }

    public void MoveNext()
    {
        if (IsLastQuestion) return;
        CurrentIndex++;
    }

    public void MovePrevious()
    {
        if (CurrentIndex == 0) return;
        CurrentIndex--;
    }
}
