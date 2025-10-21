using FluentValidation;
using Shared.Models;
using System.Net;

namespace Shared.Validators;
public class IpDetailsValidator : AbstractValidator<IpDetails>
{
    public IpDetailsValidator()
    {
        RuleFor(x => x.Ip)
            .NotEmpty()
            .WithMessage("IP address is required.")
            .Must(BeAValidIpAddress)
            .WithMessage("Invalid IP address format.");

        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Type is required.")
            .Must(t => t == "ipv4" || t == "ipv6")
            .WithMessage("Type must be either 'ipv4' or 'ipv6'.");

        RuleFor(x => x.ContinentCode)
            .MaximumLength(5)
            .When(x => !string.IsNullOrWhiteSpace(x.ContinentCode))
            .WithMessage("Continent code cannot exceed 5 characters.");

        RuleFor(x => x.ContinentName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.ContinentName));

        RuleFor(x => x.CountryCode)
            .Length(2)
            .When(x => !string.IsNullOrWhiteSpace(x.CountryCode))
            .WithMessage("Country code must be exactly 2 letters.");

        RuleFor(x => x.CountryName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.CountryName));

        RuleFor(x => x.RegionCode)
            .MaximumLength(10)
            .When(x => !string.IsNullOrWhiteSpace(x.RegionCode));

        RuleFor(x => x.RegionName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.RegionName));

        RuleFor(x => x.City)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.City));

        RuleFor(x => x.Zip)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.Zip));

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .When(x => x.Latitude.HasValue)
            .WithMessage("Latitude must be between -90 and 90.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .When(x => x.Longitude.HasValue)
            .WithMessage("Longitude must be between -180 and 180.");

        RuleFor(x => x.RetrievedAt)
            .Must(date => date != default)
            .WithMessage("RetrievedAt must be a valid date.");
    }

    private bool BeAValidIpAddress(string? ip)
    {
        return !string.IsNullOrWhiteSpace(ip) && IPAddress.TryParse(ip, out _);
    }
}
