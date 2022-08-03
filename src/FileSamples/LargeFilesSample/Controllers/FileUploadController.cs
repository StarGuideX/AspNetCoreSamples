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
        /// �ļ���С����
        /// </summary>
        private readonly long _fileSizeLimit;
        /// <summary>
        /// ������ļ���չ��
        /// </summary>
        private readonly string[] _permittedExtensions = { ".pdf", ".ppt", ".pptx", ".png", ".jpg", ".jpeg", ".zip", ".rar", ".7z", "doc", "docx" };
        /// <summary>
        /// �ļ�Ŀ¼
        /// </summary>
        private readonly string _targetFilePath;

        private readonly ILogger<FileUploadController> _logger;

        // ��ȡĬ�ϱ�ѡ��Ա����ǿ���ʹ�����������������������ݵ�Ĭ�����ơ�
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
                    "�޷����������(ContentType����Multipart).");
                _logger.LogError("�޷����������(ContentType����Multipart).");
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
                    // ������ڱ����ݣ�����ʧ�ܲ����ء�
                    if (!MultipartRequestHelper
                        .HasFileContentDisposition(contentDisposition))
                    {
                        ModelState.AddModelError("File", $"�޷����������(ContentDisposition����form-data�����ļ���Ϊ��)");
                        _logger.LogError("�޷����������(ContentDisposition����form-data�����ļ���Ϊ��)");
                        return BadRequest(ModelState);
                    }
                    else
                    {
                        // ��Ҫ���ſͻ��˷��͵��ļ����� Ҫ��ʾ�ļ��������ֵ���� HTML ���롣
                        var trustedFileNameForDisplay = WebUtility.HtmlEncode(
                                contentDisposition.FileName.Value);
                        var trustedFileNameForFileStorage = Path.GetRandomFileName();

                        // **����!**
                        // �������ļ��������У�����ɨ���ļ������ݡ�
                        // �ڴ�������������У��ڽ��ļ��ṩ���û�������ϵͳ֮ǰ��
                        // �����ļ���ʹ�÷�����/���������ɨ����� API��

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
                                "�ļ�'{TrustedFileNameForDisplay}' ������ " +
                                "'{TargetFilePath}' ��Ϊ {TrustedFileNameForFileStorage}",
                                trustedFileNameForDisplay, _targetFilePath,
                                trustedFileNameForFileStorage);
                        }
                    }
                }

                // ��ȡ��һ��
                section = await reader.ReadNextSectionAsync();
            }

            return Created(nameof(FileUploadController), null);
        }
    }
}