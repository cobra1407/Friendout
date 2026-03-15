using Friendout.Infrastructure.Enums;

namespace Friendout.Infrastructure.Utils;

public static class FileCategoryMapper
{

    public static string ResolveFolder(FileCategory category)
    {
        return category switch
        {
            FileCategory.UserAvatar => "users/avatars",
            FileCategory.ActivityImage => "activities/images",
            FileCategory.ActivityAttachment => "activities/attachments",
            FileCategory.Document => "documents",
            _ => "misc"
        };
    }

}