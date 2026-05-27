import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { BookService } from '../../services/book.service';
import { Book } from '../../models/book.model';

@Component({
  selector: 'app-book-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './book-list.component.html',
  styleUrls: ['./book-list.component.css']
})
export class BookListComponent implements OnInit {
  books: Book[] = [];
  allBooks: Book[] = [];
  searchQuery: string = '';
  isLoading: boolean = true;
  errorMessage: string = '';

  // Custom Delete Modal State
  showConfirmModal: boolean = false;
  selectedBookForDeletion: Book | null = null;

  constructor(private bookService: BookService) {}

  ngOnInit(): void {
    this.loadBooks();
  }

  loadBooks(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.bookService.getBooks().subscribe({
      next: (data) => {
        this.allBooks = data;
        this.filterBooks();
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading books:', err);
        this.errorMessage = 'Failed to load books. Please check if the .NET backend is running.';
        this.isLoading = false;
      }
    });
  }

  filterBooks(): void {
    if (!this.searchQuery.trim()) {
      this.books = [...this.allBooks];
      return;
    }
    const query = this.searchQuery.toLowerCase().trim();
    this.books = this.allBooks.filter(book => 
      book.title.toLowerCase().includes(query) ||
      book.author.toLowerCase().includes(query) ||
      book.category.toLowerCase().includes(query)
    );
  }

  clearSearch(): void {
    this.searchQuery = '';
    this.filterBooks();
  }

  openDeleteConfirmation(book: Book): void {
    this.selectedBookForDeletion = book;
    this.showConfirmModal = true;
  }

  closeDeleteConfirmation(): void {
    this.showConfirmModal = false;
    this.selectedBookForDeletion = null;
  }

  confirmDeletion(): void {
    if (this.selectedBookForDeletion && this.selectedBookForDeletion.id) {
      const id = this.selectedBookForDeletion.id;
      this.bookService.deleteBook(id).subscribe({
        next: () => {
          this.allBooks = this.allBooks.filter(b => b.id !== id);
          this.filterBooks();
          this.closeDeleteConfirmation();
        },
        error: (err) => {
          console.error('Error deleting book:', err);
          alert('Failed to delete book. Please try again.');
          this.closeDeleteConfirmation();
        }
      });
    }
  }
}
