namespace FastBiteGroup.Persistence.DependencyInjection.Options;

public class PasswordValidatorOptions
{
    public int RequiredMinLength { get; set; }
    public int RequiredMaxLength { get; set; } = int.MaxValue;
    public int RequireNonLetterOrDigitLength { get; set; }
    public int RequireLowercaseLength { get; set; }
    public int RequireUppercaseLength { get; set; }
    public int RequireDigitLength { get; set; }
}
