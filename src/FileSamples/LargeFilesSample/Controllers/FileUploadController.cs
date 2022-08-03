using LargeFilesSample.Utilities;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Net;

namespace LargeFilesSample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileUploadController : ControllerBase
    {
        /// <summary>
        /// 文件大小限制
        /// </summary>
        private readonly long _fileSizeLimit;
        /// <summary>
        /// 允许的文件扩展名
        /// </summary>
        private readonly string[] _permittedExtensions = { ".pdf", ".ppt", ".pptx", ".png", ".jpg", ".jpeg", ".zip", ".rar", ".7z", "doc", "docx" };
        /// <summary>
        /// 文件目录
        /// </summary>
        private readonly string _targetFilePath;

        private readonly ILogger<FileUploadController> _logger;

        // 获取默认表单选项，以便我们可以使用它们来设置请求正文数据的默认限制。
        private static readonly FormOptions _defaultFormOptions = new FormOptions();

        private readonly IConfiguration _configuration;
        public FileUploadController(ILogger<FileUploadController> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            _logger = logger;

            _fileSizeLimit = _configuration.GetValue<int>("FileSizeLimit");
            _targetFilePath = _configuration.GetValue<string>("TargetFilePath");

            if (!Directory.Exists(_targetFilePath))
            {
                Directory.CreateDirectory(_targetFilePath);
            }
        }

        [HttpPost]
        //[Authorize]
        [Route("upload")]
        public async Task<IActionResult> Upload()
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                ModelState.AddModelError("File",
                    "无法处理该请求(ContentType不是Multipart).");
                _logger.LogError("无法处理该请求(ContentType不是Multipart).");
                return BadRequest();
            }

            var boundary = MultipartRequestHelper.GetBoundary(
                MediaTypeHeaderValue.Parse(Request.ContentType),
                _defaultFormOptions.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            var section = await reader.ReadNextSectionAsync();

            while (section != null)
            {
                var hasContentDispositionHeader =
                    ContentDispositionHeaderValue.TryParse(
                        section.ContentDisposition, out var contentDisposition);

                if (hasContentDispositionHeader)
                {
                    // 如果存在表单数据，立即失败并返回。
                    if (!MultipartRequestHelper
                        .HasFileContentDisposition(contentDisposition))
                    {
                        ModelState.AddModelError("File", $"无法处理该请求(ContentDisposition不是form-data，或文件名为空)");
                        _logger.LogError("无法处理该请求(ContentDisposition不是form-data，或文件名为空)");
                        return BadRequest(ModelState);
                    }
                    else
                    {
                        // 不要相信客户端发送的文件名。 要显示文件名，请对值进行 HTML 编码。
                        var trustedFileNameForDisplay = WebUtility.HtmlEncode(
                                contentDisposition.FileName.Value);
                        var trustedFileNameForFileStorage = Path.GetRandomFileName();

                        // **警告!**
                        // 在以下文件处理方法中，不会扫描文件的内容。
                        // 在大多数生产场景中，在将文件提供给用户或其他系统之前，
                        // 会在文件上使用防病毒/反恶意软件扫描程序 API。

                        var streamedFileContent = await FileHelpers.ProcessStreamedFile(
                            section, contentDisposition, ModelState,
                            _permittedExtensions, _fileSizeLimit, _logger);

                        if (!ModelState.IsValid)
                        {
                            return BadRequest(ModelState);
                        }

                        using (var targetStream = System.IO.File.Create(
                            Path.Combine(_targetFilePath, trustedFileNameForFileStorage)))
                        {
                            await targetStream.WriteAsync(streamedFileContent);

                            _logger.LogInformation(
                                "文件'{TrustedFileNameForDisplay}' 保存在 " +
                                "'{TargetFilePath}' 作为 {TrustedFileNameForFileStorage}",
                                trustedFileNameForDisplay, _targetFilePath,
                                trustedFileNameForFileStorage);
                        }
                    }
                }

                // 读取下一节
                section = await reader.ReadNextSectionAsync();
            }

            return Created(nameof(FileUploadController), null);
        }
    }
}