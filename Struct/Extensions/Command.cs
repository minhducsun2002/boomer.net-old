using System.Linq;
using Pepper.Classes.Command;
using Qmmands;

public static class CommandExtensions
{
    public static string Category(this Command command)
    {
        var category
            = command.Attributes.FirstOrDefault(attrib => attrib is CategoryAttribute);
        return category != null ? ((CategoryAttribute) category).Category : "";
    }
    
    public static string PrefixCategory(this Command command)
    {
        var category
            = command.Attributes.FirstOrDefault(attrib => attrib is PrefixCategoryAttribute);
        return category != null ? ((PrefixCategoryAttribute) category).PrefixCategory : "";
    }
}