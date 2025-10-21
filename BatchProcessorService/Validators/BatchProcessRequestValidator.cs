using System.Net;
using FluentValidation;

namespace BatchProcessorService.Validators;

public class BatchProcessRequestValidator : AbstractValidator<IEnumerable<string>>
{
    public BatchProcessRequestValidator()
    {
        RuleFor(x => x)
            .NotEmpty()
            .WithMessage("The list of IP addresses cannot be empty.");

        RuleForEach(x => x)
            .NotEmpty()
            .WithMessage("IP address cannot be null or empty")
            .Must(BeAValidIpAddress)
            .WithMessage("Invalid IP address format");
    }

    private bool BeAValidIpAddress(string ip)
    {
        return IPAddress.TryParse(ip, out _);
    }
}