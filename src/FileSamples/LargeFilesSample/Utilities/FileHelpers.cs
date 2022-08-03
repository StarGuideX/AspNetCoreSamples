using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Reflection;
namespace LargeFilesSample.Utilities
{
    public class FileHelpers
    {
        // 如果想在IsValidFileExtensionAndSignature方法检查特殊的字符，在_allowedChars内添加。
        private static readonly byte[] _allowedChars = { };

        // 在网站(https://www.filesignatures.net/)可以获得更多的文件签名
        private static readonly Dictionary<string, List<byte[]>> _fileSignature = new Dictionary<string, List<byte[]>>
        {
            { ".gif", new List<byte[]> { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
            { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".jpeg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                }
            },
            { ".jpg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 },
                }
            },
            { ".pdf", new List<byte[]>
                {
                    new byte[] { 0x25, 0x50, 0x44, 0x46 },
                }
            },
            { ".ppt",
                new List<byte[]>
                {
                    new byte[] { 0xD0,0xCF,0x11,0xE0,0xA1,0xB1,0x1A,0xE1 },
                    new byte[] { 0x00,0x6E,0x1E,0xF0 },
                    new byte[] { 0x0F,0x00,0xE8,0x03 },
                    new byte[] { 0xA0,0x46,0x1D,0xF0 },
                    new byte[] { 0xFD,0xFF,0xFF,0xFF,0x0E,0x00,0x00,0x00 },
                    new byte[] { 0xFD,0xFF,0xFF,0xFF,0x1C,0x00,0x00,0x00 },
                    new byte[] { 0xFD,0xFF,0xFF,0xFF,0x43,0x00,0x00,0x00 },
                }
            },
            { ".pptx", new List<byte[]>
                {
                    new byte[] { 0x50,0x4B,0x03,0x04 },
                    new byte[] { 0x50,0x4B,0x03,0x04,0x14,0x00,0x06,0x00 },
                }
            },
            { ".doc", new List<byte[]>
                {
                    new byte[] { 0xD0,0xCF,0x11,0xE0,0xA1,0xB1,0x1A,0xE1 },
                    new byte[] { 0x0D,0x44,0x4F,0x43 },
                    new byte[] { 0xCF,0x11,0xE0,0xA1,0xB1,0x1A,0xE1,0x00 },
                    new byte[] { 0xDB,0xA5,0x2D,0x00 },
                    new byte[] { 0xEC,0xA5,0xC1,0x00 },
                }
            },
            { ".docx", new List<byte[]>
                {
                    new byte[] { 0x50,0x4B,0x03,0x04},
                    new byte[] { 0x50,0x4B,0x03,0x04,0x14,0x00,0x06,0x00},
                }
            },
            { ".zip", new List<byte[]>
                {
                    new byte[] { 0x50, 0x4B, 0x03, 0x04 },
                    new byte[] { 0x50, 0x4B, 0x4C, 0x49, 0x54, 0x45 },
                    new byte[] { 0x50, 0x4B, 0x53, 0x70, 0x58 },
                    new byte[] { 0x50, 0x4B, 0x05, 0x06 },
                    new byte[] { 0x50, 0x4B, 0x07, 0x08 },
                    new byte[] { 0x57, 0x69, 0x6E, 0x5A, 0x69, 0x70 },
                }
            },
            { ".rar", new List<byte[]> { new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07,0x00 } } },
            { ".7z", new List<byte[]>{ new byte[] { 0x37,0x7A,0xBC,0xAF,0x27,0x1C } } },
        };
        // **警告!**
        // 在以下文件处理方法中，不会扫描文件的内容。
        // 在大多数生产场景中，在将文件提供给用户或其他系统之前，
        // 会在文件上使用防病毒/反恶意软件扫描程序 API。

