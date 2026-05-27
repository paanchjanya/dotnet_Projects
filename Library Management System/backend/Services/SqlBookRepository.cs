using System.Collections.Generic;
using System.Linq;
using backend.Models;

namespace backend.Services;

public class SqlBookRepository : IBookRepository
{
    private readonly LibraryDbContext _context;

    public SqlBookRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Book> GetAll()
    {
        return _context.Books.ToList();
    }

    public Book? GetById(int id)
    {
        return _context.Books.Find(id);
    }

    public Book Add(Book book)
    {
        book.Id = 0; // Let SQL Server generate the Identity column
        _context.Books.Add(book);
        _context.SaveChanges();
        return book;
    }

    public bool Update(Book book)
    {
        var existing = _context.Books.Find(book.Id);
        if (existing == null) return false;

        _context.Entry(existing).CurrentValues.SetValues(book);
        _context.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var book = _context.Books.Find(id);
        if (book == null) return false;

        _context.Books.Remove(book);
        _context.SaveChanges();
        return true;
    }
}
