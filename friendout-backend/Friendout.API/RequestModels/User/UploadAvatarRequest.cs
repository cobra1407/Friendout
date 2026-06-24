namespace friendout_backend.RequestModels.User
{
    /// <summary>
    /// Request model for uploading a custom user avatar in the API layer.
    /// This model contains ASP.NET Core-specific dependencies such as IFormFile.
    /// </summary>
    public class UploadAvatarRequest
    {
        /// <summary>
        /// The avatar image file to upload.
        /// </summary>
        public IFormFile? Avatar { get; set; }
    }
}
 