using BookAPI.Models;
using BookAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BookAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookRepository _repo;

    public BooksController(IBookRepository repo) => _repo = repo;

    // GET api/books
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _repo.GetAllAsync());

    // GET api/books/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var book = await _repo.GetByIdAsync(id);
        return book is null ? NotFound() : Ok(book);
    }

    // POST api/books
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Book book)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _repo.CreateAsync(book);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT api/books/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Book book)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _repo.UpdateAsync(id, book);
        return updated is null ? NotFound() : Ok(updated);
    }

    // DELETE api/books/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _repo.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}