using BookAPI.Models;

namespace BookAPI.Repositories;

public interface IBookRepository
{
    Task<IEnumerable<Book>> GetAllAsync();
    Task<Book?> GetByIdAsync(int id);
    Task<Book> CreateAsync(Book book);
    Task<Book?> UpdateAsync(int id, Book book);
    Task<bool> DeleteAsync(int id);
}