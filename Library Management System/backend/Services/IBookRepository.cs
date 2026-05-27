using System.Collections.Generic;
using backend.Models;

namespace backend.Services;

public interface IBookRepository
{
    IEnumerable<Book> GetAll();
    Book? GetById(int id);
    Book Add(Book book);
    bool Update(Book book);
    bool Delete(int id);
}
