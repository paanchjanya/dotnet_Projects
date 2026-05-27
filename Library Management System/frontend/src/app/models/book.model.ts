export interface Book {
  id?: number;
  title: string;
  author: string;
  category: string;
  price: number;
  publishedDate: string; // "yyyy-MM-dd" format
  isAvailable: boolean;
}
