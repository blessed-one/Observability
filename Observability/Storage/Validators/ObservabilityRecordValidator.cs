using FluentValidation;
using Realisation;

namespace Storage.Validators;

public class ObservabilityRecordValidator : AbstractValidator<ObservabilityRecord>
{
    public ObservabilityRecordValidator()
    {
        RuleFor(x => x.TraceId).NotEmpty().WithMessage("TraceId is required");
        RuleFor(x => x.ParentId).NotEmpty().WithMessage("ParentId is required");
        RuleFor(x => x.HttpRequestData).NotEmpty().WithMessage("HttpRequestData is required");
        RuleFor(x => x.NodeId).NotEmpty().WithMessage("NodeId is required");
        RuleFor(x => x.Timestamp).NotEmpty().WithMessage("Timestamp is required");
    }
} 