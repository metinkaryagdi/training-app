using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TrainingPlatform.Application.Abstractions.Persistence;
using TrainingPlatform.Application.Common.Cqrs;
using TrainingPlatform.Application.Common.Exceptions;

namespace TrainingPlatform.Application.Features.Questions;

public sealed record GetQuestionByIdQuery(Guid QuestionId) : IQuery<QuestionDto>;

public sealed class GetQuestionByIdQueryValidator : AbstractValidator<GetQuestionByIdQuery>
{
    public GetQuestionByIdQueryValidator()
    {
        RuleFor(query => query.QuestionId).NotEmpty();
    }
}

public sealed class GetQuestionByIdQueryHandler(ITrainingPlatformDbContext dbContext) : IQueryHandler<GetQuestionByIdQuery, QuestionDto>
{
    public async Task<QuestionDto> Handle(GetQuestionByIdQuery query, CancellationToken cancellationToken)
    {
        var question = await dbContext.Questions
            .AsNoTracking()
            .Include(entry => entry.Options)
            .SingleOrDefaultAsync(entry => entry.Id == query.QuestionId, cancellationToken)
            ?? throw new NotFoundException("The requested question was not found.");

        return new QuestionDto(
            question.Id,
            question.TopicId,
            question.QuestionType,
            question.Prompt,
            question.Explanation,
            question.Difficulty,
            question.EstimatedSolvingTimeSeconds,
            question.MinimumPassingScore,
            question.Tags,
            question.AcceptedAnswers,
            question.Options.OrderBy(option => option.Order).Select(option => new QuestionOptionDto(option.Id, option.Text, option.IsCorrect, option.Order)).ToList());
    }
}
