using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using backend.Models;

namespace backend.Services;

public class BookRepository : IBookRepository
{
    private readonly ConcurrentDictionary<int, Book> _books = new();
    private int _nextId = 103; // Start from 103 so next is 104

    public BookRepository()
    {
        // Seed initial data
        _books.TryAdd(101, new Book
        {
            Id = 101,
            Title = "Clean Code",
            Author = "Robert Martin",
            Category = "Programming",
            Price = 599,
            PublishedDate = new DateOnly(2020, 10, 15),
            IsAvailable = true
        });

        _books.TryAdd(102, new Book
        {
            Id = 102,
            Title = "The Pragmatic Programmer",
            Author = "Andy Hunt & Dave Thomas",
            Category = "Programming",
            Price = 649,
            PublishedDate = new DateOnly(2019, 10, 20),
            IsAvailable = true
        });

        _books.TryAdd(103, new Book
        {
            Id = 103,
            Title = "Introduction to Algorithms",
            Author = "Thomas H. Cormen",
            Category = "Algorithms",
            Price = 1200,
            PublishedDate = new DateOnly(2009, 7, 31),
            IsAvailable = false
        });
    }

    public IEnumerable<Book> GetAll()
    {
        return _books.Values.OrderBy(b => b.Id);
    }

    public Book? GetById(int id)
    {
        _books.TryGetValue(id, out var book);
        return book;
    }

    public Book Add(Book book)
    {
        book.Id = System.Threading.Interlocked.Increment(ref _nextId);
        _books.TryAdd(book.Id, book);
        return book;
    }

    public bool Update(Book book)
    {
        if (!_books.ContainsKey(book.Id))
        {
            return false;
        }

        _books.AddOrUpdate(book.Id, book, (key, existingBook) =>
        {
            existingBook.Price = book.Price;
            existingBook.Category = book.Category;
            existingBook.IsAvailable = book.IsAvailable;
            
            // Retain Title, Author and PublishedDate if they were not passed or to maintain integrity
            if (!string.IsNullOrWhiteSpace(book.Title)) existingBook.Title = book.Title;
            if (!string.IsNullOrWhiteSpace(book.Author)) existingBook.Author = book.Author;
            if (book.PublishedDate != default) existingBook.PublishedDate = book.PublishedDate;

            return existingBook;
        });

        return true;
    }

    public bool Delete(int id)
    {
        return _books.TryRemove(id, out _);
    }
}
