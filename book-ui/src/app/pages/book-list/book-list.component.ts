import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { BookService } from '../../core/services/book.service';
import { BookCardComponent } from '../../shared/book-card/book-card.component';
import { Book } from '../../core/models/book.model';

@Component({
  selector: 'app-book-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, BookCardComponent],
  templateUrl: './book-list.component.html',
  styleUrl: './book-list.component.scss'
})
export class BookListComponent implements OnInit {
  books: Book[] = [];
  filtered: Book[] = [];
  searchQuery = '';
  selectedGenre = '';
  loading = true;
  genres: string[] = [];

  constructor(
    private bookService: BookService,
    private cdr: ChangeDetectorRef   // 👈 Add this
  ) { }


  clearSearch() {
    this.searchQuery = '';
    this.applyFilter();
  }
  ngOnInit() {
    this.loadBooks();
  }

  loadBooks() {
    this.loading = true;
    this.bookService.getAll().subscribe({
      next: (data) => {
        this.books = data;
        this.filtered = data;
        this.genres = [...new Set(data.map(b => b.genre))];
        this.loading = false;
        this.cdr.detectChanges();   // 👈 Add this
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();   // 👈 Add this
      }
    });
  }

  applyFilter() {
    this.filtered = this.books.filter(b => {
      const matchSearch =
        b.title.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
        b.author.toLowerCase().includes(this.searchQuery.toLowerCase());
      const matchGenre = this.selectedGenre ? b.genre === this.selectedGenre : true;
      return matchSearch && matchGenre;
    });
    this.cdr.detectChanges();       // 👈 Add this too
  }

  onDelete(id: number | undefined) {
    if (!id) return;
    this.bookService.delete(id).subscribe(() => this.loadBooks());
  }
}