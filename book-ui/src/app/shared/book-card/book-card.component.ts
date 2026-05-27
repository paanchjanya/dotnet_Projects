import { Component, Input, Output, EventEmitter } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Book } from '../../core/models/book.model';

@Component({
  selector: 'app-book-card',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './book-card.component.html',
  styleUrl: './book-card.component.scss'
})
export class BookCardComponent {
  @Input() book!: Book;
  @Output() deleted = new EventEmitter<number>();

  showDeleteModal = false;

  openDeleteModal() {
    this.showDeleteModal = true;
  }

  cancelDelete() {
    this.showDeleteModal = false;
  }

  confirmDelete() {
    this.showDeleteModal = false;
    this.deleted.emit(this.book.id);
  }

  getGenreColor(genre: string): string {
    const map: Record<string, string> = {
      'Programming': '#6366f1',
      'Software Engineering': '#8b5cf6',
      'Computer Science': '#06b6d4',
      'Web Development': '#10b981',
      'Artificial Intelligence': '#f59e0b',
      'Business': '#ef4444',
      'Interview Prep': '#ec4899',
      'Science Fiction': '#3b82f6',
      'Self Help': '#84cc16',
    };
    return map[genre] || '#64748b';
  }
}