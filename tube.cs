using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace YoutubeStreamingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StreamingController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public StreamingController()
        {
            _httpClient = new HttpClient();
        }

        private async Task<string> ShortenUrlAsync(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync($"http://tinyurl.com/api-create.php?url={url}");
                if (response.IsSuccessStatusCode)
                {
                    return (await response.Content.ReadAsStringAsync()).Trim();
                }
            }
            catch (Exception)
            {
                // Handle any errors if necessary
            }
            return url;
        }

        private async Task<List<Dictionary<string, string>>> GetStreamingUrls(string videoUrl)
        {
            var youtubeClient = new YoutubeClient();
            var urlsAndResolutions = new List<Dictionary<string, string>>();

            try
            {
                var video = await youtubeClient.Videos.GetAsync(videoUrl);
                var streams = await youtubeClient.Videos.Streams.GetManifestAsync(videoUrl);

                var resolucoes = new HashSet<string>();

                foreach (var streamInfo in streams.GetMuxedStreams())
                {
                    if (streamInfo.Container == Container.Mp4 && !resolucoes.Contains(streamInfo.VideoResolution.Height.ToString()))
                    {
                        resolucoes.Add(streamInfo.VideoResolution.Height.ToString());

                        var temAudio = streamInfo.AudioCodec != null ? "sim" : "não";
                        var url = await ShortenUrlAsync(streamInfo.Url);

                        urlsAndResolutions.Add(new Dictionary<string, string>
                        {
                            { "resolucao", streamInfo.VideoResolution.Height.ToString() + "p" },
                            { "url_streaming", url },
                            { "audio", temAudio }
                        });
                    }
                }
            }
            catch (Exception)
            {
                // Handle any errors if necessary
            }

            return urlsAndResolutions;
        }

        [HttpPost("obter_resolucoes")]
        public async Task<IActionResult> ObterResolucoes([FromBody] VideoRequest videoRequest)
        {
            try
            {
                var urlsAndResolutions = await GetStreamingUrls(videoRequest.VideoUrl);

                if (urlsAndResolutions.Count == 0)
                {
                    return NotFound(new { message = "Não foi possível encontrar uma stream válida para o vídeo." });
                }

                return Ok(urlsAndResolutions);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Ocorreu um erro ao processar a solicitação." });
            }
        }
    }

    public class VideoRequest
    {
        public string VideoUrl { get; set; }
    }
}