        public static async Task<byte[]> ProcessFormFile<T>(IFormFile formFile,
            ModelStateDictionary modelState, string[] permittedExtensions,
            long sizeLimit, ILogger _logger)
        {
            var fieldDisplayName = string.Empty;

            // 使用反射来获取与此 IFormFile 关联的模型属性的显示名称。
            // 如果未找到显示名称，则错误消息根本不会显示显示名称。
            MemberInfo property =
                typeof(T).GetProperty(
                    formFile.Name.Substring(formFile.Name.IndexOf(".",
                    StringComparison.Ordinal) + 1));

            if (property != null)
            {
                if (property.GetCustomAttribute(typeof(DisplayAttribute)) is
                    DisplayAttribute displayAttribute)
                {
                    fieldDisplayName = $"{displayAttribute.Name} ";
                }
            }
            // 不要相信客户端发送的文件名。 要显示文件名，请对值进行 HTML 编码。
            var trustedFileNameForDisplay = WebUtility.HtmlEncode(
                formFile.FileName);

            // 检查文件长度。 此检查不会捕获仅包含 BOM 作为其内容的文件。
            if (formFile.Length == 0)
            {
                modelState.AddModelError(formFile.Name,
                    $"{fieldDisplayName}({trustedFileNameForDisplay}) 为空.");

                return Array.Empty<byte>();
            }

            if (formFile.Length > sizeLimit)
            {
                var megabyteSizeLimit = sizeLimit / 1048576;
                modelState.AddModelError(formFile.Name,
                    $"{fieldDisplayName}({trustedFileNameForDisplay}) 超过 " +
                    $"{megabyteSizeLimit:N1} MB.");

                return Array.Empty<byte>();
            }

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await formFile.CopyToAsync(memoryStream);

                    // 检查内容长度，以防文件的唯一内容是 BOM，并且在删除 BOM 后内容实际上是空的。
                    if (memoryStream.Length == 0)
                    {
                        modelState.AddModelError(formFile.Name,
                            $"{fieldDisplayName}({trustedFileNameForDisplay}) 为空.");
                    }

                    if (!IsValidFileExtensionAndSignature(
                        formFile.FileName, memoryStream, permittedExtensions))
                    {
                        modelState.AddModelError(formFile.Name,
                            $"{fieldDisplayName}({trustedFileNameForDisplay})" +
                            $"不允许此文件类型或文件的签名与扩展名不匹配");
                    }
                    else
                    {
                        return memoryStream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                modelState.AddModelError(formFile.Name,
                    $"{fieldDisplayName}({trustedFileNameForDisplay}) 上传失败，请联系管理员。错误: {ex.HResult}");
                _logger.LogError($"文件上传失败。错误: {ex.HResult}");
            }

            return Array.Empty<byte>();
        }

        public static async Task<byte[]> ProcessStreamedFile(
            MultipartSection section, ContentDispositionHeaderValue contentDisposition,
            ModelStateDictionary modelState, string[] permittedExtensions, long sizeLimit, ILogger _logger)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await section.Body.CopyToAsync(memoryStream);

                    // 检查文件是否为空或文件大小是否符合
                    if (memoryStream.Length == 0)
                    {
                        modelState.AddModelError("File", "文件为空");
                    }
                    else if (memoryStream.Length > sizeLimit)
                    {
                        var megabyteSizeLimit = sizeLimit / 1048576;
                        modelState.AddModelError("File",
                        $"此文件超过 {megabyteSizeLimit:N1} MB.");
                    }
                    else if (!IsValidFileExtensionAndSignature(
                        contentDisposition.FileName.Value, memoryStream,
                        permittedExtensions))
                    {
                        modelState.AddModelError("File",
                            "不允许此文件类型或文件的签名与扩展名不匹配");
                    }
                    else
                    {
                        return memoryStream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                modelState.AddModelError("File", $"上传失败，请联系管理员。错误: {ex.HResult}");
                _logger.LogError($"文件上传失败。错误: {ex.HResult}");
            }

            return Array.Empty<byte>();
        }

        private static bool IsValidFileExtensionAndSignature(string fileName, Stream data, string[] permittedExtensions)
        {
            if (string.IsNullOrEmpty(fileName) || data == null || data.Length == 0)
            {
                return false;
            }

            var ext = Path.GetExtension(fileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
            {
                return false;
            }

            data.Position = 0;

            using (var reader = new BinaryReader(data))
            {
                if (ext.Equals(".txt") || ext.Equals(".csv") || ext.Equals(".prn"))
                {
                    if (_allowedChars.Length == 0)
                    {
                        // 将字符限制为 ASCII 编码
                        for (var i = 0; i < data.Length; i++)
                        {
                            if (reader.ReadByte() > sbyte.MaxValue)
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        // 限制字符为ASCII 编码和_allowedChars的值
                        for (var i = 0; i < data.Length; i++)
                        {
                            var b = reader.ReadByte();
                            if (b > sbyte.MaxValue ||
                                !_allowedChars.Contains(b))
                            {
                                return false;
                            }
                        }
                    }

                    return true;
                }

                // 文件签名检查
                var signatures = _fileSignature[ext];
                var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

                return signatures.Any(signature =>
                    headerBytes.Take(signature.Length).SequenceEqual(signature));
            }
        }
    }
}
