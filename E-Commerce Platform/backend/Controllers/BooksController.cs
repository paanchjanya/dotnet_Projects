using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.DTOs;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly BookstoreDbContext _context;
        private readonly IWebHostEnvironment _env;

        public BooksController(BookstoreDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? category)
        {
            var query = _context.Books.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(b => b.Title.ToLower().Contains(lowerSearch) || 
                                         b.Author.ToLower().Contains(lowerSearch) ||
                                         b.Description.ToLower().Contains(lowerSearch));
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(b => b.Category.ToLower() == category.ToLower());
            }

            var books = await query.OrderByDescending(b => b.Id).ToListAsync();
            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound(new { Message = "Book not found." });
            }
            return Ok(book);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] BookCreateUpdate dto)
        {
            var book = new Book
            {
                Title = dto.Title,
                Author = dto.Author,
                Description = dto.Description,
                Price = dto.Price,
                Category = dto.Category,
                StockQuantity = dto.StockQuantity,
                CoverImageUrl = dto.CoverImageUrl
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] BookCreateUpdate dto)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound(new { Message = "Book not found." });
            }

            book.Title = dto.Title;
            book.Author = dto.Author;
            book.Description = dto.Description;
            book.Price = dto.Price;
            book.Category = dto.Category;
            book.StockQuantity = dto.StockQuantity;
            if (!string.IsNullOrEmpty(dto.CoverImageUrl))
            {
                book.CoverImageUrl = dto.CoverImageUrl;
            }

            await _context.SaveChangesAsync();
            return Ok(book);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound(new { Message = "Book not found." });
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Book deleted successfully." });
        }

        [HttpPost("upload")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadCoverImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { Message = "No file uploaded." });
            }

            // Verify it's an image
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { Message = "Invalid file type. Only JPG, JPEG, PNG, WEBP, and GIF are allowed." });
            }

            // Create directories if they don't exist
            var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path
            var relativePath = $"/uploads/{fileName}";
            return Ok(new { Url = relativePath });
        }
    }
}
