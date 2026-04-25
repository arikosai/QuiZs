using QuiZs.Models;

namespace QuiZs.Mappers;

public static class DraftQuestionMapper
{
    public static DraftQuestion FromQuestion(Question question)
    {
        var draft = new DraftQuestion { Text = question.Text };

        for (var i = 0; i < DraftQuestion.AnswerCount && i < question.Answers.Count; i++)
        {
            draft.Answers[i] = question.Answers[i].Text;
            if (question.Answers[i].IsCorrect) 
                draft.CorrectAnswerIndex = i;
        }

        return draft;
    }

    public static Question ToQuestion(DraftQuestion draft)
    {
        return new Question
        {
            Text = draft.Text.Trim(),
            Answers = draft.Answers
                .Select((answer, index) => new Answer
                {
                    Text = answer.Trim(),
                    IsCorrect = index == draft.CorrectAnswerIndex
                })
                .ToList()
        };
    }
}
