using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using ShrURL.Data;
using ShrURL.DTOs;
using ShrURL.Models;
using System;
using System.Diagnostics;
using System.Text.Json;

namespace ShrURL.Controllers {
    public class HomeController : Controller {

        private readonly IDistributedCache _cache;
        private readonly ApplicationDbContext _context;
        private static readonly Random _rnd = new();

        public HomeController(IDistributedCache cache, ApplicationDbContext context) {
            _cache = cache;
            _context = context;
        }

        private string NewId_FromRandomLong() => _rnd.Next().ToString("x");

        //private byte[] ToByteArray(string obj) {
        //    return JsonSerializer.SerializeToUtf8Bytes(obj);
        //}

        //private string? FromByteArray(byte[] data) {
        //    return JsonSerializer.Deserialize<string>(data);
        //}

        private async void SetCacheURL(string key, string url) {
            await _cache.SetStringAsync(key.ToString(), url, new DistributedCacheEntryOptions {
                SlidingExpiration = TimeSpan.FromMinutes(2),
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            });
        }

        private async Task<string?> GetCacheURL(string key) {
            string? value = await _cache.GetStringAsync(key);

            return value;
        }


        [HttpGet]
        public IActionResult Short() {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromRoute] string id) {
            if (id is null) return NotFound();

            string? URLInCache = await GetCacheURL(id);
            if (URLInCache is not null) return Redirect(URLInCache);

            var shortUrl = await _context.ShortURLs
                .FirstOrDefaultAsync(url => url.ShortUniqueId == id);

            if (shortUrl == null) return NotFound();

            SetCacheURL(id, shortUrl.Original);

            return Redirect(shortUrl.Original);
        }

        [HttpPost]
        public async Task<IActionResult> Short([FromBody] CreateDTOShortURL createDTO) {
            if (!ModelState.IsValid) {
                var errors = ModelState
                    .Where(m => m.Value.Errors.Any())
                    .Select(m => new {
                        Field = m.Key,
                        Messages = m.Value.Errors.Select(e => e.ErrorMessage)
                    })
                    .ToList();

                return BadRequest(errors);
            }

            var url = await _context.ShortURLs
                .FirstOrDefaultAsync(url => url.Original == createDTO.LongURL);

            if (url != null) {

                string? URLInCache = await GetCacheURL(url.ShortUniqueId);
                if (URLInCache is null) {
                    SetCacheURL(url.ShortUniqueId, url.Original);
                }

                return Ok(url);
            }

            string shortId = NewId_FromRandomLong();

            ShortURL shortURL = new() {
                Original = createDTO.LongURL,
                ShortUniqueId = shortId,
            };

            await _context.AddAsync(shortURL);
            await _context.SaveChangesAsync();

            SetCacheURL(shortId, shortURL.Original);

            return Created($"/{shortId}", shortURL);
        }




        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
