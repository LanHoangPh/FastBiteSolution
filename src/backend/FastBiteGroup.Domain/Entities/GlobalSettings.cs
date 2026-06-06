using System.ComponentModel.DataAnnotations;

namespace FastBiteGroup.Domain.Entities;

public class GlobalSettings
{
    [Key]
    [MaxLength(100)]
    public string SettingKey { get; set; } = string.Empty;

    [Required]
    public string SettingValue { get; set; } = string.Empty;

    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}

