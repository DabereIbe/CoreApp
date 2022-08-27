using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CoreApp.Utilities
{
    public class CloudinaryUpload
    {
        public CloudinaryCredentials _credentials;
        public CloudinaryUpload(IConfiguration configuration)
        {
            _credentials = configuration.GetSection("Cloudinary").Get<CloudinaryCredentials>();
        }

        public Task FileUpload(string filepath)
        {
            Account account = new Account(
            _credentials.CloudName,
            _credentials.ApiKey,
            _credentials.ApiSecret
            );

            Cloudinary cloudinary = new Cloudinary(account);
            cloudinary.Api.Secure = true;
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(filepath),
                PublicId = "watchstop/images/" + Path.GetFileNameWithoutExtension(filepath),
                Overwrite = true,
            };
            var uploadResult = cloudinary.Upload(uploadParams);
            return Task.FromResult(uploadResult);
        }

        public Task InvoiceUpload(string filepath)
        {
            Account account = new Account(
            _credentials.CloudName,
            _credentials.ApiKey,
            _credentials.ApiSecret);

            Cloudinary cloudinary = new Cloudinary(account);
            cloudinary.Api.Secure = true;
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(filepath),
                PublicId = "watchstop/invoices/" + Path.GetFileNameWithoutExtension(filepath),
                Overwrite = true,
            };
            var uploadResult = cloudinary.Upload(uploadParams);
            return Task.FromResult(uploadResult);
        }
    }
}
