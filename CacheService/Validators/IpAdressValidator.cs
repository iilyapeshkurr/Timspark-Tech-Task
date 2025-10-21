using FluentValidation;

namespace IpLookupService.Validators;

public class IpAddressValidator : AbstractValidator<string>
{
    public IpAddressValidator()
    {
        RuleFor(x => x)
            .NotEmpty()
            .WithMessage("IP address cannot be null or empty")
            .Must(BeAValidIpAddress)
            .WithMessage("Invalid IP address format");
    }

    private bool BeAValidIpAddress(string ip)
    {
        return System.Net.IPAddress.TryParse(ip, out _);
    }
}