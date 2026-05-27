using System.Collections.Generic;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

// Attrbiute Routing
[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookRepository _repository;

    public BooksController(IBookRepository repository)
    {
        _repository = repository;
    }

    // GET /api/books
    [HttpGet]
    public ActionResult<IEnumerable<Book>> GetAll()
    {
        var books = _repository.GetAll();
        return Ok(books);
    }

    // GET /api/books/{id}
    [HttpGet("{id}")]
    public ActionResult<Book> GetById(int id)
    {
        var book = _repository.GetById(id);
        if (book == null)
        {
            return NotFound(new { message = $"Book with ID {id} not found." });
        }
        return Ok(book);
    }

    // POST /api/books
    [HttpPost]
    public ActionResult<Book> Add([FromBody] Book book)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdBook = _repository.Add(book);
        return CreatedAtAction(nameof(GetById), new { id = createdBook.Id }, createdBook);
    }

    // PUT /api/books/{id}
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Book book)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        book.Id = id;
        var updated = _repository.Update(book);
        if (!updated)
        {
            return NotFound(new { message = $"Book with ID {id} not found." });
        }

        var updatedBook = _repository.GetById(id);
        return Ok(updatedBook);
    }

    // DELETE /api/books/{id}
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var deleted = _repository.Delete(id);
        if (!deleted)
        {
            return NotFound(new { message = $"Book with ID {id} not found." });
        }

        return NoContent();
    }
}
