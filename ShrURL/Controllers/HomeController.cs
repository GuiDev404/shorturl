using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ShrURL.Data;
using ShrURL.DTOs;
using ShrURL.Models;
using System;
using System.Diagnostics;

namespace ShrURL.Controllers {
    public class HomeController : Controller {

        private readonly IMemoryCache _cache;
        private readonly ApplicationDbContext _context;
        private static readonly Random _rnd = new();

        public HomeController(IMemoryCache cache, ApplicationDbContext context) {
            _cache = cache;
            _context = context;
        }

        public string NewId_FromRandomLong() => _rnd.Next().ToString("x");

        private void SetCacheURL(string key, string url) {
            _cache.Set(key, url, new MemoryCacheEntryOptions {
                SlidingExpiration = TimeSpan.FromMinutes(2), // tiempo acumulativo +2min
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10), // maximo 10min
            });
        }

        private string? GetCacheURL(string key) {
            var inCache = _cache.TryGetValue(key, out string cachedURL);
            if (!inCache) return null;

            return cachedURL;
        }


        [HttpGet]
        public IActionResult Short() {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromRoute] string id) {
            if (id is null) return NotFound();

            string? URLInCache = GetCacheURL(id);
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

                string? URLInCache = GetCacheURL(url.ShortUniqueId);
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
